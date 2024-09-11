[<AutoOpen>]
module LibLifeCycle.Services

open FSharp.Control
open System
open System.Threading.Tasks
open System.Collections.Generic

// Allow lifecycles to register custom singleton dependencies that can be injected into Environments
[<AttributeUsage(AttributeTargets.Class)>]
[<AllowNullLiteral>]
type RegisterSingletonDependencyAttribute() =
    inherit System.Attribute()

// Allow lifecycles to register custom scoped dependencies that can be injected into Environments
[<AttributeUsage(AttributeTargets.Class)>]
[<AllowNullLiteral>]
type RegisterScopedDependencyAttribute() =
    inherit System.Attribute()

type Request = interface end

type ResponseVerificationToken = private ReplyVerificationToken of Guid

type ResponseChannel<'Response> =
    abstract member Respond: value: 'Response -> ResponseVerificationToken

type MultiResponseChannel<'Response> =
    abstract member RespondNext: value: 'Response -> unit
    abstract member Complete: unit -> ResponseVerificationToken

type Service<'Request when 'Request :> Request> =
    abstract member Name:  string
    abstract member Query: buildRequest: (ResponseChannel<'Response> -> 'Request) -> Task<'Response>
    abstract member QueryAndReturnRequestResponse: buildRequest: (ResponseChannel<'Response> -> 'Request) -> 'Request * Task<'Response>

type MultiResponseService<'Request when 'Request :> Request> =
    abstract member Name:  string
    abstract member Query: buildRequest: (MultiResponseChannel<'Response> -> 'Request) -> IAsyncEnumerable<'Response>
    abstract member QueryAndReturnRequestResponse: buildRequest: (MultiResponseChannel<'Response> -> 'Request) -> 'Request * IAsyncEnumerable<'Response>

type ConnectorInterceptor<'Request when 'Request :> Request> =
    IServiceProvider -> 'Request -> (IServiceProvider -> 'Request -> Task<ResponseVerificationToken>) -> Task<ResponseVerificationToken>

and FullyTypedConnectorFunction<'Res> =
    abstract member Invoke: Connector<_, _> -> 'Res

and Connector =
    abstract member Name: string
    abstract member Invoke: FullyTypedConnectorFunction<'Res> -> 'Res

and
    [<CLIMutable>] // Temporary hack needed to register foreign ecosystem connector in the current ecosystem in the test environment
    Connector<'Request, 'Env when 'Request :> Request and 'Env :> Env> = {
    RequestProcessor: 'Env -> 'Request -> Task<ResponseVerificationToken>
    Name: string
    Interceptors: list<ConnectorInterceptor<'Request>>
    ShouldSendTelemetry: bool
} with
    member this.Request<'Response, 'SourceAction when 'SourceAction :> LifeAction>
        (buildRequest: (ResponseChannel<'Response> -> 'Request), buildAction: ('Response -> 'SourceAction)) : ExternalOperation<'SourceAction> =
        ExternalOperation.ExternalConnectorOperation(this.Name, (box buildRequest), (fun obj -> obj :?> 'Response |> buildAction), typeof<'Response>)

    member this.RequestMultiResponse<'Response, 'SourceAction when 'SourceAction :> LifeAction>
        (buildRequest: (MultiResponseChannel<'Response> -> 'Request), buildAction: ('Response -> 'SourceAction)) : ExternalOperation<'SourceAction> =
        ExternalOperation.ExternalConnectorMultiResponseOperation(this.Name, (box buildRequest), (fun obj -> obj :?> 'Response |> buildAction), typeof<'Response>)

    interface Connector with
        member this.Name = this.Name
        member this.Invoke (fn: FullyTypedConnectorFunction<_>) = fn.Invoke this

type private ResponseChannelImpl<'Response>() =
    let verificationToken = ReplyVerificationToken (Guid.NewGuid())
    let mutable actualResponse: Option<'Response> = None
with
    member _.VerificationToken = verificationToken
    member _.ActualResponse = actualResponse

    interface ResponseChannel<'Response> with
        member this.Respond (response: 'Response) =
            actualResponse <- Some response
            verificationToken

type private MultiResponseChannelImpl<'Response>() =
    let verificationToken = ReplyVerificationToken (Guid.NewGuid())
    let syncRoot = Object()
    let mutable isCompleted: bool = false
    // in case if ReadNext called before RespondNext (most of the time), it holds a single promise for MoveNext that happened before RespondNext
    let mutable pendingResponsePromise: Option<TaskCompletionSource<Option<'Response>>> = None
    // in case if RespondNext called before ReadNext (e.g. in test stubs), it holds ready to consume response values
    let producedResponses: Queue<Option<'Response>> = Queue<_>()
with
    member this.VerificationToken = verificationToken
    member this.IsCompleted = isCompleted

    member this.ReadNext() : Task<Option<'Response>> =
        lock syncRoot (fun () ->
            match producedResponses.TryDequeue() with
            | true, response ->
                Task.FromResult response
            | false, _ ->
                if isCompleted then
                    Task.FromResult None
                else
                    let promise = TaskCompletionSource<Option<'Response>>()
                    pendingResponsePromise <- Some promise
                    promise.Task
        )

    interface MultiResponseChannel<'Response> with
        member this.RespondNext (response: 'Response) =
            lock syncRoot (fun () ->
            if isCompleted then
                invalidOp $"{nameof(MultiResponseChannel)} is completed, no more values can be added."
            else
                let value = Some response
                match pendingResponsePromise with
                | Some promise ->
                    promise.SetResult value
                    pendingResponsePromise <- None
                | None ->
                    producedResponses.Enqueue value
        )

        member this.Complete() =
            lock syncRoot (fun () ->
                if isCompleted then
                    ()
                else
                    isCompleted <- true
                    // Signal completion if there's a pending MoveNextAsync call
                    match pendingResponsePromise with
                    | Some promise ->
                        promise.SetResult None
                        pendingResponsePromise <- None
                    | None ->
                        producedResponses.Enqueue None
                verificationToken
            )

let private queryAndReturnRequestResponse (opName: string) (buildRequest: (ResponseChannel<'Response> -> 'Request)) (serviceHandler: 'Request -> Task<ResponseVerificationToken>) : 'Request * Task<'Response> =
    let channel = ResponseChannelImpl<'Response>()
    let request = buildRequest channel

    request,
    backgroundTask {
        let! tokenToVerify = serviceHandler request

        if tokenToVerify <> channel.VerificationToken
        then
            failwithf "Unverified reply detected from service %s for request %A" opName request

        match channel.ActualResponse with
        | Some reply ->
            return reply
        | None ->
            return failwith "Reply corruption. This should never happen"
    }

let private queryAndReturnRequestMultiResponse (opName: string) (buildRequest: (MultiResponseChannel<'Response> -> 'Request)) (serviceHandler: 'Request -> Task<ResponseVerificationToken>) : 'Request * IAsyncEnumerable<'Response> =

    let channel = new MultiResponseChannelImpl<'Response>()
    let request = buildRequest channel

    // fire and forget serviceHandler, yuck
    // TODO: can we do it cleaner? Only if we introduce entirely new breed of connectors that expose IAsyncEnumerable at their interface i.e. trade Occam's razor for internal framework elegance
    let serviceHandlerTask : Task<ResponseVerificationToken> =
        backgroundTask {
            try
                let! tokenToVerify = serviceHandler request
                if tokenToVerify <> channel.VerificationToken then
                    failwithf $"Unverified reply detected from service %s{opName} for request %A{request}"
                if not channel.IsCompleted then
                    failwithf $"Verification token matched but channel is not completed, service %s{opName} for request %A{request}"
                return tokenToVerify
            finally
                 // always complete the channel if it's not yet e.g. due to exception, to avoid asyncSeq leak
                if not channel.IsCompleted then
                    (channel :> MultiResponseChannel<_>).Complete() |> ignore
        }

    request,
    AsyncSeq.unfoldAsync (
        fun _ ->
            async {
                match! channel.ReadNext() |> Async.AwaitTask with
                | Some response ->
                    return Some (response, Nothing)
                | None ->
                    if not serviceHandlerTask.IsCompleted then
                        failwithf $"Channel completed but serviceHandlerTask is not, service %s{opName} for request %A{request}"
                    return None
            }
        ) Nothing
    |> AsyncSeq.toAsyncEnum

let createService (name: string) (serviceHandler: 'Request -> Task<ResponseVerificationToken>) : Service<'Request> =
    { new Service<'Request> with
        member _.Name = name

        member _.QueryAndReturnRequestResponse<'Response> (buildRequest: (ResponseChannel<'Response> -> 'Request)) : 'Request * Task<'Response> =
            queryAndReturnRequestResponse name buildRequest serviceHandler

        member this.Query<'Response> (buildRequest: (ResponseChannel<'Response> -> 'Request)) : Task<'Response> =
            backgroundTask {
                let (_, responseTask) = this.QueryAndReturnRequestResponse buildRequest
                let! response = responseTask
                return response
            }
    }

let createMultiResponseService (name: string) (serviceHandler: 'Request -> Task<ResponseVerificationToken>) : MultiResponseService<'Request> =
    { new MultiResponseService<'Request> with
        member _.Name = name

        member _.QueryAndReturnRequestResponse<'Response> (buildRequest: (MultiResponseChannel<'Response> -> 'Request)) : 'Request * IAsyncEnumerable<'Response> =
            queryAndReturnRequestMultiResponse name buildRequest serviceHandler

        member this.Query<'Response> (buildRequest: (MultiResponseChannel<'Response> -> 'Request)) : IAsyncEnumerable<'Response> =
            let (_, enumerable) = this.QueryAndReturnRequestResponse buildRequest
            enumerable
    }

// Indicates a potentially blocking task that is unsafe to run within a lifecycle, but allowed in things like views
// FIXME, ideally we should flip things the other way around, Task<T> should be bindable within views, but only SafeTask<T>
// should be bindable within lifecycles
[<Struct>]
type BlockingTask<'T> = internal BlockingTask of Task<'T>
with
    static member ofResult result = BlockingTask result
    member internal this.Task = let (BlockingTask t) = this in t


let private runConnectorDirectly (connector: Connector<'Request, 'Env>) (env: 'Env) (requestBuilder: ResponseChannel<'Output> -> 'Request) : BlockingTask<'Output>  =
    let serviceHandler = connector.RequestProcessor env
    backgroundTask {
        let _, resTask = queryAndReturnRequestResponse connector.Name requestBuilder serviceHandler
        let! res = resTask
        return res
    } |> BlockingTask

// Allows slightly better handling of the response channel requirement
// Service.Query (fun reply -> Message(param1, param2, reply) now becomes
// Service.Query Message param1 param2 :=)
[<System.Runtime.CompilerServices.Extension>]
type ServiceQueryExtensions() =
    // 0 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member BlockingRequest (connector: Connector<'Request, 'Env>, request: (ResponseChannel<'Response> -> 'Request)) =
        fun (env: 'Env) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request responseChannel
            |> runConnectorDirectly connector env

    // 0 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: (ResponseChannel<'Response> -> 'Request)) =
        fun (responseChannel: ResponseChannel<'Response>) ->
            request(responseChannel)
        |> fun buildRequest buildAction -> connector.Request(buildRequest, buildAction)

    // 1 Param
    [<System.Runtime.CompilerServices.Extension>]
    static member Query (service: Service<'Request>, query: ('Param1 * ResponseChannel<'Response> -> 'Request)) =
        fun (param: 'Param1) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                query(param, responseChannel)
            |> service.Query

    // 1 Param
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * ResponseChannel<'Response> -> 'Request)) =
        fun (param: 'Param1) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param, responseChannel)
            |> fun buildRequest buildAction -> connector.Request(buildRequest, buildAction)

    // 1 Param
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * MultiResponseChannel<'Response> -> 'Request)) =
        fun (param: 'Param1) ->
            fun (responseChannel: MultiResponseChannel<'Response>) ->
                request(param, responseChannel)
            |> fun buildRequest buildAction -> connector.RequestMultiResponse(buildRequest, buildAction)

    // 1 Param
    [<System.Runtime.CompilerServices.Extension>]
    static member BlockingRequest (connector: Connector<'Request, 'Env>, request: ('Param1 * ResponseChannel<'Response> -> 'Request)) =
        fun (env: 'Env) (param: 'Param1) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param, responseChannel)
            |> runConnectorDirectly connector env

    // 2 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Query (service: Service<'Request>, query: ('Param1 * 'Param2 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                query(param1, param2, responseChannel)
            |> service.Query

    // 2 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, responseChannel)
            |> fun buildRequest buildAction -> connector.Request(buildRequest, buildAction)

    // 2 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * MultiResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) ->
            fun (responseChannel: MultiResponseChannel<'Response>) ->
                request(param1, param2, responseChannel)
            |> fun buildRequest buildAction -> connector.RequestMultiResponse(buildRequest, buildAction)

    // 2 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member BlockingRequest (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * ResponseChannel<'Response> -> 'Request)) =
        fun (env: 'Env) (param1: 'Param1) (param2: 'Param2) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, responseChannel)
            |> runConnectorDirectly connector env

    // 3 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Query (service: Service<'Request>, query: ('Param1 * 'Param2 * 'Param3 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                query(param1, param2, param3, responseChannel)
            |> service.Query

    // 3 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, responseChannel)
            |> fun buildRequest buildAction -> connector.Request(buildRequest, buildAction)

    // 3 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * MultiResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) ->
            fun (responseChannel: MultiResponseChannel<'Response>) ->
                request(param1, param2, param3, responseChannel)
            |> fun buildRequest buildAction -> connector.RequestMultiResponse(buildRequest, buildAction)

    // 3 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member BlockingRequest (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * ResponseChannel<'Response> -> 'Request)) =
        fun (env: 'Env) (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, responseChannel)
            |> runConnectorDirectly connector env

    // 4 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Query (service: Service<'Request>, query: ('Param1 * 'Param2 * 'Param3 * 'Param4 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                query(param1, param2, param3, param4, responseChannel)
            |> service.Query

    // 4 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, responseChannel)
            |> fun buildRequest buildAction -> connector.Request(buildRequest, buildAction)

    // 4 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * MultiResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) ->
            fun (responseChannel: MultiResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, responseChannel)
            |> fun buildRequest buildAction -> connector.RequestMultiResponse(buildRequest, buildAction)

    // 4 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member BlockingRequest (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * ResponseChannel<'Response> -> 'Request)) =
        fun ( env: 'Env) (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, responseChannel)
            |> runConnectorDirectly connector env

    // 5 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Query (service: Service<'Request>, query: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                query(param1, param2, param3, param4, param5, responseChannel)
            |> service.Query

    // 5 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, param5, responseChannel)
            |> fun buildRequest buildAction -> connector.Request(buildRequest, buildAction)

    // 5 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * MultiResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) ->
            fun (responseChannel: MultiResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, param5, responseChannel)
            |> fun buildRequest buildAction -> connector.RequestMultiResponse(buildRequest, buildAction)

    // 5 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member BlockingRequest (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * ResponseChannel<'Response> -> 'Request)) =
        fun (env: 'Env) (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, param5, responseChannel)
            |> runConnectorDirectly connector env

    // 6 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Query (service: Service<'Request>, query: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * 'Param6 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) (param6: 'Param6) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                query(param1, param2, param3, param4, param5, param6, responseChannel)
            |> service.Query

    // 6 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * 'Param6 * ResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) (param6: 'Param6) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, param5, param6, responseChannel)
            |> fun buildRequest buildAction -> connector.Request(buildRequest, buildAction)

    // 6 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member Request (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * 'Param6 * MultiResponseChannel<'Response> -> 'Request)) =
        fun (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) (param6: 'Param6) ->
            fun (responseChannel: MultiResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, param5, param6, responseChannel)
            |> fun buildRequest buildAction -> connector.RequestMultiResponse(buildRequest, buildAction)

    // 6 Params
    [<System.Runtime.CompilerServices.Extension>]
    static member BlockingRequest (connector: Connector<'Request, 'Env>, request: ('Param1 * 'Param2 * 'Param3 * 'Param4 * 'Param5 * 'Param6 * ResponseChannel<'Response> -> 'Request)) =
        fun (env: 'Env) (param1: 'Param1) (param2: 'Param2) (param3: 'Param3) (param4: 'Param4) (param5: 'Param5) (param6: 'Param6) ->
            fun (responseChannel: ResponseChannel<'Response>) ->
                request(param1, param2, param3, param4, param5, param6, responseChannel)
            |> runConnectorDirectly connector env
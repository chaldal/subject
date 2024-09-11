[<AutoOpen>]
module LibLifeCycle.ViewCE

open System.Threading.Tasks

type ViewCE() =
    member _.Bind(task': Task<'T>, binder: 'T -> ViewResult<'Output, 'OpError>) : ViewResult<'Output, 'OpError> =
        backgroundTask {
            let! result = task'
            let (ViewResult binderTask) = binder result
            return! binderTask
        }
        |> ViewResult

    member _.Bind(task': BlockingTask<'T>, binder: 'T -> ViewResult<'Output, 'OpError>) : ViewResult<'Output, 'OpError> =
        backgroundTask {
            let! result = task'.Task
            let (ViewResult binderTask) = binder result
            return! binderTask
        }
        |> ViewResult

    member _.Return<'Output, 'OpError when 'OpError :> OpError>(error: 'OpError) : ViewResult<'Output, 'OpError> =
        Error error |> Task.FromResult |> ViewResult

    member _.ReturnFrom(value: ViewResult<'Output, 'OpError>): ViewResult<'Output, 'OpError> = value

    member _.Zero() : ViewResult<unit, 'OpError> = () |> Ok |> Task.FromResult |> ViewResult

    member _.Combine (ViewResult res1: ViewResult<'Output1, 'OpError>, res2: unit -> ViewResult<'Output2, 'OpError>) : ViewResult<'Output2, 'OpError> =
        backgroundTask {
            match! res1 with
            | Ok _ ->
                let (ViewResult res2Task) = res2()
                match! res2Task with
                | Ok res ->
                    return Ok res
                | Error err ->
                    return Error err
            | Error err ->
                return Error err
        }
        |> ViewResult

    member _.Delay(value: unit -> ViewResult<'Output, 'OpError>) : unit -> ViewResult<'Output, 'OpError> = value

    member _.Run(value: unit -> ViewResult<'Output, 'OpError>) : ViewResult<'Output, 'OpError> = value()

    member this.TryWith(body: unit -> ViewResult<'Output, 'OpError>, handler: exn -> ViewResult<'Output, 'OpError>) : ViewResult<'Output, 'OpError> =
        try this.ReturnFrom(body())
        with e -> handler e

    member this.TryFinally(body: unit -> ViewResult<'Output, 'OpError>, compensation: unit -> unit) : ViewResult<'Output, 'OpError> =
        try this.ReturnFrom(body())
        finally compensation()

    member this.Using(disposable: #System.IDisposable, body: #System.IDisposable -> ViewResult<_, _>) : ViewResult<_, _> =
        let body' = fun () -> body disposable
        this.TryFinally(
            body',
            fun () ->
                match disposable with
                | null -> ()
                | disp -> disp.Dispose())

    // TODO: The specific Bind implementation cannot be resolved without typing body, which means overloading While for every Bind.
    // Also, a Zero and Combine implementation are both required. Zero kind of makes sense, but Combine...?
    //member this.While(guard: unit -> bool, body: unit -> Task) =
    //    if not (guard()) then
    //        this.Zero()
    //    else
    //        this.Bind(body(), fun () -> this.While(guard, body))

    // TODO: This requires + operator and Zero, but also requires Combine implementation :/
    //member inline _.While (guard: unit -> bool, body: unit -> 'T) : 'U =
    //    let rec loop guard body =
    //        if guard () then
    //            body () + loop guard body
    //        else
    //            LanguagePrimitives.GenericZero
    //    loop guard body

    // TODO: Relies on While
    //member this.For(sequence: seq<_>, body) =
    //    this.Using(sequence.GetEnumerator(), fun enum ->
    //        this.While(enum.MoveNext,
    //            this.Delay(fun () -> body enum.Current)))

let view = ViewCE()

[<AutoOpen>]
module ViewBuilderExtensions =
    type ViewCE with
        member _.Return(value: 'Output) : ViewResult<'Output, 'OpError> =
            Ok value |> Task.FromResult |> ViewResult
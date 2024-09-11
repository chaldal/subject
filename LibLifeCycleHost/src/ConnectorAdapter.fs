[<AutoOpen>]
module LibLifeCycleHost.ConnectorAdapter

open LibLifeCycle
open LibLifeCycleCore
open Orleans
open System
open System.Reflection

type IConnectorAdapter =
    abstract member RequestOnGrain: hostEcosystemGrainFactory: IGrainFactory -> responseType: Type -> buildRequest: obj (* ResponseChannel<'Response> -> Request  *) -> buildAction: (obj (* Request *) -> LifeAction) -> grainPartition: GrainPartition -> requestor: SubjectPKeyReference -> sideEffectId: GrainSideEffectId -> unit
    abstract member RequestMultiResponseOnGrain: hostEcosystemGrainFactory: IGrainFactory -> responseType: Type -> buildRequest: obj (* MultiResponseChannel<'Response> -> Request  *) -> buildAction: (obj (* Request *) -> LifeAction) -> grainPartition: GrainPartition -> requestor: SubjectPKeyReference -> sideEffectId: GrainSideEffectId -> unit
    abstract member ShouldSendTelemetry: bool

// Modules can't be typeof<>'ed. But we can get their type via an actual type contained within in
type private AnchorTypeForModule = private AnchorTypeForModule of unit

let private sendTypedRequest<'Request, 'Env, 'Response>
        (hostEcosystemGrainFactory: IGrainFactory)
        (buildRequest: obj (* ResponseChannel<'Response> -> 'Request  *))
        (buildAction: obj (* 'Request *) -> LifeAction)
        (GrainPartition grainPartitionGuid)
        (requestor: SubjectPKeyReference)
        (sideEffectId: GrainSideEffectId) : unit =
    let typedBuildRequest = buildRequest :?> (ResponseChannel<'Response> -> 'Request)
    let typedAction  = fun (resp: 'Response) -> resp |> box |> buildAction
    let connectorGrain = hostEcosystemGrainFactory.GetGrain<IConnectorGrain<'Request, 'Env>> grainPartitionGuid
    // Connector grains by their very nature are interacted with a fire-and-forget mechanism, so we don't need to wait for a response
    connectorGrain.InvokeOneWay(
        fun oneWayGrain ->
            oneWayGrain.SendRequest typedBuildRequest typedAction requestor sideEffectId
    )

let private sendTypedRequestMultiResponse<'Request, 'Env, 'Response>
        (hostEcosystemGrainFactory: IGrainFactory)
        (buildRequest: obj (* MultiResponseChannel<'Response> -> 'Request  *))
        (buildAction: obj (* 'Request *) -> LifeAction)
        (GrainPartition grainPartitionGuid)
        (requestor: SubjectPKeyReference)
        (sideEffectId: GrainSideEffectId) : unit =
    let typedBuildRequest = buildRequest :?> (MultiResponseChannel<'Response> -> 'Request)
    let typedAction  = fun (resp: 'Response) -> resp |> box |> buildAction
    let connectorGrain = hostEcosystemGrainFactory.GetGrain<IConnectorGrain<'Request, 'Env>> grainPartitionGuid
    // Connector grains by their very nature are interacted with a fire-and-forget mechanism, so we don't need to wait for a response
    connectorGrain.InvokeOneWay(
        fun oneWayGrain ->
            oneWayGrain.SendRequestMultiResponse typedBuildRequest typedAction requestor sideEffectId
    )

type ConnectorAdapter<'Request, 'Env when 'Request :> Request and 'Env :> Env> = {
    Connector: Connector<'Request, 'Env>
}
with
    interface IConnectorAdapter with
        member _.RequestOnGrain
                (hostEcosystemGrainFactory: IGrainFactory)
                (responseType: Type)
                (buildRequest: obj (* ResponseChannel<'Response> -> 'Request  *))
                (buildAction: obj (* 'Request *) -> LifeAction)
                (grainPartition: GrainPartition)
                (requestor: SubjectPKeyReference)
                (sideEffectId: GrainSideEffectId) : unit =
            typeof<AnchorTypeForModule>
                .DeclaringType // The Module
                .GetMethod((nameof sendTypedRequest), BindingFlags.NonPublic ||| BindingFlags.Static)
                .MakeGenericMethod([| typeof<'Request>; typeof<'Env>; responseType |])
                .Invoke(null, [| hostEcosystemGrainFactory; buildRequest; buildAction; grainPartition; requestor; sideEffectId |])
                :?> unit

        member _.RequestMultiResponseOnGrain
                (hostEcosystemGrainFactory: IGrainFactory)
                (responseType: Type)
                (buildRequest: obj (* MultiResponseChannel<'Response> -> 'Request  *))
                (buildAction: obj (* 'Request *) -> LifeAction)
                (grainPartition: GrainPartition)
                (requestor: SubjectPKeyReference)
                (sideEffectId: GrainSideEffectId) : unit =
            typeof<AnchorTypeForModule>
                .DeclaringType // The Module
                .GetMethod((nameof sendTypedRequestMultiResponse), BindingFlags.NonPublic ||| BindingFlags.Static)
                .MakeGenericMethod([| typeof<'Request>; typeof<'Env>; responseType |])
                .Invoke(null, [| hostEcosystemGrainFactory; buildRequest; buildAction; grainPartition; requestor; sideEffectId |])
                :?> unit

        member this.ShouldSendTelemetry = this.Connector.ShouldSendTelemetry

type ConnectorAdapterCollection = ConnectorAdapterCollection of Map<string, IConnectorAdapter>
with
    interface System.Collections.Generic.IEnumerable<IConnectorAdapter> with
        member this.GetEnumerator(): Collections.Generic.IEnumerator<IConnectorAdapter> =
            let (ConnectorAdapterCollection dictionary) = this
            dictionary.Values.GetEnumerator()

        member this.GetEnumerator(): Collections.IEnumerator =
            let (ConnectorAdapterCollection dictionary) = this
            dictionary.Values.GetEnumerator() :> Collections.IEnumerator

    member this.GetConnectorAdapterByName name : Option<IConnectorAdapter> =
        match this with
        | ConnectorAdapterCollection dictionary ->
            match dictionary.TryGetValue name with
            | true, adapter -> Some adapter
            | false, _ -> None

let makeConnectorAdapter (connector: Connector) =
    connector.Invoke
        { new FullyTypedConnectorFunction<_> with
            member _.Invoke (connector: Connector<'Request, 'Env>) = { Connector = connector } :> IConnectorAdapter }
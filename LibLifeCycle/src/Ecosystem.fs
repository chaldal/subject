namespace LibLifeCycle

open LibLifeCycle

type Ecosystem = {
    Name:                 string
    // indexed by key instead of local name for co-hosting referenced LC implementations in tests
    LifeCycles:           Map<LifeCycleKey, ILifeCycle>
    Views:                Map<string, IView>
    TimeSeries:           Map<TimeSeriesKey, ITimeSeries>
    Connectors:           Map<string, Connector>
    ReferencedEcosystems: Map<string, ReferencedEcosystem>
    EnforceRateLimits:    bool
    Def:                  EcosystemDef
}
with
    member this.LifeCycleDefs = this.LifeCycles |> Map.values |> Seq.map (fun lc -> lc.Def) |> List.ofSeq
    member this.ViewDefs = this.Views |> Map.values |> Seq.map (fun v -> v.Def) |> List.ofSeq

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Ecosystem =
    let addConnectorInterceptor
            (connector: Connector<'Request, 'Env>)
            (interceptor: ConnectorInterceptor<'Request>)
            (ecosystem: Ecosystem) : Ecosystem =
        match ecosystem.Connectors.TryFind connector.Name with
        | Some _ ->
            // mutate the instance of connector instead of copy-on-update because views run blocking
            // connector requests by original connector reference not by name (unlike LifeCycles) and
            // would ignore the interceptors otherwise. Can we do better?
            connector.GetType()
                .GetProperty(nameof(connector.Interceptors)).GetSetMethod()
                .Invoke(connector, [| interceptor :: connector.Interceptors |])
                |> ignore

            { ecosystem with Connectors = ecosystem.Connectors.Add(connector.Name, connector) }
        | None ->
            failwithf "Connector with Name %s is not yet registered. Interceptors can only be added after connector registration" connector.Name

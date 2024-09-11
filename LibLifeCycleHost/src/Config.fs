[<AutoOpen>]
module LibLifeCycleHost.Config

type AnchorTypeForProject = private AnchorTypeForProject of unit

open LibLifeCycle.Config

type [<CLIMutable>] OrleansClusteringConfiguration = {
    MembershipConnectionString: string
    ShouldInitializeForLocal1NodeEnvironment: bool
    DevHostSiloPort: int
    DevHostGatewayPort: int
}
with
    interface IValidatable with
        member this.Validate(): unit =
            if System.String.IsNullOrWhiteSpace(this.MembershipConnectionString) then
                ConfigurationValidationException("MembershipConnectionString not specified", this.MembershipConnectionString) |> raise

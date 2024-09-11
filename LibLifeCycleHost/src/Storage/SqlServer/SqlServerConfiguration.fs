namespace LibLifeCycleHost.Storage.SqlServer

open LibLifeCycle.Config

[<CLIMutable>]
type SqlServerConfiguration = {
    ConnectionString: string
}
with
    interface IValidatable with
        member this.Validate(): unit =
            if System.String.IsNullOrWhiteSpace(this.ConnectionString) then
                ConfigurationValidationException("ConnectionString not specified", this.ConnectionString) |> raise

type SqlServerConnectionStrings = {
    // typically contains one value, can be many for test data seeding within biosphere
    ByEcosystemName: NonemptyMap<string, string>
}
with
    member this.ForEcosystem (name: string) =
        match this.ByEcosystemName.TryFind name with
        | Some connStr -> connStr
        | None -> failwithf "Connection string not found for ecosystem: %s" name
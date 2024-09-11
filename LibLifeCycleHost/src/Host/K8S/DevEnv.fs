namespace LibLifeCycleHost.Host.K8S

open System
open LibLifeCycle
open Microsoft.Extensions.Configuration

module DevEnv =
    /// Checks if DevEnv mode enabled for a K8S launcher
    /// (not to be confused with DevelopmentHost / DevHost which is an entirely separate launcher)
    let isDevEnvEnabled() =
        match
            System.Environment.GetEnvironmentVariable "DevEnv.Enabled"
            |> System.Boolean.TryParse
        with
        | false, _ -> false
        | true, enabled -> enabled

    let applyDevEnvConfigToEcosystem (ecosystem: Ecosystem) (config: IConfiguration) =
        let devEnvEnabled = isDevEnvEnabled()

        if devEnvEnabled then
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development")

        match
            devEnvEnabled,
            config.GetSection("DevEnv.UnrestrictedApiAccess").Get<bool>()
            with
        | false, _
        | true, false ->
            ecosystem
        | true, true ->
            { Name = ecosystem.Name
              Def = ecosystem.Def
              Connectors = ecosystem.Connectors
              EnforceRateLimits = ecosystem.EnforceRateLimits
              LifeCycles =
                  ecosystem.LifeCycles
                  |> Map.map (fun _ lc ->
                      lc.Invoke
                          { new FullyTypedLifeCycleFunction<_> with
                              member _.Invoke lifeCycle =
                                  { lifeCycle with
                                      MaybeApiAccess =
                                          { AccessRules = [{ Input = MatchAny; Roles = MatchAny; EventTypes = MatchAny; Decision = Grant }]
                                            AccessPredicate = fun _ _ _ _ -> true
                                            RateLimitsPredicate = fun _ -> Some [] // no api limits, should be a separate config key?
                                            AnonymousCanReadTotalCount = true }
                                          |> Some }
                                  :> ILifeCycle })
              Views =
                  ecosystem.Views
                  |> Map.map (fun _ v ->
                      v.Invoke
                          { new FullyTypedViewFunction<_> with
                              member _.Invoke view =
                                  { view with
                                      MaybeApiAccess =
                                          { ViewApiAccess.AccessRules = [{ Input = MatchAny; Roles = MatchAny; Decision = Grant }]
                                            AccessPredicate = fun _ _ _ _ -> true }
                                          |> Some }
                                  :> IView })

              TimeSeries =
                  ecosystem.TimeSeries
                  |> Map.map (fun _ timeSeries ->
                      timeSeries.Invoke
                        { new FullyTypedTimeSeriesFunction<_> with
                            member _.Invoke timeSeries =
                               { timeSeries with
                                    MaybeApiAccess =
                                        { TimeSeriesApiAccess.AccessRules = [{ Input = MatchAny; EventTypes = MatchAny; Roles = MatchAny; Decision = Grant }]
                                          AccessPredicate = fun _ _ _ _ -> true }
                                        |> Some }
                               :> ITimeSeries })

              ReferencedEcosystems =
                  ecosystem.ReferencedEcosystems
                  |> Map.map (fun _ referencedEcosystem ->
                      { Def = referencedEcosystem.Def
                        LifeCycles =
                            referencedEcosystem.LifeCycles
                            |> List.map (fun lc ->
                                lc.Invoke
                                    { new FullyTypedReferencedLifeCycleFunction<_> with
                                        member _.Invoke lifeCycle =
                                            { lifeCycle with
                                                MaybeApiAccess =
                                                    { AccessRules = [{ Input = MatchAny; Roles = MatchAny; EventTypes = MatchAny; Decision = Grant }]
                                                      AccessPredicate = fun _ _ _ _ -> true
                                                      RateLimitsPredicate = fun _ -> Some [] // no api limits, should be a separate config key?
                                                      AnonymousCanReadTotalCount = true }
                                                    |> Some }
                                            :> IReferencedLifeCycle })
                        Views =
                            referencedEcosystem.Views
                            |> List.map (fun v ->
                                v.Invoke
                                    { new FullyTypedReferencedViewFunction<_> with
                                        member _.Invoke view =
                                            { view with
                                                MaybeApiAccess =
                                                    { AccessRules = [{ Input = MatchAny; Roles = MatchAny; Decision = Grant }]
                                                      AccessPredicate = fun _ _ _ _ -> true }
                                                    |> Some }
                                            :> IReferencedView }) }) }

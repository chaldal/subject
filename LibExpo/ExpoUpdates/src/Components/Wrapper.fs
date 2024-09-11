[<AutoOpen>]
module ExpoUpdates.Components.Wrapper

#if !EGGSHELL_PLATFORM_IS_WEB
open Fable.Core
open Fable.React
open LibClient
open ExpoUpdates.Bindings

type ExpoUpdates with
    [<Component>]
    static member Wrapper (children: ReactElement, ?reloadOnUpdateDownload: bool) : ReactElement =
        let reloadOnUpdateDownload = defaultArg reloadOnUpdateDownload false

        Hooks.useEffect (
            (fun () ->
                promise {
                    try
                        match! checkUpdate() with
                        | UpdateCheckResult.Available updateBundlePath ->
                            do! fetchUpdate()
                            Log.Info $"ExpoUpdates: Update downloaded. UpdateBundlePath: {updateBundlePath}"
                            if reloadOnUpdateDownload then
                                do! reload()
                        | UpdateCheckResult.RollBack ->
                            do! fetchUpdate()
                            Log.Info "ExpoUpdates: RollBack Update downloaded"
                            if reloadOnUpdateDownload then
                                do! reload()
                        | UpdateCheckResult.NotAvailable reason ->
                            JS.console.log $"ExpoUpdates: Update not available. reason: {reason}"
                    with
                    | ex ->
                        Log.Error $"ExpoUpdates: Failed to check/fetch update. message: {ex.Message}"
                }
                |> Async.AwaitPromise
                |> startSafely
            ),
            [||]
        )

        children
#endif

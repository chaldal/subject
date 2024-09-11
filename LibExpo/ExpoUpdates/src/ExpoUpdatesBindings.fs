module ExpoUpdates.Bindings

#if !EGGSHELL_PLATFORM_IS_WEB
open Fable.Core
open Fable.Core.JsInterop

type UpdateCheckResult =
| Available    of UpdateBundlePath: string
| RollBack
| NotAvailable of Reason: string

type UseUpdatesReturnType = {
    isChecking:        bool
    isDownloading:     bool
    isUpdatePending:   bool
    isUpdateAvailable: bool
}

let private expoUpdatesRaw: obj = import "*" "expo-updates"

let checkUpdate () : JS.Promise<UpdateCheckResult> =
    expoUpdatesRaw?checkForUpdateAsync()
    |> Promise.map (fun result ->
        if result?isAvailable then
            let updateBundlePath: string = result?manifest?extra?updateBundlePath
            UpdateCheckResult.Available updateBundlePath
        elif result?isRollBackToEmbedded then
            UpdateCheckResult.RollBack
        else
            UpdateCheckResult.NotAvailable result?reason
    )

let fetchUpdate () : JS.Promise<unit> =
    expoUpdatesRaw?fetchUpdateAsync()

let reload () : JS.Promise<unit> =
    expoUpdatesRaw?reloadAsync()

let useUpdates () : UseUpdatesReturnType = expoUpdatesRaw?useUpdates()

#endif

module ThirdParty.ReactNativeCodePush.CodePush

open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop

#if !EGGSHELL_PLATFORM_IS_WEB
let private reactNativeCodePush: obj = importDefault "react-native-code-push"

type CheckFrequency =
| ON_APP_START
| ON_APP_RESUME
| MANUAL
with
    member this.toJS() =
        match this with
        | ON_APP_START  -> 0
        | ON_APP_RESUME -> 1
        | MANUAL        -> 2

type UpdateState =
| RUNNING
| PENDING
| LATEST
with
    member this.toJS() =
        match this with
        | RUNNING -> 0
        | PENDING -> 1
        | LATEST  -> 2

type InstallMode =
| IMMEDIATE
| ON_NEXT_RESTART
| ON_NEXT_RESUME
| ON_NEXT_SUSPEND
    member this.toJS() =
        match this with
        | IMMEDIATE       -> 0
        | ON_NEXT_RESTART -> 1
        | ON_NEXT_RESUME  -> 2
        | ON_NEXT_SUSPEND -> 3

type SyncStatus =
| CHECKING_FOR_UPDATE
| AWAITING_USER_ACTION
| DOWNLOADING_PACKAGE
| INSTALLING_UPDATE
| UP_TO_DATE
| UPDATE_IGNORED
| UPDATE_INSTALLED
| SYNC_IN_PROGRESS
| UNKNOWN_ERROR
with
    member this.toJS(): int =
        match this with
        | CHECKING_FOR_UPDATE -> 0
        | AWAITING_USER_ACTION -> 1
        | DOWNLOADING_PACKAGE -> 2
        | INSTALLING_UPDATE  -> 3
        | UP_TO_DATE -> 4
        | UPDATE_IGNORED -> 5
        | UPDATE_INSTALLED -> 6
        | SYNC_IN_PROGRESS -> 7
        | UNKNOWN_ERROR -> -1

    static member fromJS(statusNumber: int): SyncStatus =
        match statusNumber with
        | 0 -> CHECKING_FOR_UPDATE
        | 1 -> AWAITING_USER_ACTION
        | 2 -> DOWNLOADING_PACKAGE
        | 3 -> INSTALLING_UPDATE
        | 4 -> UP_TO_DATE
        | 5 -> UPDATE_IGNORED
        | 6 -> UPDATE_INSTALLED
        | 7 -> SYNC_IN_PROGRESS
        | _ -> UNKNOWN_ERROR

type CodePushOptions = {
    checkFrequency: Option<CheckFrequency>
    mandatoryInstallMode: Option<InstallMode>
}
with
    member this.toJS() =
        createObj [
            if this.checkFrequency.IsSome then
                "checkFrequency" ==> this.checkFrequency.Value.toJS()
            if this.mandatoryInstallMode.IsSome then
                "mandatoryInstallMode" ==> this.mandatoryInstallMode.Value.toJS()
        ]

type LocalPackage = {
    appVersion:    string
    deploymentKey: string
    description:   string
    failedInstall: bool
    isFirstRun:    bool
    isMandatory:   bool
    isPending:     bool
    label:         string
    packageHash:   int64

    install:       int -> Promise<unit>
}

type DownloadProgress = {
    totalBytes:    float
    receivedBytes: float
}

type RemotePackage = {
    appVersion:    string
    deploymentKey: string
    description:   string
    failedInstall: bool
    isFirstRun:    bool
    isMandatory:   bool
    isPending:     bool
    label:         string
    packageHash:   int64

    downloadUrl:   string
    download:      (DownloadProgress->Unit) -> Promise<LocalPackage>
}

type SyncOptions = {
    installMode: InstallMode
    mandatoryInstallMode: InstallMode

    //TODO - Add followings when needed
    //updateDialog
    //rollbackRetryOptions
}
with
    member this.toJS() =
        createObj [
            "installMode"          ==> this.installMode.toJS()
            "mandatoryInstallMode" ==> this.mandatoryInstallMode.toJS()
        ]

// What you pass in to this function is the constructor of your EggShellReact subclass component.
// e.g. if you want to wrap the AppExample.Components.AppContext component, you say:
// let codePushWrappedAppContextClass: obj = wrapInCodePush AppExample.Components.AppContext.AppContext
// and then to construct an actual instance of it, you need to call the raw react createElement function,
// like this:
// Fable.React.ReactBindings.React.createElement (codePushWrappedAppContextClass, appContextProps, [rootView])
// You will need to externally construct the props object, specifying its type explicitly, like this:
// let appContextProps: AppContext.Props = {key = None; children = noElement}
let wrapInCodePush: obj -> obj = reactNativeCodePush :?> (obj -> obj)

let wrapInCodePushWithOption : CodePushOptions-> obj -> obj = fun (codePushOptions: CodePushOptions) ->
    let options = codePushOptions.toJS()
    wrapInCodePush options :?> (obj -> obj)

let sync(options: SyncOptions, maybeSyncStatusChangeCallback: Option<SyncStatus->unit>, maybeDownloadProgressCallback: Option<DownloadProgress -> unit>): Async<SyncStatus> =
    let syncStatusChangeCallback =
        maybeSyncStatusChangeCallback
        |> Option.map (fun callback -> SyncStatus.fromJS >> callback)
        |> Option.defaultValue (fun _ -> ())

    let downloadProgressCallback =
        maybeDownloadProgressCallback
        |> Option.defaultValue (fun _ -> ())

    async {
        let! syncStatusNumber = reactNativeCodePush?sync (options.toJS()) syncStatusChangeCallback downloadProgressCallback |> Async.AwaitPromise
        return SyncStatus.fromJS syncStatusNumber
    }

let getUpdateMetadataByState (updateState: UpdateState) : Async<Option<LocalPackage>> =
    async {
        let! metaData = reactNativeCodePush?getUpdateMetadata(updateState.toJS()) |> Async.AwaitPromise
        let metaDataNullable = metaData :> obj :?> LibClient.JsInterop.JsNullable<LocalPackage>
        return metaDataNullable.ToOption
    }

let getUpdateMetadata () : Async<Option<LocalPackage>> =
    async {
        let! metaData = reactNativeCodePush?getUpdateMetadata() |> Async.AwaitPromise
        let metaDataNullable = metaData :> obj :?> LibClient.JsInterop.JsNullable<LocalPackage>
        return metaDataNullable.ToOption
    }

let checkForUpdate (deploymentKey: Option<string>) : Async<Option<RemotePackage>> =
    async {
        let! remotePackage = reactNativeCodePush?checkForUpdate(deploymentKey) |> Async.AwaitPromise
        let remotePackageNullable = remotePackage :> obj :?> LibClient.JsInterop.JsNullable<RemotePackage>
        return remotePackageNullable.ToOption
    }

let restartApp (onlyIfUpdateIsPending: bool) : unit =
    reactNativeCodePush?restartApp(onlyIfUpdateIsPending) |> ignore

#endif
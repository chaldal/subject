[<AutoOpen>]
module LibAppUpdateManager.Components.CodePushUpdateManager

open Fable.React
open LibClient
open LibClient.Chars
open LibClient.Services.LocalStorageService
open LibRouter.RoutesSpec
open LibClient.Components
open ReactXP.Components
open ReactXP.Styles
open ThirdParty.ReactNativeCodePush
open LibAppUpdateManager.icons

#if !EGGSHELL_PLATFORM_IS_WEB
type private InitialRouteState =
| Loading
| Loaded of Option<Location>

type SyncState =
| InProgress of CodePush.SyncStatus
| Complete of CodePush.SyncStatus

type AppUpdateScreen =
| CheckingForUpdate
| DownloadInProgress of progress: Option<PositivePercentage>
| DownloadComplete
| NoUpdateAvailable of binaryUpdateUrl: Option<NonemptyString>

module private Styles =
    let background =
        makeViewStyles {
            backgroundColor Color.White
        }
        
    let view =
        makeViewStyles {
            marginTop 16
            FlexDirection.Row
            JustifyContent.Center
            AlignItems.Center
        }
        
    let buttons =
        makeViewStyles {
            maxWidth 150
            JustifyContent.Center
            AlignItems.Center
            AlignSelf.Center
        }
        
    let primaryContent = makeViewStyles {
        FlexDirection.Column
        AlignItems.Center
    }
    
    let newUpdateVersionText = makeTextStyles {
        fontSize 12
    }
    
    let syncStateText = makeTextStyles {
        fontSize 8
        color (Color.Grey "cc")
    }
    
    let updateScreenProgressBar = makeViewStyles {
        marginTop 5
        marginBottom 15
        FlexDirection.Row
    }
    
    let progressBarPercentage = makeTextStyles {
        color Color.Black
        fontSize 16
    }
        
    let progressBarLayoutProvider = makeViewStyles {
        flex 1
    }

    let progressBarContainer = ViewStyles.Memoize (
        fun layoutWidth -> makeViewStyles {
            backgroundColor (Color.Grey "cc")
            borderRadius 20
            height 15
            width layoutWidth
            Overflow.Hidden
            JustifyContent.Center
        }
    )
    
    let progressBar = ViewStyles.Memoize (
        fun (layoutWidth: int)-> makeViewStyles {
            backgroundColor (Color.Hex "#ff3458")
            borderRadius 20
            height 15
            width layoutWidth
        }
    )
    
    let updateScreenContainer = makeViewStyles {
        flex 1
        paddingHorizontal 60
        paddingVertical 20
        AlignItems.Center
        JustifyContent.SpaceBetween
        FlexDirection.Column
    }
    
    let updateScreenTopContent = makeViewStyles {
        AlignItems.Center
    }
    
    let iconContainer = makeViewStyles {
        marginTop 40
        marginBottom 70
    }
    
    let updateScreenIcon = makeTextStyles {
        fontSize 130
        color Color.Black
    }
    
    let titleContainer = makeViewStyles {
        AlignItems.Center
        maxWidth 250
    }
    
    let updateScreenTitle = makeTextStyles {
        marginBottom 30
        fontSize 22
        color Color.Black
    }
    
    let updateScreenDescription = makeTextStyles {
        fontSize 12
        TextAlign.Center
        color Color.Black
    }
    
    let updateScreenFootNoteContainer = makeViewStyles {
        AlignItems.Center
    }
    
    let updateScreenFootNote = makeTextStyles {
        fontSize 12
        TextAlign.Center
    }
    
    let currentAppVersion = makeTextStyles {
        fontSize 12
        marginTop 20
    }
    
    let error = makeViewStyles {
        AlignSelf.Center
        flex      1
        padding   0
        minHeight 200
    }
    
type Helpers =
    [<Component>]
    static member progressbar (progressPercentage: Percentage.PositivePercentage) =
        LC.With.Layout (fun (onLayoutOption, maybeLayout) ->
            RX.View (styles=[|Styles.progressBarLayoutProvider|], ?onLayout = onLayoutOption, children = [|
                maybeLayout
                |> Option.map(fun layout->
                    RX.View(styles = [|Styles.progressBarContainer layout.Width|], children = [|
                        match UnsignedDecimal.ofDecimal (decimal layout.Width) with
                        | Some widthUnsignedDecimal ->
                            let currentProgress = progressPercentage.PercentOf widthUnsignedDecimal
                            RX.View(styles = [|Styles.progressBar (int currentProgress.Value)|])
                        | None -> noElement
                    |])
                )
                |> Option.defaultValue (RX.View(children=[|
                    RX.Text "No Layout"
                |]))
            |])
        )
        
    [<Component>]
    static member updateScreen (screenState: AppUpdateScreen, maybeSyncStatus: Option<CodePush.SyncStatus>, maybeRunningAppVersion: Option<string>, maybeLatestAppVersion: Option<string>) =
        
        let icon =
            match screenState with
            | CheckingForUpdate    -> Icon.CloudLoading
            | DownloadInProgress _ -> Icon.CloudSync
            | DownloadComplete     -> Icon.CloudComplete
            | NoUpdateAvailable _  -> Icon.AppDownload
            
        let title =
            match screenState with
            | CheckingForUpdate    -> "Checking for update"
            | DownloadInProgress _ -> "Downloading update"
            | DownloadComplete     -> "Update Complete"
            | NoUpdateAvailable  _ -> "Download update"
            
        let description =
            match screenState with
            | CheckingForUpdate    -> "Please wait while we check for new app update"
            | DownloadInProgress _ -> "Please wait while we download the app update"
            | DownloadComplete     -> "Update complete. We will take you to the app now"
            | NoUpdateAvailable  _ -> "Please update the latest version of the app from the store"
            
        let footnote =
            match screenState with
            | NoUpdateAvailable _ -> ""
            | _ -> "You will be able to continue using the app as soon as the app update is complete"

            
        let activityIndicator = RX.ActivityIndicator(color = "#ff3458", size = Size.Large)
        let latestAppVersion =
            maybeLatestAppVersion |> Option.mapOrElse "" (fun appVersion -> appVersion)
            
        let runningAppVersion =
            maybeRunningAppVersion |> Option.mapOrElse "" (fun appVersion -> appVersion)
            
        let mainContent =
            match screenState with
            | CheckingForUpdate -> activityIndicator
            | DownloadInProgress maybePercentage ->
                match maybePercentage with
                | None -> activityIndicator
                | Some percentage -> element {
                    RX.Text (latestAppVersion, styles = [|Styles.newUpdateVersionText|])
                    RX.View ( styles = [|Styles.updateScreenProgressBar|], children = [|
                        Helpers.progressbar(percentage)    
                    |])
                    RX.Text ($"{(int percentage.Value).ToString()}{Char.percent}", styles = [|Styles.progressBarPercentage|])
                }
            | DownloadComplete  -> noElement
            | NoUpdateAvailable maybeUpdateUrl->
                match maybeUpdateUrl with
                | Some updateUrl ->
                    LC.Button (label = "Update app", state = ButtonHighLevelStateFactory.MakeLowLevel (ButtonLowLevelState.Actionable (fun _ -> (ReactXP.Linking.openUrl(updateUrl.Value)))))
                | None -> noElement

        
        RX.View( styles = [|Styles.updateScreenContainer; Styles.background|], children=[|
            // icon
            RX.View( styles=[|Styles.updateScreenTopContent|], children=[|
                RX.View( styles = [|Styles.iconContainer|], children=[|
                    LC.Icon(icon, styles=[|Styles.updateScreenIcon|])
                |])

                // Title
                RX.View( styles = [|Styles.titleContainer|], children=[|
                    RX.Text(title, styles=[|Styles.updateScreenTitle|])
                    RX.Text(description, styles=[|Styles.updateScreenDescription|])
                |])
            |])
            
            // ProgressBar
            RX.View( styles = [|Styles.primaryContent|], children=[|
                mainContent
            |])
            
            // footnote
            RX.View(styles = [|Styles.updateScreenFootNoteContainer|], children=[|
                RX.Text(footnote, styles = [|Styles.updateScreenFootNote|])
                
                RX.Text (runningAppVersion, styles=[|Styles.currentAppVersion|])
                match maybeSyncStatus with
                | Some syncStatus -> RX.Text (syncStatus.ToString(), styles = [|Styles.syncStateText|])
                | None -> noElement
            |])
                    
        |])

    
    [<Component>]
    static member UpdateUI (maybeSyncState: Option<SyncState>, maybeDownloadProgress: Option<CodePush.DownloadProgress>, isPendingUpdateAvailable: bool, ?binaryUpdateUrl: NonemptyString) =
        let maybeRunningMetaData = Hooks.useState(None)
        let maybeLatestMetaData  = Hooks.useState(None)
        
        let refreshMetaData () =
            async {
                let! runningUpdate = CodePush.getUpdateMetadata()
                maybeRunningMetaData.update(runningUpdate)
                
                let! latestUpdate  = CodePush.checkForUpdate(None)
                maybeLatestMetaData.update(latestUpdate)
            }
            |> startSafely
        
        Hooks.useEffect(
            ( fun _ ->
                refreshMetaData ()
                if isPendingUpdateAvailable then
                    CodePush.restartApp (*onlyRestartIfUpdatePending*) false
            ), [|isPendingUpdateAvailable|])
        
        let currentScreen =
            match maybeSyncState with
            | Some runningSyncState ->
                match runningSyncState with
                | SyncState.InProgress syncStatus ->
                    match syncStatus with
                    | CodePush.CHECKING_FOR_UPDATE ->
                        AppUpdateScreen.CheckingForUpdate
                    | CodePush.SYNC_IN_PROGRESS ->
                        maybeDownloadProgress
                        |> Option.mapOrElse None (fun downloadProgress ->
                            (decimal downloadProgress.receivedBytes / decimal downloadProgress.totalBytes) * 100m
                            |> PositivePercentage.ofDecimal
                        )
                        |> AppUpdateScreen.DownloadInProgress 
                    | CodePush.INSTALLING_UPDATE ->
                        AppUpdateScreen.DownloadInProgress None
                    | CodePush.DOWNLOADING_PACKAGE ->
                        AppUpdateScreen.DownloadInProgress None
                    | CodePush.AWAITING_USER_ACTION ->
                        AppUpdateScreen.DownloadComplete
                    | _ ->
                        AppUpdateScreen.CheckingForUpdate
                | SyncState.Complete completeSyncState ->
                    match completeSyncState with
                    | CodePush.AWAITING_USER_ACTION ->
                        AppUpdateScreen.DownloadComplete
                    | CodePush.UP_TO_DATE
                    | CodePush.UPDATE_IGNORED
                    | CodePush.UPDATE_INSTALLED
                    | CodePush.INSTALLING_UPDATE
                    | CodePush.CHECKING_FOR_UPDATE
                    | CodePush.DOWNLOADING_PACKAGE
                    | CodePush.UNKNOWN_ERROR
                    | CodePush.SYNC_IN_PROGRESS ->
                        AppUpdateScreen.NoUpdateAvailable binaryUpdateUrl
            | None ->
                AppUpdateScreen.CheckingForUpdate
        
        let maybeSyncStatus     = maybeSyncState |> Option.map( fun syncState -> match syncState with | Complete status | InProgress status -> status )
        let maybeRunningVersion = maybeRunningMetaData.current |> Option.map (fun metaData -> metaData.label)
        let maybeLatestVersion  = maybeLatestMetaData.current |> Option.map (fun metaData -> metaData.label)
            
        Helpers.updateScreen (currentScreen, maybeSyncStatus, maybeRunningVersion, maybeLatestVersion)

    

type CodePushUpdateManager =
    [<Component>]
    static member Wrapper ( storageService: LocalStorageService, ``with``: (Option<Location>) -> (Option<Location->bool>) -> ReactElement, ?binaryUpdateUrl: NonemptyString) : ReactElement =
        let localStoragePendingRouteKey = "PendingNextRoute"
        let isPendingUpdateAvailable    = Hooks.useState(false)
        let maybeInitialRouteState      = Hooks.useState(InitialRouteState.Loading)
        
        let codePushSyncState           = Hooks.useState<Option<SyncState>>(None)
        let downloadProgress            = Hooks.useState(None)
                
        let restartAppIfPendingUpdate() =
            CodePush.restartApp (*onlyRestartIfUpdatePending*) true
        
        let checkPendingInitialRoute () =
            async {
                let! maybePendingRoute = storageService.Get localStoragePendingRouteKey Json.FromString<Location>
                maybeInitialRouteState.update (Loaded maybePendingRoute)
                do! storageService.Remove localStoragePendingRouteKey
            } |> startSafely
            
        let syncCodePush () =
            async {
                let syncOption: CodePush.SyncOptions = {
                    installMode = CodePush.InstallMode.ON_NEXT_SUSPEND
                    mandatoryInstallMode = CodePush.InstallMode.IMMEDIATE
                }
        
                let! syncState =
                    CodePush.sync (
                        syncOption,
                        Some (
                            fun syncStatus ->
                                codePushSyncState.update (Some (SyncState.InProgress syncStatus))
                        ),
                        Some (
                            fun progress ->
                                downloadProgress.update (Some progress)
                        )
                    )

                codePushSyncState.update (Some (SyncState.Complete syncState))
                
                match syncState with
                | CodePush.SyncStatus.AWAITING_USER_ACTION ->
                    isPendingUpdateAvailable.update true
                | _ -> Noop
            } |> startSafely

        let onNavigation = fun (location: Location) ->
            if isPendingUpdateAvailable.current then
                async {
                    do! storageService.Put localStoragePendingRouteKey location Json.ToString<Location> 
                    isPendingUpdateAvailable.update false
                    restartAppIfPendingUpdate ()
                } |> startSafely
                false
            else
                true
                
        Hooks.useEffect(fun () ->
            syncCodePush ()
            checkPendingInitialRoute ()
        , [||])

        element {
            LC.ErrorBoundary (
            ``try`` = (
                match maybeInitialRouteState.current with
                | InitialRouteState.Loading ->
                    RX.View (styles = [| Styles.view |], children=[|
                        RX.ActivityIndicator(color = "#aaaaaa", size = Size.Medium)
                    |])
                | InitialRouteState.Loaded maybeInitialRoute ->
                    ``with`` maybeInitialRoute (Some onNavigation)
            )
            ,
            catch =
                (fun (error: System.Exception, retry: unit -> unit) ->
                    match error with
                    | AsyncDataException AsyncDataFailure.NetworkFailure
                    | AsyncDataException (AsyncDataFailure.RequestFailure _) ->
                        LC.AppShell.TopLevelErrorMessage (retry = retry, error = error)
                    | _ ->
                        Log.Error ("CodePushManager error boundary: {Error}", error)
                        element {
                            Helpers.UpdateUI (codePushSyncState.current, downloadProgress.current, isPendingUpdateAvailable.current, ?binaryUpdateUrl = binaryUpdateUrl)
                        }
                )
            )
        }
        
    [<Component>]
    static member FallBackWrapper () =
        let fallbackCodePushSyncCall () =
            async {
                let syncOption: CodePush.SyncOptions = {
                    installMode = CodePush.InstallMode.IMMEDIATE
                    mandatoryInstallMode = CodePush.InstallMode.IMMEDIATE
                }
                
                return! CodePush.sync (syncOption, None, None) |> Async.Ignore
            } |> startSafely
        
        Hooks.useEffect (
            (fun _ ->
                fallbackCodePushSyncCall ()
            ), [||]
        )
        
        noElement

    
#endif
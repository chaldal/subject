module AppEggShellGallery.Bootstrap

open Fable.Core.JsInterop

open LibClient
open AppEggShellGallery.Components

open ReactXP.Components
open ReactXP.Helpers
open LibClient.Components

LibClient.Initialize.initialize (AppEggShellGallery.LocalImages.localImage)

let init(configRes: Result<AppEggShellGallery.Config, string>) =
    LibClient.ComponentRegistration.registerAllTheThings()
    LibUiAdmin.ComponentRegistration.registerAllTheThings()
    LibRouter.ComponentRegistration.registerAllTheThings()
    ThirdParty.Showdown.ComponentRegistration.registerAllTheThings()
    ThirdParty.Map.ComponentRegistration.registerAllTheThings()
    AppEggShellGallery.ComponentRegistration.registerAllTheThings()

    let element =
        match configRes with
        | Ok config ->
            ReactXP.Styles.Config.setIsDevMode config.InitializeReactXPInDevMode

            AppServices.initialize config
            AppEggShellGallery.Config.initialize config

            AppEggShellGallery.ComponentsTheme.applyTheme()

            let maybeTelemetrySink =
                config.MaybeAppInsightsConfig
                |> Option.map (fun appInsightsConfig -> new LibClient.ApplicationInsights.ApplicationInsightsSink(appInsightsConfig))

            let logSinks: seq<ILogSink> =
                seq {
                    ConsoleLogSink()

                    match maybeTelemetrySink with
                    | Some telemetrySink -> telemetrySink
                    | None -> ()
                }

            setLogLevel LogLevel.Info
            registerLogSinks logSinks

            // let initialPstoreData = serizlied data
            let initialPstoreData = Map.empty
            let pstore = InitializePersistentStore initialPstoreData

            let element = App.Make { PstoreKey = "app" } [||]

            ReactXPRaw?App?initialize ((* DEBUG *) config.InitializeReactXPInDebugMode, (* DEV *) config.InitializeReactXPInDevMode)
            ReactXPRaw?UserInterface?setContextWrapper (fun rootView ->
                Ui.AppContext (children = [|rootView|])
            )

            #if EGGSHELL_PLATFORM_IS_WEB
            let consoleTestBindings =
                createObj [
                    "app"                     ==> element
                    "pstore"                  ==> pstore
                    "setTapCaptureVisibility" ==> LibClient.Components.TapCaptureDebugVisibility.setVisibleForDebug
                ]
            Browser.Dom.window?eggshell?AppEggShellGallery?test <- consoleTestBindings
            #endif

            element

        | Error reason ->
            Log.Error ("App initialization failed because config construction failed: " + reason)
            ReactXPRaw?App?initialize((* DEBUG *) false, (* DEV *) false);

            LibClient.Components.AppShell.TopLevelErrorMessage.Make
                {
                    Error = exn reason
                    Retry = NoopFn // TODO this should be optional
                    key   = None
                }
                [|
                    RX.Text "Failed to initialize application config"
                    RX.Text reason
                |]

    async {
        do! LibClient.ServiceInstances.services().Image.WhenInitialized ()
        ReactXPRaw?UserInterface?setMainView element
    } |> startSafely

open Fable.Core
[<Global>]
let private eggshell: obj = jsNative
eggshell?AppEggShellGallery?configSourceOverrides
|> ConfigSource.Base.withOverrides
|> Config.tryOfSource
|> init

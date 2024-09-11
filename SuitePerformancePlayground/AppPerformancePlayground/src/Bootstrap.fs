module AppPerformancePlayground.Bootstrap

open Fable.Core.JsInterop

open Fable.React
open LibClient
open ReactXP.Components
open AppPerformancePlayground.Components
open AppPerformancePlayground.AppServices

open ReactXP.Helpers

LibClient.Initialize.initialize AppPerformancePlayground.LocalImages.localImage

[<Component>]
// passing "props" as an anonymous record is key for getting around some nonsense errors...
let private AppContextWrapInCodePushHelper (props: {|children: ReactChildrenProp|}) : ReactElement =
    Ui.AppContext props.children

let init(configRes: Result<AppPerformancePlayground.Config, string>) =
    LibClient.ComponentRegistration.registerAllTheThings()
    LibRouter.ComponentRegistration.registerAllTheThings()
    LibUiAdmin.ComponentRegistration.registerAllTheThings()
    LibUiSubject.ComponentRegistration.registerAllTheThings()
    LibUiSubjectAdmin.ComponentRegistration.registerAllTheThings()
    LibUiAuth.ComponentRegistration.registerAllTheThings()

    let element =
        match configRes with
        | Ok config ->
            ReactXP.Styles.Config.setIsDevMode config.InitializeReactXPInDevMode

            let maybeTelemetrySink =
                config.MaybeAppInsightsConfig
                |> Option.map (fun appInsightsConfig -> LibClient.ApplicationInsights.ApplicationInsightsSink(appInsightsConfig))

            let logSinks: seq<ILogSink> =
                seq {
                    ConsoleLogSink(
                        includeFormatting =
#if EGGSHELL_PLATFORM_IS_WEB
                            true
#else
                            false
#endif
                    )

                    match maybeTelemetrySink with
                    | Some telemetrySink -> telemetrySink
                    | None -> ()

                    match config.MaybeSeqConfig with
                    | Some seqConfig ->
                        new LibClient.Seq.SeqLogSink(seqConfig, fun () -> services().Http)
                    | None ->
                        ()
                }

            setLogLevel config.LogLevel
            registerLogSinks logSinks

            let telemetrySinks: seq<ITelemetrySink> =
                seq {
                    ConsoleTelemetrySink()

                    match maybeTelemetrySink with
                    | Some telemetrySink -> telemetrySink
                    | None -> ()
                }

            registerTelemetrySinks telemetrySinks

            AppServices.initialize config
            AppPerformancePlayground.Config.initialize config

            AppPerformancePlayground.ComponentsTheme.applyTheme()

            // let initialPstoreData = serizlied data
            let initialPstoreData = Map.empty
            let pstore = InitializePersistentStore initialPstoreData

            let mutable element = None
            AppPerformancePlayground.I18nGlobal.start ()
            element <- Ui.App.Root () |> Some

            ReactXPRaw?App?initialize ((* DEBUG *) config.InitializeReactXPInDebugMode, (* DEV *) config.InitializeReactXPInDevMode)

            #if EGGSHELL_PLATFORM_IS_WEB
            ReactXPRaw?UserInterface?setContextWrapper (fun (rootView: ReactElement) -> Ui.AppContext (castAsElements rootView))

            let consoleTestBindings =
                createObj [
                    "app"                     ==> element
                    "pstore"                  ==> pstore
                    "setTapCaptureVisibility" ==> LibClient.Components.TapCaptureDebugVisibility.setVisibleForDebug
                ]

            Browser.Dom.window?eggshell?AppPerformancePlayground?test <- consoleTestBindings
            #else
            pstore |> ignore

            ReactXPRaw?UserInterface?setContextWrapper (fun (rootView: ReactElement) ->
                let codepushWrapped = ThirdParty.ReactNativeCodePush.CodePush.wrapInCodePush AppContextWrapInCodePushHelper
                Fable.React.ReactBindings.React.createElement (codepushWrapped, {|children = rootView|}, [||])
            )
            #endif

            element.Value

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

        #if EGGSHELL_PLATFORM_IS_WEB
        Browser.Dom.document.getElementById("app-pre-bootstrap-loader").remove()
        #endif
    } |> startSafely

open Fable.Core
[<Global>]
let private eggshell: obj = jsNative
eggshell?AppPerformancePlayground?configSourceOverrides
|> ConfigSource.Base.withOverrides
|> Config.tryOfSource
|> init

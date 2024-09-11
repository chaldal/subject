namespace AppPerformancePlayground

open System

type ConfigSource = {
    AppUrlBase:                                    Option<string>
    BackendUrl:                                    Option<string>
    LogLevel:                                      Option<string>
    MaybeAppInsightsInstrumentationKey:            Option<string>
    MaybeAppInsightsCloudRole:                     Option<string>
    MaybeSeqEndpoint:                              Option<string>
    MaybeSeqApiKey:                                Option<string>
    MaybeSeqMaxBufferCapacity:                     Option<int>
    MaybeSeqMaxFlushDelay:                         Option<TimeSpan>
    MaybeSeqMaxFlushBufferCapacity:                Option<int>
    InitializeReactXPInDevMode:                    Option<string>
    InitializeReactXPInDebugMode:                  Option<string>
    MaybeInBundleImageServiceBaseUrl:              Option<string>
    MaybeInBundleStaticResourceUrlPattern:         Option<string>
    MaybeExternalImageServiceBaseUrl:              Option<string>
    MaybeExternalStaticResourceUrlPattern:         Option<string>
    MaybeInBundleResourceUrlHashedDirectoryPrefix: Option<string>
    OtpResendTimeoutSeconds:                       Option<string>
    MaybeSessionIdCookieDomain:                    Option<string>
} with
    static member Base : ConfigSource = {
        AppUrlBase                                    = None // TODO location.origin for web
        BackendUrl                                    = Some "http://localhost:5000"
        LogLevel                                      = None
        MaybeAppInsightsInstrumentationKey            = None
        MaybeAppInsightsCloudRole                     = Some "Sample.App.Web"
        MaybeSeqEndpoint                              = None
        MaybeSeqApiKey                                = None
        MaybeSeqMaxBufferCapacity                     = None
        MaybeSeqMaxFlushDelay                         = None
        MaybeSeqMaxFlushBufferCapacity                = None
        InitializeReactXPInDevMode                    = Some "false"
        InitializeReactXPInDebugMode                  = Some "false"
        MaybeInBundleImageServiceBaseUrl              = None
        MaybeInBundleStaticResourceUrlPattern         = None
        MaybeExternalImageServiceBaseUrl              = None
        MaybeExternalStaticResourceUrlPattern         = None
        MaybeInBundleResourceUrlHashedDirectoryPrefix = None
        OtpResendTimeoutSeconds                       = Some "30"
        MaybeSessionIdCookieDomain                    = None
    }

    member this.withOverrides (overrides: obj) : ConfigSource =
        LibClient.JsInterop.extendRecordWithObj overrides this

type Config = {
    AppUrlBase:                                    string
    BackendUrl:                                    string
    LogLevel:                                      LibClient.LogLevel
    MaybeAppInsightsConfig:                        Option<LibClient.ApplicationInsights.AppInsightsConfig>
    MaybeSeqConfig:                                Option<LibClient.Seq.SeqConfig>
    InitializeReactXPInDevMode:                    bool
    InitializeReactXPInDebugMode:                  bool
    MaybeInBundleImageServiceBaseUrl:              Option<string>
    MaybeInBundleStaticResourceUrlPattern:         Option<string>
    MaybeExternalImageServiceBaseUrl:              Option<string>
    MaybeExternalStaticResourceUrlPattern:         Option<string>
    MaybeInBundleResourceUrlHashedDirectoryPrefix: Option<string>
    OtpResendTimeoutSeconds:                       int
    MaybeSessionIdCookieDomain:                    Option<NonemptyString>
} with
    static member tryOfSource (source: ConfigSource) : Result<Config, string> =
        resultful {
            let! theAppUrlBase                   = source.AppUrlBase |> Result.ofOption "Missing AppUrlBase"
            let! theBackendUrl                   = source.BackendUrl |> Result.ofOption "Missing BackendUrl"
            let! theInitializeReactXPInDevMode   = source.InitializeReactXPInDevMode     |> Option.flatMap System.Boolean.ParseOption |> Result.ofOption "Missing InitializeReactXPInDevMode"
            let! theInitializeReactXPInDebugMode = source.InitializeReactXPInDebugMode   |> Option.flatMap System.Boolean.ParseOption |> Result.ofOption "Missing InitializeReactXPInDebugMode"
            let! theOtpResendTimeoutSeconds      = source.OtpResendTimeoutSeconds        |> Option.flatMap System.Int32.ParseOption   |> Result.ofOption "Missing OtpResendTimeoutSeconds"
            let  theMaybeSessionIdCookieDomain   = source.MaybeSessionIdCookieDomain     |> Option.flatMap NonemptyString.ofString

            let theLogLevel =
                source.LogLevel
                |> Option.flatMap LibClient.LogLevel.ParseOption
                |> Option.defaultValue (LibClient.LogLevel.Info)

            let maybeAppInsightsConfig: Option<LibClient.ApplicationInsights.AppInsightsConfig> =
                match (source.MaybeAppInsightsInstrumentationKey, source.MaybeAppInsightsCloudRole) with
                | (Some key, Some role) -> Some { InstrumentationKey = key; CloudRole = role }
                | _ -> None

            let maybeSeqConfig: Option<LibClient.Seq.SeqConfig> =
                match source.MaybeSeqEndpoint with
                | Some endpoint ->
                    Some
                        {
                            Endpoint = endpoint
                            MaybeApiKey = source.MaybeSeqApiKey
                            MaxBufferCapacity = source.MaybeSeqMaxBufferCapacity |> Option.defaultValue 10
                            MaxFlushDelay = source.MaybeSeqMaxFlushDelay |> Option.defaultValue (TimeSpan.FromSeconds(2.))
                            MaxFlushBufferCapacity = source.MaybeSeqMaxFlushBufferCapacity |> Option.defaultValue 1000
                        }
                | _ -> None

            return {
                AppUrlBase                                    = theAppUrlBase
                BackendUrl                                    = theBackendUrl
                LogLevel                                      = theLogLevel
                MaybeAppInsightsConfig                        = maybeAppInsightsConfig
                MaybeSeqConfig                                = maybeSeqConfig
                InitializeReactXPInDevMode                    = theInitializeReactXPInDevMode
                InitializeReactXPInDebugMode                  = theInitializeReactXPInDebugMode
                MaybeInBundleImageServiceBaseUrl              = source.MaybeInBundleImageServiceBaseUrl
                MaybeInBundleStaticResourceUrlPattern         = source.MaybeInBundleStaticResourceUrlPattern
                MaybeExternalImageServiceBaseUrl              = source.MaybeExternalImageServiceBaseUrl
                MaybeExternalStaticResourceUrlPattern         = source.MaybeExternalStaticResourceUrlPattern
                MaybeInBundleResourceUrlHashedDirectoryPrefix = source.MaybeInBundleResourceUrlHashedDirectoryPrefix
                OtpResendTimeoutSeconds                       = theOtpResendTimeoutSeconds
                MaybeSessionIdCookieDomain                    = theMaybeSessionIdCookieDomain
            }
        }


// NOTE this is done mainly to allow access to the config from within
// components. Services should take config as a constructor parameter.
// Don't use for other things without thinking twice.
module Config =
    let mutable private maybeConfig: Option<Config> = None

    let initialize (config: Config) : unit =
        maybeConfig <- Some config

    let current () : Config =
        match maybeConfig with
        | Some config -> config
        | None        -> failwith "Config has not been initialized, make sure you have a Config.initialize call in Bootstrap.fs"

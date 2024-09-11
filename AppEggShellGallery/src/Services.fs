module AppEggShellGallery.AppServices

open LibClient.EventBus
open LibClient.Services.HttpService.HttpService

// NOTE the purpose of this setup is to simulate dependency injection
// while keeping circular dependency under control. In practice I've never used
// dependency injection for facilitating automated testing in five years
// of having this system in place, so maybe it's not actually worth the
// trouble (i.e. services could access their dependencies through global
// singleton instances instead of through the constructor; circular
// dependencies would be kept at bay by F#'s requirements anyway)

let mutable private maybeConfig: Option<AppEggShellGallery.Config> = None

let initialize (config: AppEggShellGallery.Config) : unit =
    maybeConfig <- Some config

    let eventBus = EventBus()
    let httpService =
        let staticResourceUrlTransformSettings = StaticResourceUrlTransformSettings.Pattern (config.MaybeInBundleStaticResourceUrlPattern, config.MaybeExternalStaticResourceUrlPattern)
        HttpService (eventBus, staticResourceUrlTransformSettings, (fun url -> url.StartsWith(config.BackendUrl)), config.MaybeInBundleResourceUrlHashedDirectoryPrefix)

    LibClient.ServiceInstances.provideInstances {
        EventBus         = eventBus
        Date             = LibClient.Services.DateService.DateService()
        Http             = httpService
        ThothEncodedHttp = LibClient.Services.HttpService.ThothEncodedHttpService.ThothEncodedHttpService httpService
        PageTitle        = LibClient.Services.PageTitleService.PageTitleService("EggShell Gallery")
        Image            = LibClient.Services.ImageService.ImageService.WithoutOptimizations httpService
    }

let private lazyServices = lazy (
    match maybeConfig with
    | None -> failwith "AppEggShellGallery.AppServices.initialize was never called"
    | Some _config ->
        let localStorageService      = LibClient.Services.LocalStorageService.LocalStorageService("AppEggShellGallery")
        let componentSettingsService = LibClient.Services.ComponentSettingsService.ComponentSettingsService(localStorageService)

        {|
            Http              = LibClient.ServiceInstances.services().Http
            ComponentSettings = componentSettingsService
        |}
)

let services () = lazyServices.Force()

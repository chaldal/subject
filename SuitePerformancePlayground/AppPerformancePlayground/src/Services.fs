module AppPerformancePlayground.AppServices

open AppPerformancePlayground.Services.Session.SessionService
open LibClient.EventBus
open LibClient.Services.HttpService.HttpService
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibUiSubject.Services.RealTimeService
open LibUiSubject.Services.SubjectService
open System

// NOTE the purpose of this setup is to simulate dependency injection
// while keeping circular dependency under control. In practice I've never used
// dependency injection for facilitating automated testing in five years
// of having this system in place, so maybe it's not actually worth the
// trouble (i.e. services could access their dependencies through global
// singleton instances instead of through the constructor; circular
// dependencies would be kept at bay by F#'s requirements anyway)

let mutable private maybeConfig: Option<AppPerformancePlayground.Config> = None

let initialize (config: AppPerformancePlayground.Config) : unit =
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
        PageTitle        = LibClient.Services.PageTitleService.PageTitleService("Sample App")
        Image            = LibClient.Services.ImageService.ImageService.WithoutOptimizations httpService
    }

type SubjectServiceInstances = {
    // one entry per subject, e.g.
    // Shipment:       SubjectService<Shipment,       Shipment,       ShipmentId,       ShipmentIndex,       ShipmentNumericIndex,       ShipmentStringIndex,       ShipmentSearchIndex,       ShipmentConstructor,       ShipmentAction,       ShipmentLifeEvent,       ShipmentOpError>

    Session:        SessionService
}


let private lazyServices = lazy (
    match maybeConfig with
    | None -> failwith "AppPerformancePlayground.AppServices.initialize was never called"
    | Some config ->
        let realTimeService         = RealTimeService (LibClient.ServiceInstances.services().EventBus, config.BackendUrl)
        let thothEncodedHttpService = LibClient.ServiceInstances.services().ThothEncodedHttp
        let localStorageService     = LibClient.Services.LocalStorageService.LocalStorageService "AppPerformancePlayground"

        let subjectServices = {
            // one entry per subject, e.g.
            // Shipment       = SubjectService.Create shipmentLifeCycleDef            { Subject = TimeSpan.FromMinutes 1.; Query = TimeSpan.FromMinutes 1. } realTimeService thothEncodedHttpService (LibClient.ServiceInstances.services().EventBus) config.BackendUrl

            Session = SessionService (config.BackendUrl, config.MaybeSessionIdCookieDomain, realTimeService, thothEncodedHttpService, localStorageService, LibClient.ServiceInstances.services().EventBus)
        }

        {| subjectServices with
            Http                    = LibClient.ServiceInstances.services().Http
            ThothEncodedHttpService = thothEncodedHttpService
            LocalStorage            = localStorageService
        |}
)

let services () = lazyServices.Force()
module LibClient.ServiceInstances

type ServiceInstances = {
    EventBus:         LibClient.EventBus.EventBus
    Date:             Services.DateService.DateService
    Http:             Services.HttpService.HttpService.HttpService
    ThothEncodedHttp: Services.HttpService.ThothEncodedHttpService.ThothEncodedHttpService
    PageTitle:        Services.PageTitleService.PageTitleService
    Image:            Services.ImageService.ImageService
}

let mutable private maybeInstances: Option<ServiceInstances> = None

let provideInstances (instances: ServiceInstances) : unit =
    maybeInstances <- Some instances

let services() : ServiceInstances =
    match maybeInstances with
    | None           -> raise (System.Exception "LibClient ServiceInstances were not provided (you're probably missing the LibClient.ServiceInstances.provideInstances call in your app's Services.fs file)")
    | Some instances -> instances

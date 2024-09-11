module AppPerformancePlayground.Navigation

open LibClient
open LibClient.Services.ImageService
open LibRouter.RoutesSpec
open LibRouter.Components.With.Navigation
open LibUiSubjectAdmin

type ResultlessDialog =
| DoSomething of int
with interface NavigationResultlessDialog

type ResultfulDialog =
| Sentinel
with interface NavigationResultfulDialog

type Route =
| Landing
| Bananas
| Mangoes
with interface NavigationRoute

type PreviousNavigationFrame = LibRouter.RoutesSpec.PreviousNavigationFrame<Route, ResultlessDialog>

let navigationState = LibRouter.RoutesSpec.NavigationState<Route, ResultlessDialog, ResultfulDialog>()

let private lazyRoutesSpec: Lazy<LibRouter.RoutesSpec.Conversions<Route, ResultlessDialog>> = lazy (
    let specs: List<LibRouter.RoutesSpec.Spec<Route>> =
        [
            ("/",
                (fun _ -> Landing),
                (function (Landing) -> Some [] | _ -> None))
            ("/Bananas",
                (fun _ -> Bananas),
                (function (Bananas) -> Some [] | _ -> None))
            ("/Mangoes",
                (fun _ -> Mangoes),
                (function (Mangoes) -> Some [] | _ -> None))
        ]
    LibRouter.RoutesSpec.makeConversions (Config.current().AppUrlBase) specs navigationState
)

let routesSpec() = lazyRoutesSpec.Force()

type Navigation(queue) =
    inherit LibRouter.Components.With.Navigation.Navigation<Route, ResultlessDialog, ResultfulDialog>(queue)

    // if you group routes, you might want to create additional this.Go overrides, e.g.
    // member this.Go (pickerRoute: PickerRoute) : ReactEvent.Action -> unit =
    //     this.Go (Picker pickerRoute)

    // member this.GoInSameTab (adminRoute: AdminRoute) : unit =
    //     this.GoInSameTab (Admin adminRoute)

// type LibClient.Input.ButtonHighLevelStateFactory with
//     Same here, additional MakeGo overrides for various grouped routes
//     static member MakeGo (pickerRoute: PickerRoute, nav: Navigation) : ButtonHighLevelState =
//         ButtonHighLevelStateFactory.MakeGo (Picker pickerRoute, nav)


let navigationQueue: LibClient.EventBus.Queue<NavigationAction<Route, ResultlessDialog, ResultfulDialog>> = LibClient.EventBus.Queue "navigation"
let nav = Navigation navigationQueue

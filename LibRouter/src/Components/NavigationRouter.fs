[<AutoOpen>]
module LibRouter.Components.NavigationRouter

open System
open Fable.React
open LibClient
open LibClient.EventBus
open LibRouter.Components
open LibRouter.Components.With.Navigation

type private Helpers =
    [<Component>]
    static member Internals (
        spec:            LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>,
        navigationState: LibRouter.RoutesSpec.NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>,
        queue:           EventBus.Queue<NavigationAction<'Route, 'ResultlessDialog, 'ResultfulDialog>>,
        child:           ReactElement,
        ?onNavigation:   LibRouter.RoutesSpec.Location->bool
    ) : ReactElement =
        let location = LibRouter.Components.Router.useLocation ()
        let navigate = LibRouter.Components.Router.useNavigate ()

        let maybeUnsubscribeState = Hooks.useState<Option<unit -> unit>> None

        Hooks.useEffectDisposable (
            (fun () ->
                maybeUnsubscribeState.current |> Option.sideEffect (fun unsubscribe -> unsubscribe())

                let navImplementation = NavigationImplementation (spec, navigationState, navigate, location, ?onNavigation = onNavigation)
                let result = LibClient.ServiceInstances.services().EventBus.On queue navImplementation.ProcessAction

                maybeUnsubscribeState.update (fun _ -> Some result.Off)

                { new IDisposable with
                    member _.Dispose () : unit =
                        maybeUnsubscribeState.current |> Option.sideEffect (fun unsubscribe -> unsubscribe())
                }
            ),
            [|location; spec|]
        )
        child

and LR with
    [<Component>]
    static member NavigationRouter (
        spec:            LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>,
        navigationState: LibRouter.RoutesSpec.NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>,
        queue:           EventBus.Queue<NavigationAction<'Route, 'ResultlessDialog, 'ResultfulDialog>>,
        child:           ReactElement,
        ?initialEntries: string array,
        ?onNavigation:   LibRouter.RoutesSpec.Location->bool
    ) : ReactElement = element {
        LR.Router(
            ?initialEntries = initialEntries,
            children = [|
                Helpers.Internals (spec, navigationState, queue, child, ?onNavigation = onNavigation)
            |]
        )
    }

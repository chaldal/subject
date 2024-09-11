[<AutoOpen>]
module LibRouter.Components.With_CurrentRoute

open Fable.React
open LibClient
open LibRouter.Components
open LibRouter.RoutesSpec

type LR.With with
    static member CurrentRoute (spec: Conversions<'Route, 'ResultlessDialog>, fn: Option<'Route> -> ReactElement) : ReactElement =
        LR.With.Route (
            spec = spec,
            ``with`` = (Option.map NavigationFrame.route >> fn)
        )

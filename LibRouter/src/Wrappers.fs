[<AutoOpen>]
module rec LibRouter_Wrappers

open Fable.React
open LibClient
open LibRouter.Components

type LR =
    static member UseRoute (spec: LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>) =
        let location = LibRouter.Components.Router.useLocation()
        spec.FromLocation { Path = location.pathname; Query = location.search }

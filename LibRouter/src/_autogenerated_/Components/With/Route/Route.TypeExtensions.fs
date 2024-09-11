namespace LibRouter.Components

open LibClient
open LibRouter.RoutesSpec
open LibRouter.Components.With.Route
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_RouteTypeExtensions =
    type LibRouter.Components.Constructors.LR.With with
        static member Route(spec: LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>, ``with``: Option<NavigationFrame<'Route, 'ResultlessDialog>> -> ReactElement, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Spec = spec
                    With = ``with``
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.With.Route.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
namespace LibRouter.Components

open LibClient
open LibClient.Dialogs
open LibRouter.RoutesSpec
open LibClient.ServiceInstances
open LibRouter.Components
open Fable.Core.JsInterop
open LibRouter.Components.With.Navigation
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_NavigationTypeExtensions =
    type LibRouter.Components.Constructors.LR.With with
        static member Navigation(spec: LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>, navigationState: LibRouter.RoutesSpec.NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>, ``with``: Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog> -> ReactElement, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Spec = spec
                    NavigationState = navigationState
                    With = ``with``
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.With.Navigation.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
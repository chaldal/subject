namespace LibRouter.Components

open LibClient
open LibRouter.Components.With.Location
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_LocationTypeExtensions =
    type LibRouter.Components.Constructors.LR.With with
        static member Location(``with``: LibRouter.Components.Router.Location -> ReactElement, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    With = ``with``
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.With.Location.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
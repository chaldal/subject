namespace LibClient.Components

open LibClient
open LibClient.Responsive
open ReactXP.Styles
open LibClient.Components.Nav.Top.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Nav_Top_BaseTypeExtensions =
    type LibClient.Components.Constructors.LC.Nav.Top with
        static member Base(handheld: unit -> ReactElement, desktop: unit -> ReactElement, ?children: ReactChildrenProp, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Handheld = handheld
                    Desktop = desktop
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Nav.Top.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
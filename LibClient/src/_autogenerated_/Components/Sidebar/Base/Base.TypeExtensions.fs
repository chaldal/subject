namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Sidebar.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Sidebar_BaseTypeExtensions =
    type LibClient.Components.Constructors.LC.Sidebar with
        static member Base(?children: ReactChildrenProp, ?fixedTop: ReactElement, ?fixedBottom: ReactElement, ?scrollableMiddle: ReactElement, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    FixedTop = defaultArg fixedTop (noElement)
                    FixedBottom = defaultArg fixedBottom (noElement)
                    ScrollableMiddle = defaultArg scrollableMiddle (noElement)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Sidebar.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
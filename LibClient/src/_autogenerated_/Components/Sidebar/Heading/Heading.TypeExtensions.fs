namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Sidebar.Heading
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Sidebar_HeadingTypeExtensions =
    type LibClient.Components.Constructors.LC.Sidebar with
        static member Heading(text: string, ?children: ReactChildrenProp, ?level: Level, ?styles: array<TextStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Text = text
                    Level = defaultArg level (Level.Primary)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Sidebar.Heading.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
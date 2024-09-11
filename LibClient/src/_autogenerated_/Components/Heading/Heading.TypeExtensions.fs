namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.Heading
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module HeadingTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member Heading(?children: ReactChildrenProp, ?level: Level, ?styles: array<TextStyles>, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Level = defaultArg level (Primary)
                    styles = styles |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Heading.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
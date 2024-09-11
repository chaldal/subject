namespace ThirdParty.SyntaxHighlighter.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open ThirdParty.SyntaxHighlighter.Components.SyntaxHighlighter
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module SyntaxHighlighterTypeExtensions =
    type ThirdParty.SyntaxHighlighter.Components.Constructors.SyntaxHighlighter with
        static member SyntaxHighlighter(language: Language, source: string, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Language = language
                    Source = source
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.SyntaxHighlighter.Components.SyntaxHighlighter.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
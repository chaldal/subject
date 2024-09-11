namespace AppEggShellGallery.Components

open LibClient
open Fable.Core.JsInterop
open Fable.Core
open ThirdParty.SyntaxHighlighter.Components
open AppEggShellGallery.Components.Code
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module CodeTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui with
        static member Code(language: SyntaxHighlighter.Language, ?children: ReactChildrenProp, ?stripLeadingWhitespace: bool, ?heading: string, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Language = language
                    StripLeadingWhitespace = defaultArg stripLeadingWhitespace (true)
                    Heading = heading |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Code.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
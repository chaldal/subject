namespace AppEggShellGallery.Components

open LibClient
open ReactXP.Styles.RulesBasic
open ThirdParty.SyntaxHighlighter.Components
open AppEggShellGallery.Components.ComponentSample
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ComponentSampleTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui with
        static member ComponentSample(visuals: ReactElement, code: Code, ?children: ReactChildrenProp, ?notes: ReactElement, ?verticalAlignment: VerticalAlignment, ?layout: Layout, ?heading: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Visuals = visuals
                    Code = code
                    Notes = defaultArg notes (noElement)
                    VerticalAlignment = defaultArg verticalAlignment (VerticalAlignment.Middle)
                    Layout = defaultArg layout (Layout.SideBySide)
                    Heading = heading |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.ComponentSample.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
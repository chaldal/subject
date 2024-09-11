namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.ComponentSampleGroup
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ComponentSampleGroupTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui with
        static member ComponentSampleGroup(samples: ReactElement, ?children: ReactChildrenProp, ?notes: ReactElement, ?heading: string, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Samples = samples
                    Notes = defaultArg notes (noElement)
                    Heading = heading |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.ComponentSampleGroup.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
namespace AppEggShellGallery.Components

open LibClient
open AppEggShellGallery.Components.ComponentContent
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ComponentContentTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui with
        static member ComponentContent(displayName: string, samples: ReactElement, ?children: ReactChildrenProp, ?isResponsive: bool, ?notes: ReactElement, ?themeSamples: ReactElement, ?props: PropsConfig, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    DisplayName = displayName
                    Samples = samples
                    IsResponsive = defaultArg isResponsive (false)
                    Notes = defaultArg notes (noElement)
                    ThemeSamples = defaultArg themeSamples (noElement)
                    Props = props |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.ComponentContent.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
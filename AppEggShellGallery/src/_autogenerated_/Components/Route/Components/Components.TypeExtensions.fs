namespace AppEggShellGallery.Components

open LibClient
open LibLang
open LibClient.Responsive
open AppEggShellGallery.Navigation
open AppEggShellGallery.Components.Route.Components
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Route_ComponentsTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Route with
        static member Components(pstoreKey: string, sampleVisualsScreenSize: ScreenSize, content: ComponentItem, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    PstoreKey = pstoreKey
                    SampleVisualsScreenSize = sampleVisualsScreenSize
                    Content = content
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Route.Components.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
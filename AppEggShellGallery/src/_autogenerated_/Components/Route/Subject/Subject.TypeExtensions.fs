namespace AppEggShellGallery.Components

open LibClient
open LibLang
open AppEggShellGallery.Components.Route.Subject
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Route_SubjectTypeExtensions =
    type AppEggShellGallery.Components.Constructors.Ui.Route with
        static member Subject(pstoreKey: string, markdownUrl: string, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    PstoreKey = pstoreKey
                    MarkdownUrl = markdownUrl
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            AppEggShellGallery.Components.Route.Subject.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
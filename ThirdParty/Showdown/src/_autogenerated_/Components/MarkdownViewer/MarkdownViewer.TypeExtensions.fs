namespace ThirdParty.Showdown.Components

open LibClient
open LibClient.ServiceInstances
open Fable.Core.JsInterop
open LibClient.Services.HttpService.Types
open ThirdParty.Showdown.Components.MarkdownViewer
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module MarkdownViewerTypeExtensions =
    type ThirdParty.Showdown.Components.Constructors.Showdown with
        static member MarkdownViewer(source: Source, ?children: ReactChildrenProp, ?showdownConverter: obj, ?globalLinkHandler: string, ?imageUrlTransformer: (Source -> string -> string), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Source = source
                    ShowdownConverter = defaultArg showdownConverter (ThirdParty.Showdown.Components.MarkdownViewer.defaultShowdownConverter)
                    GlobalLinkHandler = globalLinkHandler |> Option.orElse (None)
                    ImageUrlTransformer = imageUrlTransformer |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.Showdown.Components.MarkdownViewer.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
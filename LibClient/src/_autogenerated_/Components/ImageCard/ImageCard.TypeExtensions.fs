namespace LibClient.Components

open LibClient
open ReactXP.Styles
open LibClient.Components.ImageCard
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module ImageCardTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member ImageCard(source: LibClient.Services.ImageService.ImageSource, ?children: ReactChildrenProp, ?style: Style, ?corners: Corners, ?label: Label, ?onPress: (ReactEvent.Action -> unit), ?styles: array<ViewStyles>, ?labelStyles: array<TextStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Source = source
                    Style = defaultArg style (Style.Flat)
                    Corners = defaultArg corners (Corners.Sharp)
                    Label = label |> Option.orElse (None)
                    OnPress = onPress |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    labelStyles = labelStyles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.ImageCard.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
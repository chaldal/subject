namespace ReactXP.Components

open LibClient
open LibClient.JsInterop
open ReactXP.Helpers
open Fable.Core.JsInterop
open Fable.React
open Fable.Core
open Browser.Types
open ReactXP.Components.Text
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module TextTypeExtensions =
    type ReactXP.Components.Constructors.RX with
        static member Text(?children: ReactChildrenProp, ?selectable: bool, ?numberOfLines: int, ?allowFontScaling: bool, ?maxContentSizeMultiplier: float, ?ellipsizeMode: EllipsizeMode, ?textBreakStrategy: TextBreakStrategy, ?importantForAccessibility: ImportantForAccessibility, ?accessibilityId: string, ?autoFocus: bool, ?onPress: (PointerEvent -> unit), ?id: string, ?onContextMenu: (MouseEvent -> unit), ?style: obj, ?key: string, ?xLegacyStyles: List<ReactXP.Styles.RuntimeStyles>, ?styles: List<ReactXP.Styles.FSharpDialect.NewStyles>) =
            let __props =
                {
                    selectable = selectable |> Option.orElse ((Some true))
                    numberOfLines = numberOfLines |> Option.orElse (Undefined)
                    allowFontScaling = allowFontScaling |> Option.orElse (Undefined)
                    maxContentSizeMultiplier = maxContentSizeMultiplier |> Option.orElse (Undefined)
                    ellipsizeMode = ellipsizeMode |> Option.orElse (Undefined)
                    textBreakStrategy = textBreakStrategy |> Option.orElse (Undefined)
                    importantForAccessibility = importantForAccessibility |> Option.orElse (Undefined)
                    accessibilityId = accessibilityId |> Option.orElse (Undefined)
                    autoFocus = autoFocus |> Option.orElse (Undefined)
                    onPress = onPress |> Option.orElse (Undefined)
                    id = id |> Option.orElse (Undefined)
                    onContextMenu = onContextMenu |> Option.orElse (Undefined)
                    style = style |> Option.orElse (Undefined)
                    key = key |> Option.orElse (Undefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            match styles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?style <- styles |> ReactXP.Styles.FSharpDialect.Styles.newStylesToRuntimeStyles |> ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Text"
            ReactXP.Components.Text.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
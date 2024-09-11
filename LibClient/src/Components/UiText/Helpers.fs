[<AutoOpen>]
module LibClient.Components.UiTextHelpers

open LibClient
open LibClient.JsInterop
open Browser.Types
open LibClient.Components.UiText
open ReactXP.Styles
open LC.UiText

type LibClient.Components.Constructors.LC with
    static member UiText(value: string, ?selectable: bool, ?numberOfLines: int, ?allowFontScaling: bool, ?maxContentSizeMultiplier: float, ?ellipsizeMode: EllipsizeMode, ?textBreakStrategy: TextBreakStrategy, ?importantForAccessibility: ImportantForAccessibility, ?accessibilityId: string, ?autoFocus: bool, ?onPress: (PointerEvent -> unit), ?id: string, ?onContextMenu: (MouseEvent -> unit), ?key: string, ?styles: array<TextStyles>, ?theme: Theme -> Theme) =
        LC.UiText(
            children = [|Fable.React.Helpers.str value|],
            ?selectable = selectable,
            ?numberOfLines = numberOfLines,
            ?allowFontScaling = allowFontScaling,
            ?maxContentSizeMultiplier = maxContentSizeMultiplier,
            ?ellipsizeMode = ellipsizeMode,
            ?textBreakStrategy = textBreakStrategy,
            ?importantForAccessibility = importantForAccessibility,
            ?accessibilityId = accessibilityId,
            ?autoFocus = autoFocus,
            ?onPress = onPress,
            ?id = id,
            ?onContextMenu = onContextMenu,
            ?key = key,
            ?styles = styles,
            ?theme = theme
        )

    static member UiText(value: NonemptyString, ?selectable: bool, ?numberOfLines: int, ?allowFontScaling: bool, ?maxContentSizeMultiplier: float, ?ellipsizeMode: EllipsizeMode, ?textBreakStrategy: TextBreakStrategy, ?importantForAccessibility: ImportantForAccessibility, ?accessibilityId: string, ?autoFocus: bool, ?onPress: (PointerEvent -> unit), ?id: string, ?onContextMenu: (MouseEvent -> unit), ?key: string, ?styles: array<TextStyles>, ?theme: Theme -> Theme) =
        LC.UiText(
            children = [|Fable.React.Helpers.str value.Value|],
            ?selectable = selectable,
            ?numberOfLines = numberOfLines,
            ?allowFontScaling = allowFontScaling,
            ?maxContentSizeMultiplier = maxContentSizeMultiplier,
            ?ellipsizeMode = ellipsizeMode,
            ?textBreakStrategy = textBreakStrategy,
            ?importantForAccessibility = importantForAccessibility,
            ?accessibilityId = accessibilityId,
            ?autoFocus = autoFocus,
            ?onPress = onPress,
            ?id = id,
            ?onContextMenu = onContextMenu,
            ?key = key,
            ?styles = styles,
            ?theme = theme
        )

    static member UiText(value: Option<NonemptyString>, ?selectable: bool, ?numberOfLines: int, ?allowFontScaling: bool, ?maxContentSizeMultiplier: float, ?ellipsizeMode: EllipsizeMode, ?textBreakStrategy: TextBreakStrategy, ?importantForAccessibility: ImportantForAccessibility, ?accessibilityId: string, ?autoFocus: bool, ?onPress: (PointerEvent -> unit), ?id: string, ?onContextMenu: (MouseEvent -> unit), ?key: string, ?styles: array<TextStyles>, ?theme: Theme -> Theme) =
        LC.UiText(
            children = [|Fable.React.Helpers.str (value |> Option.map NonemptyString.value |> Option.getOrElse "")|],
            ?selectable = selectable,
            ?numberOfLines = numberOfLines,
            ?allowFontScaling = allowFontScaling,
            ?maxContentSizeMultiplier = maxContentSizeMultiplier,
            ?ellipsizeMode = ellipsizeMode,
            ?textBreakStrategy = textBreakStrategy,
            ?importantForAccessibility = importantForAccessibility,
            ?accessibilityId = accessibilityId,
            ?autoFocus = autoFocus,
            ?onPress = onPress,
            ?id = id,
            ?onContextMenu = onContextMenu,
            ?key = key,
            ?styles = styles,
            ?theme = theme
        )

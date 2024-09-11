[<AutoOpen>]
module ReactXP.Components.UiText

open LibClient

open ReactXP.Helpers

open Fable.Core.JsInterop
open Fable.Core
open Browser.Types

[<StringEnum>]
type EllipsizeMode =
| Head
| Middle
| Tail

[<StringEnum>]
type TextBreakStrategy =
| HighQuality
| Simple
| Balanced

type ImportantForAccessibility =
| Auto              = 1
| Yes               = 2
| No                = 3
| NoHideDescendants = 4


type ReactXP.Components.Constructors.RX with
    static member UiText(
        value:                      string,
        ?selectable:                bool,
        ?numberOfLines:             int,
        ?allowFontScaling:          bool,
        ?maxContentSizeMultiplier:  float,
        ?ellipsizeMode:             EllipsizeMode,
        ?textBreakStrategy:         TextBreakStrategy,
        ?importantForAccessibility: ImportantForAccessibility,
        ?accessibilityId:           string,
        ?autoFocus:                 bool,
        ?onPress:                   PointerEvent -> unit,
        ?id:                        string,
        ?onContextMenu:             MouseEvent -> unit,
        ?key:                       string,
        ?xLegacyStyles:             List<ReactXP.LegacyStyles.RuntimeStyles>,
        ?styles:                    array<ReactXP.Styles.FSharpDialect.TextStyles>
    ) =
        ReactXP.Components.Constructors.RX.UiText(
            children                   = [|Fable.React.Helpers.str value|],
            ?selectable                = selectable,
            ?numberOfLines             = numberOfLines,
            ?allowFontScaling          = allowFontScaling,
            ?maxContentSizeMultiplier  = maxContentSizeMultiplier,
            ?ellipsizeMode             = ellipsizeMode,
            ?textBreakStrategy         = textBreakStrategy,
            ?importantForAccessibility = importantForAccessibility,
            ?accessibilityId           = accessibilityId,
            ?autoFocus                 = autoFocus,
            ?onPress                   = onPress,
            ?id                        = id,
            ?onContextMenu             = onContextMenu,
            ?key                       = key,
            ?xLegacyStyles             = xLegacyStyles,
            ?styles                    = styles
        )

    static member UiText(
        ?children:                  ReactChildrenProp,
        ?selectable:                bool,
        ?numberOfLines:             int,
        ?allowFontScaling:          bool,
        ?maxContentSizeMultiplier:  float,
        ?ellipsizeMode:             EllipsizeMode,
        ?textBreakStrategy:         TextBreakStrategy,
        ?importantForAccessibility: ImportantForAccessibility,
        ?accessibilityId:           string,
        ?autoFocus:                 bool,
        ?onPress:                   PointerEvent -> unit,
        ?id:                        string,
        ?onContextMenu:             MouseEvent -> unit,
        ?key:                       string,
        ?xLegacyStyles:             List<ReactXP.LegacyStyles.RuntimeStyles>,
        ?styles:                    array<ReactXP.Styles.FSharpDialect.TextStyles>
    ) =
        let __props = createEmpty
        __props?selectable                <- selectable |> Option.orElse (Some false)
        __props?numberOfLines             <- numberOfLines
        __props?allowFontScaling          <- allowFontScaling
        __props?maxContentSizeMultiplier  <- maxContentSizeMultiplier
        __props?ellipsizeMode             <- ellipsizeMode
        __props?textBreakStrategy         <- textBreakStrategy
        __props?importantForAccessibility <- importantForAccessibility
        __props?accessibilityId           <- accessibilityId
        __props?autoFocus                 <- autoFocus
        __props?onPress                   <- onPress
        __props?id                        <- id
        __props?onContextMenu             <- onContextMenu
        __props?style                     <- styles
        __props?key                       <- key

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?Text,
            __props,
            ThirdParty.fixPotentiallySingleChild (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
        )

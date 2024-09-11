[<AutoOpen>]
module ReactXP.Components.AnimatableText

open LibClient

open ReactXP.Helpers
open ReactXP.Styles

open Fable.Core
open Fable.Core.JsInterop
open Browser.Types

module RX =
    module AnimatableText =
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

open RX.AnimatableText

type ReactXP.Components.Constructors.RX with
    static member AnimatableText(
        ?children:                  ReactElements,
        ?selectable:                bool,
        ?numberOfLines:             int,
        ?allowFontScaling:          bool,
        ?maxContentSizeMultiplier:  float,
        ?ellipsizeMode:             EllipsizeMode,
        ?textBreakStrategy:         TextBreakStrategy,
        ?importantForAccessibility: ImportantForAccessibility,
        ?accessibilityId:           string,
        ?autoFocus:                 bool,
        ?onPress:                   Event -> unit,
        ?id:                        string,
        ?onContextMenu:             MouseEvent -> unit,
        ?styles:                    array<AnimatableTextStyles>
    ) =
        let __props = createEmpty

        __props?selectable                <- selectable |> Option.orElse (Some true)
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

        let children =
            children
            |> Option.map tellReactArrayKeysAreOkay
            |> Option.getOrElse [||]
            |> ThirdParty.fixPotentiallySingleChild

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?Animated?Text,
            __props,
            children
        )

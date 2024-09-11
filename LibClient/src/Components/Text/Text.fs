[<AutoOpen>]
module LibClient.Components.Text

open LibClient
open LibClient.JsInterop
open Fable.Core.JsInterop
open Browser.Types
open ReactXP.Styles

type EllipsizeMode = ReactXP.Components.Text.EllipsizeMode
type TextBreakStrategy = ReactXP.Components.Text.TextBreakStrategy
type ImportantForAccessibility = ReactXP.Components.Text.ImportantForAccessibility

let Head   = EllipsizeMode.Head
let Middle = EllipsizeMode.Middle
let Tail   = EllipsizeMode.Tail

let HighQuality = TextBreakStrategy.HighQuality
let Simple      = TextBreakStrategy.Simple
let Balanced    = TextBreakStrategy.Balanced

let Auto              = ImportantForAccessibility.Auto
let Yes               = ImportantForAccessibility.Yes
let No                = ImportantForAccessibility.No
let NoHideDescendants = ImportantForAccessibility.NoHideDescendants

module LC =
    module Text =
        type Theme = {
            FontFamily: string
        }

open LC.Text

type LC.Text.Theme with
    member this.Styles = makeTextStyles {
        fontFamily this.FontFamily
    }

let private baseThemeStyle = lazy (
    (Themes.GetMaybeUpdatedWith Option<Theme -> Theme>.None).Styles
)

type LibClient.Components.Constructors.LC with
    static member Text(
        children:                   ReactChildrenProp,
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
        ?theme:                     Theme -> Theme,
        ?styles:                    array<TextStyles>
    ) =
        let themeStyles =
            match theme with
            | None       -> baseThemeStyle.Value
            | Some theme -> (Themes.GetMaybeUpdatedWith (Some theme)).Styles

        let styleValue =
            match styles with
            | Option.Some style -> [|themeStyles; !!style|]
            | _ -> !!themeStyles

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
        __props?key                       <- key
        __props?style                     <- styleValue

        Fable.React.ReactBindings.React.createElement(
            ReactXP.Helpers.ReactXPRaw?Text,
            __props,
            ThirdParty.fixPotentiallySingleChild (tellReactArrayKeysAreOkay children)
        )

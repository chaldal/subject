[<AutoOpen>]
module LibClient.Components.LegacyText

open LibClient
open LibClient.JsInterop
open Fable.Core.JsInterop
open Browser.Types
open ReactXP.LegacyStyles

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

let private baseThemeStyle = lazy (
    (Themes.GetMaybeUpdatedWith Option<LibClient.Components.Text.LC.Text.Theme -> LibClient.Components.Text.LC.Text.Theme>.None).Styles
)

type LibClient.Components.Constructors.LC with
    static member LegacyText(children: ReactChildrenProp, ?selectable: bool, ?numberOfLines: int, ?allowFontScaling: bool, ?maxContentSizeMultiplier: float, ?ellipsizeMode: EllipsizeMode, ?textBreakStrategy: TextBreakStrategy, ?importantForAccessibility: ImportantForAccessibility, ?accessibilityId: string, ?autoFocus: bool, ?onPress: (PointerEvent -> unit), ?id: string, ?onContextMenu: (MouseEvent -> unit), ?key: string, ?xLegacyStyles: List<RuntimeStyles>, ?xLegacyClassName: string, ?theme: LibClient.Components.Text.LC.Text.Theme -> LibClient.Components.Text.LC.Text.Theme, ?styles: array<ReactXP.Styles.FSharpDialect.TextStyles>) =
        ignore xLegacyClassName
        let themeStyles =
            match theme with
            | None       -> baseThemeStyle.Value
            | Some theme -> (Themes.GetMaybeUpdatedWith (Some theme)).Styles

        let maybeStyleValueFirstPass =
            match xLegacyStyles with
            | Option.Some legacyStyles ->
                match Runtime.findTopLevelBlockStyles legacyStyles with
                | [] -> None
                | styles ->
                    [|
                        themeStyles
                        Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Text" styles
                    |]
                    |> Some
            | _ -> Some !!themeStyles

        let maybeStyleValueSecondPass =
            match (maybeStyleValueFirstPass, styles) with
            | (None, None) -> None
            | (None, Some value) -> Some value
            | (Some value, None) -> Some value
            | (Some a, Some b)   -> Array.append a b |> Some

        let __props = createEmpty
        __props?selectable <- selectable |> Option.orElse ((Some true))
        __props?numberOfLines <- numberOfLines |> Option.orElse (Undefined)
        __props?allowFontScaling <- allowFontScaling |> Option.orElse (Undefined)
        __props?maxContentSizeMultiplier <- maxContentSizeMultiplier |> Option.orElse (Undefined)
        __props?ellipsizeMode <- ellipsizeMode |> Option.orElse (Undefined)
        __props?textBreakStrategy <- textBreakStrategy |> Option.orElse (Undefined)
        __props?importantForAccessibility <- importantForAccessibility |> Option.orElse (Undefined)
        __props?accessibilityId <- accessibilityId |> Option.orElse (Undefined)
        __props?autoFocus <- autoFocus |> Option.orElse (Undefined)
        __props?onPress <- onPress |> Option.orElse (Undefined)
        __props?id <- id |> Option.orElse (Undefined)
        __props?onContextMenu <- onContextMenu |> Option.orElse (Undefined)
        __props?key <- key |> Option.orElse (JsUndefined)
        __props?style <- maybeStyleValueSecondPass

        Fable.React.ReactBindings.React.createElement(
            ReactXP.Helpers.ReactXPRaw?Text,
            __props,
            ThirdParty.fixPotentiallySingleChild (tellReactArrayKeysAreOkay children)
        )

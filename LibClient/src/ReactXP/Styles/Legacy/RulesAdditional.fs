namespace ReactXP.LegacyStyles

open ReactXP.LegacyStyles
open LibClient.ColorModule

[<AutoOpen>]
module RulesAdditional =
    let trbl (t: int) (r: int) (b: int) (l: int) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            top    t
            right  r
            bottom b
            left   l
        |]

    let size (w: int) (h: int) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            width  w
            height h
        |]

    let border (width: int) (color: Color) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            borderWidth width
            borderColor color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderTop (width: int) (color: Color) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            borderTopWidth width
            borderColor color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderBottom (width: int) (color: Color) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            borderBottomWidth width
            borderColor       color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderLeft (width: int) (color: Color) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            borderLeftWidth width
            borderColor color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderRight (width: int) (color: Color) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            borderRightWidth width
            borderColor color
        |]

    let paddingHV (h: int) (v: int) : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            paddingHorizontal h
            paddingVertical v
        |]

    let shadow (color: Color) (blur: int) (offsetXY: int * int) : RuleFunctionReturnedStyleRules =
        let (offsetX, offsetY) = offsetXY

        RuleFunctionReturnedStyleRules.Many [|
            shadowColor  color
            shadowRadius blur
            shadowOffset { width = offsetX; height = offsetY }
            shadowOpacity 1
            elevation (max 3 (blur /2)) // Approximate elevation based on blur
        |]

    let centerContentVH : RuleFunctionReturnedStyleRules =
        RuleFunctionReturnedStyleRules.Many [|
            FlexDirection.Row
            AlignItems.Center
            JustifyContent.Center
        |]

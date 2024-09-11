namespace ReactXP.Styles

open ReactXP
open ReactXP.Styles
open LibClient.ColorModule

[<AutoOpen>]
module RulesAdditional =
    let trbl (t: int) (r: int) (b: int) (l: int) : array<RawReactXPFlexStyleRule> =
        [|
            top    t
            right  r
            bottom b
            left   l
        |]

    let size (w: int) (h: int) : array<RawReactXPFlexStyleRule> =
        [|
            width  w
            height h
        |]

    let border (width: int) (color: Color) : array<RawReactXPViewStyleRule> =
        [|
            borderWidth width
            borderColor color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderTop (width: int) (color: Color) : array<RawReactXPViewStyleRule> =
        [|
            borderTopWidth width
            borderColor color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderBottom (width: int) (color: Color) : array<RawReactXPViewStyleRule> =
        [|
            borderBottomWidth width
            borderColor       color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderLeft (width: int) (color: Color) : array<RawReactXPViewStyleRule> =
        [|
            borderLeftWidth width
            borderColor color
        |]

    // NOTE crappy ReactXP doesn't support setting color for just one side, so these helper function may eventually confuse some users
    let borderRight (width: int) (color: Color) : array<RawReactXPViewStyleRule> =
        [|
            borderRightWidth width
            borderColor color
        |]

    let paddingHV (h: int) (v: int) : array<RawReactXPFlexStyleRule> =
        [|
            paddingHorizontal h
            paddingVertical v
        |]

    let shadow (color: Color) (blur: int) (offsetXY: int * int) : array<RawReactXPViewStyleRule> =
        let (offsetX, offsetY) = offsetXY

        [|
            shadowColor  color
            shadowRadius blur
            shadowOffset { width = offsetX; height = offsetY }
            shadowOpacity 1

            #if !EGGSHELL_PLATFORM_IS_WEB
            // TODO: Do this inside ReactXP
            if ReactXP.Runtime.platform = Native NativePlatform.Android then
                let colorWithoutAlpha =
                    match color with
                    | Color.Rgba (r, g, b, _) -> Color.Rgb (r, g, b)
                    | Color.WhiteAlpha _      -> Color.White
                    | Color.BlackAlpha _      -> Color.Black
                    | this -> this

                shadowColor colorWithoutAlpha
                elevation (max 3 (blur / 2)) // Approximate elevation based on blur
            #endif
        |]
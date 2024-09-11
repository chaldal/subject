module LibClient.Components.Legacy.CardStyles

open ReactXP.LegacyStyles
open LibClient.Components.Legacy.Card



let private baseStyles = lazy (asBlocks [
    "view" => [
        margin          8
        padding         8
        backgroundColor Color.White
    ] && [
        "style-" + Shadowed.ToString() => [
            borderRadius 2
            elevation    10
            shadow       (Color.BlackAlpha 0.3) 4 (0, 1)
            Overflow.Hidden
        ]

        "style-" + Flat.ToString() => [
            borderWidth  1
            borderRadius 6
        ]
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Legacy.Card", baseStyles)

    static member All (theBorderColor: Color) : unit =
        customize [
            Theme.Rules(theBorderColor)
        ]

    static member One (theBorderColor: Color) : Styles =
        Theme.Rules(theBorderColor) |> makeSheet

    static member Rules (theBorderColor: Color) : List<ISheetBuildingBlock> = [
        "view && style-" + Flat.ToString() => [
            borderColor theBorderColor
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (theBorderColor = Color.DevRed)
]))

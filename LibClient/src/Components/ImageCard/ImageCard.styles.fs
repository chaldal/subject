module LibClient.Components.ImageCardStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "image" => [
        Position.Absolute
        trbl 0 0 0 0
    ]

    "view" => [
        FlexDirection.Column
        JustifyContent.FlexEnd
    ] && [
        $"corners-{LibClient.Components.ImageCard.Corners.Rounded}" => [
            borderRadius 6
        ]

        $"corners-{LibClient.Components.ImageCard.Style.Shadowed}" => [
            shadow (Color.BlackAlpha 0.3) 4 (0, 1)
        ]
    ]

    "scrim" => [
        Position.Absolute
        trbl            0 0 0 0
        backgroundColor (Color.BlackAlpha 0.4)
    ]

    "label-text-wrapper" => [
        paddingHV 16 8
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.ImageCard", baseStyles)

    static member All (theHeight: int, textColor: Color, ?theFontSize: int) : unit =
        customize [
            Theme.Rules (theHeight, textColor, ?theFontSize = theFontSize)
        ]

    static member One (theHeight: int, textColor: Color, ?theFontSize: int) : Styles =
        Theme.Rules (theHeight, textColor, ?theFontSize = theFontSize) |> makeSheet


    static member Rules (theHeight: int, textColor: Color, ?theFontSize: int) : List<ISheetBuildingBlock> =
        let theFontSize = defaultArg theFontSize 16
        [
            "view" => [
                height theHeight
            ]

            "label-text" => [
                color    textColor
                fontSize theFontSize
            ]
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        theHeight = 200,
        textColor = Color.White
    )
]))

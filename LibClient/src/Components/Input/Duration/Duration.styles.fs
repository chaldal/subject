module LibClient.Components.Input.DurationStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "fields" => [
        FlexDirection.Row
        AlignItems.Center
        gap 8
        paddingVertical 4
    ]

    "field" => [
        width 44
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Input.Duration", baseStyles)

    static member All (labelBlurredColor: Color, labelFocusedColor: Color, labelInvalidColor: Color, invalidReasonColor: Color) : unit =
        customize [
            Theme.Rules(labelBlurredColor, labelFocusedColor, labelInvalidColor, invalidReasonColor)
        ]

    static member One (labelBlurredColor: Color, labelFocusedColor: Color, labelInvalidColor: Color, invalidReasonColor: Color) : Styles =
        Theme.Rules(labelBlurredColor, labelFocusedColor, labelInvalidColor, invalidReasonColor) |> makeSheet


    static member Rules (labelBlurredColor: Color, labelFocusedColor: Color, labelInvalidColor: Color, invalidReasonColor: Color) : List<ISheetBuildingBlock> = [
        "invalid-reason" => [
            color invalidReasonColor
        ]

        "label" => [
            color labelBlurredColor
        ] && [
            "focused" => [
                color labelFocusedColor
            ]
            "invalid" => [
                color labelInvalidColor
            ]
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        labelBlurredColor  = Color.Grey "44",
        labelFocusedColor  = Color.DevGreen,
        labelInvalidColor  = Color.DevRed,
        invalidReasonColor = Color.DevRed
    )
]))

module LibClient.Components.Legacy.Input.PickerStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        height          46
        backgroundColor Color.White
    ]

    "withLabel" => [
       paddingTop 4 // to make space for the label
    ]

    "border" => [
        AlignItems.Center
        FlexDirection.Row
        JustifyContent.SpaceBetween
        flex              1
        borderWidth       1
        borderRadius      4
        paddingHorizontal 10
    ]

    "invalid-reason" => [
        fontSize 12
    ]

    "label" => [
        Position.Absolute
        top               0
        left              10
        paddingHorizontal 3
        backgroundColor   Color.White
    ]

    "focused" => [
        opacity 0.2
    ]

    "label-text" => [
        fontSize 12
        FontWeight.W700
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Legacy.Input.Picker", baseStyles)

    static member All (borderLabelColor: Color, borderLabelInvalidColor: Color, textColor: Color, invalidReasonColor: Color, iconSize: int) : unit =
        customize [
            Theme.Rules(borderLabelColor, borderLabelInvalidColor, textColor, invalidReasonColor, iconSize)
        ]

    static member One (borderLabelColor: Color, borderLabelInvalidColor: Color, textColor: Color, invalidReasonColor: Color, iconSize: int) : Styles =
        Theme.Rules(borderLabelColor, borderLabelInvalidColor, textColor, invalidReasonColor, iconSize) |> makeSheet

    static member Rules (borderLabelColor: Color, borderLabelInvalidColor: Color, textColor: Color, invalidReasonColor: Color, iconSize: int) : List<ISheetBuildingBlock> = [
        "selected-item" => [
           color textColor
        ]

        "icon" => [
            fontSize iconSize
            color    textColor
        ]

        "invalid-reason" => [
            color invalidReasonColor
        ]

        "border" => [
            borderColor  borderLabelColor
        ] && [
            "is-invalid" => [
                borderColor borderLabelInvalidColor
            ]
        ]

        "label-text" => [
            color borderLabelColor
        ] && [
            "is-invalid" => [
                color borderLabelInvalidColor
            ]
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        borderLabelColor        = Color.Grey "44",
        borderLabelInvalidColor = Color.DevRed,
        textColor               = Color.Grey "44",
        invalidReasonColor      = Color.DevRed,
        iconSize                = 20
    )
]))
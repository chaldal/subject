module LibClient.Components.Input.DayOfTheWeekStyles

open ReactXP.LegacyStyles

let private size = 38
let private gap  = 10

let private baseStyles = lazy (asBlocks [
    "days" => [
        FlexDirection.Row
        Overflow.Visible
        marginBottom 10
    ]

    "label" => [
        marginBottom 15
    ]

    "invalid-reason" => [
        fontSize 12
    ]

    "day" => [
        AlignItems.Center
        JustifyContent.Center
        Overflow.VisibleForTapCapture
        width        size
        height       size
        marginRight  gap
        borderRadius (size / 2)
    ]

    "day-label" => [
        FontWeight.Bold
        fontSize 16
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Input.DayOfTheWeek", baseStyles)

    static member All (unselectedTextColor: Color, unselectedBackgroundColor: Color, selectedTextColor: Color, selectedBackgroundColor: Color, labelColor: Color, invalidColor: Color) : unit =
        customize [
            Theme.Rules(unselectedTextColor, unselectedBackgroundColor, selectedTextColor, selectedBackgroundColor, labelColor, invalidColor)
        ]

    static member One (unselectedTextColor: Color, unselectedBackgroundColor: Color, selectedTextColor: Color, selectedBackgroundColor: Color, labelColor: Color, invalidColor: Color) : Styles =
        Theme.Rules(unselectedTextColor, unselectedBackgroundColor, selectedTextColor, selectedBackgroundColor, labelColor, invalidColor) |> makeSheet

    static member Rules (unselectedTextColor: Color, unselectedBackgroundColor: Color, selectedTextColor: Color, selectedBackgroundColor: Color, labelColor: Color, invalidColor: Color) : List<ISheetBuildingBlock> = [
        "day" => [
            backgroundColor unselectedBackgroundColor
        ] && [
            "selected" => [
                backgroundColor selectedBackgroundColor
            ]
        ]

        "day-label" => [
            color unselectedTextColor
        ] && [
            "selected" => [
                color selectedTextColor
            ]
        ]

        "label-text" => [
            color labelColor
        ] && [
            "invalid" => [
                color invalidColor
            ]
        ]

        "invalid-reason" => [
            color invalidColor
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        unselectedTextColor       = Color.Black,
        unselectedBackgroundColor = Color.White,
        selectedTextColor         = Color.DevRed,
        selectedBackgroundColor   = Color.DevGreen,
        labelColor                = Color.Black,
        invalidColor              = Color.DevRed
    )
]))

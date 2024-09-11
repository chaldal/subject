module LibClient.Components.DateSelectorStyles

open LibClient.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let headerText =
        makeTextStyles {
            FontWeight.Bold
            fontSize 16
            color Color.White
        }

    let navigationControlsText =
        makeTextStyles {
            fontSize 12
            color (Color.Grey "33")
        }

    let iconButtonTheme (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = Color.Grey "99"
                }
        }

open ReactXP.LegacyStyles

let private dayWidth = 30

let private baseStyles = lazy (asBlocks [
    "view" => [
        width    270
        minWidth (7 * dayWidth)
        height   330
    ]

    "header" => [
        paddingTop        40
        paddingBottom     5
        paddingHorizontal 10
    ]

    "header-text" => [
        FontWeight.Bold
        fontSize 16
        color    Color.White
    ]

    "navigation-controls" => [
        JustifyContent.SpaceBetween
        FlexDirection.Row
        AlignItems.Center
        marginVertical    10
        paddingHorizontal 10
    ]

    "arrow-container" => [
        FlexDirection.Row
    ]

    "weekday-headers-row" => [
        FlexDirection.Row
        JustifyContent.SpaceBetween
    ]

    "weekday-header" => [
        width  dayWidth
        height 15
    ]

    "weekday-header-text" => [
        color    (Color.Grey "33")
        fontSize 12
        TextAlign.Center
    ]

    "row" => [
        FlexDirection.Row
        JustifyContent.SpaceBetween
    ]

    "day" => [
        JustifyContent.Center
        Cursor.Pointer
        height dayWidth
        width  dayWidth
    ] && [
        "other-month" => [
            Cursor.Default
        ]
        "selected" => [
            borderColor  (Color.Hex "#c5d7ff")
            borderRadius (dayWidth / 2)
        ]
    ]

    "day-text" => [
        TextAlign.Center
        fontSize 12
        color    Color.Black
    ] && [
        "other-month" => [
            color (Color.Grey "aa")
            Cursor.Default
        ]
        "today" => [
            FontWeight.Bold
        ]
        "disabled" => [
            color (Color.Grey "cc")
        ]
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.DateSelector", baseStyles)

    static member One (headerBackgroundColor: Color, selectedDateBackgroundColor: Color) : Styles =
        Theme.Rules(headerBackgroundColor, selectedDateBackgroundColor) |> makeSheet

    static member All (headerBackgroundColor: Color, selectedDateBackgroundColor: Color) : unit =
        customize [
            Theme.Rules(headerBackgroundColor, selectedDateBackgroundColor)
        ]

    static member Rules (headerBackgroundColor: Color, selectedDateBackgroundColor: Color) : List<ISheetBuildingBlock> = [
        "header" => [
            backgroundColor headerBackgroundColor
        ]

        "day && selected" => [
            backgroundColor selectedDateBackgroundColor
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (headerBackgroundColor = Color.DevGreen, selectedDateBackgroundColor = Color.Hex "#becff7")
]))

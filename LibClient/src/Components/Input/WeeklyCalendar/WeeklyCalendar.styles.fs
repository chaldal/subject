module LibClient.Components.Input.WeeklyCalendarStyles

open ReactXP.LegacyStyles
open LibClient.Responsive

let private dayWidthDesktop:  int = 54
let private dayWidthHandheld: int = 47

let private baseStyles = lazy (asBlocks [
    "view" => [
        flex 1
        FlexDirection.Row
        AlignItems.Center
    ]

    "center" => [
        JustifyContent.Center
        AlignItems.Center
    ]

    "weekly-calendar-container" => [
        minWidth   275
        flexShrink 1
    ]

    "weekly-calendar" => [
        FlexDirection.Row
        Overflow.VisibleForScrolling
        marginTop 20
    ] && [
        ScreenSize.Desktop.Class => [
            JustifyContent.Center
        ]
    ]

    "day" => [
        FlexDirection.Column
        AlignItems.Center
        height 60
    ] && [
        ScreenSize.Desktop.Class => [
            width dayWidthDesktop
        ]

        ScreenSize.Handheld.Class => [
            width dayWidthHandheld
        ]
    ]

    "day-of-week" => [
        FontWeight.Bold
        fontSize 16
    ]

    "date" => [
        AlignSelf.Stretch
        FlexDirection.Column
        JustifyContent.Center
        AlignItems.Center
        height 30
    ]

    "date-text" => [
        FontWeight.Bold
        fontSize 16
    ]

    "circle && " + ScreenSize.Desktop.Class => (
        let size = 30
        [
            Position.Absolute
            height          size
            width           size
            top             0
            left            ((dayWidthDesktop - size) / 2)
            borderRadius    (size / 2)
        ]
    )

    "circle && " + ScreenSize.Handheld.Class => (
        let size = 30
        [
            height          size
            width           size
            top             0
            left            ((dayWidthHandheld - size) / 2)
            borderRadius    (size / 2)
            Position.Absolute
            Overflow.Hidden
        ]
    )

    "label" => [
        marginBottom 4
    ]

    "invalid-reason" => [
        fontSize 12
    ]
])

type Colors = {
    DayOfWeekText:       Color
    DateTextUnavailable: Color
    DateTextAvailable:   Color
    DateTextSelected:    Color
    Circle:              Color
    InvalidReason:       Color
}

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Input.WeeklyCalendar", baseStyles)

    static member One (colors: Colors) : Styles =
        Theme.Rules colors |> makeSheet

    static member All (colors: Colors) : unit =
        customize [
            Theme.Rules colors
        ]

    static member Rules (colors: Colors) : List<ISheetBuildingBlock> = [
        "day-of-week" => [
            color colors.DayOfWeekText
        ]

        "date-text" => [
            color colors.DateTextUnavailable
        ] && [
            "available" => [
                color colors.DateTextAvailable
            ]

            "selected" => [
                color colors.DateTextSelected
            ]
        ]

        "circle" => [
            backgroundColor colors.Circle
        ]

        "invalid-reason" => [
            color colors.InvalidReason
        ]

        "invalid" => [
            color colors.InvalidReason
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules ({
        DayOfWeekText       = Color.DevRed
        DateTextUnavailable = Color.DevRed
        DateTextAvailable   = Color.DevRed
        DateTextSelected    = Color.DevRed
        Circle              = Color.DevRed
        InvalidReason       = Color.DevRed
    })
]))

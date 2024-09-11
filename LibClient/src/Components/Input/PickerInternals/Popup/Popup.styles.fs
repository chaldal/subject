module LibClient.Components.Input.PickerInternals.PopupStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "scroll-view" => [
        // TODO this should probably be driven by screen height, but hardcoding for now
        maxHeight 400
        shadow    (Color.BlackAlpha 0.3) 5 (0, 2)
    ]

    "view" => [
        border          1 (Color.Grey "cc")
        backgroundColor Color.White
        Overflow.VisibleForScrolling
    ]

    "item" => [
        FlexDirection.Row
        AlignItems.Center
        Cursor.Pointer
        paddingHorizontal 18
        borderTop         1 (Color.Grey "cc")
    ] && [
        "first" => [
            borderTopWidth 0
        ]

        "highlighted" => [
            backgroundColor (Color.Grey "ee")
        ]
    ]

    "item" => [
        paddingLeft     16
        paddingRight    8
        paddingVertical 9
        FlexDirection.Row
    ]

    "item-selectedness" => [
        width 24
        flex  0
    ]

    "item-body" => [
        flex 1
    ]

    "item-selected-icon" => [
        fontSize 16
        color (Color.Grey "cc")
    ]

    "item-label" => [
        color    (Color.Grey "66")
        fontSize 16
    ] && [
        "highlighted" => [
            color (Color.Grey "33")
        ]
    ]

    "no-items-message" => [
        paddingHV 20 20
        AlignItems.Center
    ]

    "no-items-message-text" => [
        color (Color.Grey "66")
    ]

    "activity-indicator-block" => [
        FlexDirection.Row
        JustifyContent.Center
        AlignItems.Center
        paddingVertical 12
    ]

    "activity-indicator-overlay" => [
        Position.Absolute
        trbl 0 0 0 0
        FlexDirection.Row
        JustifyContent.Center
        AlignItems.Center
        backgroundColor (Color.WhiteAlpha 0.5)
    ]
])

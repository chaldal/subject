module LibClient.Components.ContextMenu.PopupStyles

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

    "button" => [
        FlexDirection.Row
        AlignItems.Center
        Cursor.Pointer
        height            36
        paddingHorizontal 18
        borderTop         1 (Color.Grey "cc")
    ] && [
        "first" => [
            borderTopWidth 0
        ]
    ]

    "button-text" => [
        color    (Color.Grey "66")
        fontSize 14
    ] && [
        "selected" => [
            FontWeight.Bold
        ]

        "button-cautionary" => [
            color Color.DevRed
        ]
    ]
])

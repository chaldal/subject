module LibClient.Components.Dialog.Shell.WhiteRounded.StandardStyles

open ReactXP.LegacyStyles
open LibClient.Responsive

let styles = lazy (compile [
    "dialog-contents" => [
        flex      1
        minWidth  200
        minHeight 100
    ] && [
        ScreenSize.Desktop.Class => [
            paddingHorizontal 40
            paddingTop        40
            paddingBottom     24
        ]

        ScreenSize.Handheld.Class => [
            padding 14
        ]
    ]

    "heading" => [
        flex             0
        marginHorizontal 20
    ]

    "scroll-view" => [
        flex 1
    ]

    "heading" => [] && [
        ScreenSize.Desktop.Class => [
            fontSize     32
            marginBottom 40
        ]

        ScreenSize.Handheld.Class => [
            fontSize     20
            marginBottom 22
        ]
    ]

    "body" => [
        flex 1
    ]

    "buttons" => [
        FlexDirection.Row
        JustifyContent.Center
        marginTop 40
    ]

    "in-progress" => [
        Position.Absolute
        trbl 0 0 0 0
        backgroundColor (Color.WhiteAlpha 0.5)
    ]

    "error" => [
        TextAlign.Center
        marginTop 20
        color     Color.DevRed
    ]
])

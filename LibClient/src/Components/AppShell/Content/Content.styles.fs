module LibClient.Components.AppShell.ContentStyles

open ReactXP.Styles

[<RequireQualifiedAccessAttribute>]
module Styles =
    let scrim =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
        }

open ReactXP.LegacyStyles

let styles = lazy (
    let staticStyles = compile [
        "safe-insets-view" => [
            flex 1
            backgroundColor Color.White
        ]

        "view" => [
            flex 1
        ]

        "invisible" => [
            opacity 0.
        ]

        "top-nav-block" => [
            Overflow.Hidden
        ]

        "bottom-nav-block" => [
            Overflow.VisibleForDropShadow
        ]

        "sidebar-and-content-block" => [
            FlexDirection.Row
            AlignItems.Stretch
            flex 1
        ]

        "content-block" => [
            flex 1
        ]

        "desktop" => [] && [
            "sidebar-block" => [
                FlexDirection.Row
                AlignItems.Stretch
                flex 0
            ]
        ]

        "sidebar-draggable" => [
            Position.Absolute
            top    0
            bottom 0
            left   0
        ]

        "sidebar-wrapper" => [
            Position.Absolute
            top 0
            bottom 0
            paddingRight 10
        ]

        "scrim" => [
            Position.Absolute
            trbl 0 0 0 0
        ]

        "sidebar-popup-wrapper" => [
            borderBottom 1 (Color.Grey "cc")
            shadow       (Color.BlackAlpha 0.3) 4 (0, 1)
            // Just setting to a reasonable size that will avoid cutting off the content, but also not allow it to extend off screen.
            // We may want to introduce a smarter calculation here at some point, perhaps based on screen size or client area.
            maxHeight    750
        ]

        "top-nav-shadow" => [
            Position.Absolute
            flex         0
            right        0
            left         0
            height       3
            marginTop    -3
            shadow       (Color.BlackAlpha 0.2) 3 (0, 2)
            borderBottom 1 (Color.Grey "cc")
        ]
    ]

    staticStyles
)

module LibClient.Components.Dialog.BaseStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "background" => [
        Position.Absolute
        trbl 0 0 0 0
        backgroundColor (Color.BlackAlpha 0.7)
    ]

    "dialog" => [
        Position.Absolute
        trbl 0 0 0 0
    ]

    "position-wrapper" => [
        Position.Absolute
        trbl 0 0 0 0
        FlexDirection.Column
        AlignItems.Center
    ]
    && [
        "position-CenterTop" => [
            JustifyContent.FlexStart
        ]
        "position-Center" => [
            JustifyContent.Center
        ]
    ]
])

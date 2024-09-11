module LibClient.Components.TriStateful.SimpleStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "shield" => [
        Position.Absolute
        FlexDirection.Row
        JustifyContent.Center
        AlignItems.Center
        trbl            0 0 0 0
        backgroundColor (Color.WhiteAlpha 0.8)
    ]

    "error" => [
        JustifyContent.FlexStart
        paddingLeft 24
    ]

    "message" => [
        color Color.DevRed
        marginRight 24
    ]
])

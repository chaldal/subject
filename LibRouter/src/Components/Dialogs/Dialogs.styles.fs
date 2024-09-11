module LibRouter.Components.DialogsStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        Position.Absolute
        trbl 0 0 0 0
    ]

    "frame" => [
        Position.Absolute
        trbl            0 0 0 0
        backgroundColor (Color.BlackAlpha 0.3)
    ]

    "sentinel" => [
        Position.Absolute
        trbl 0 0 0 0
    ]
])

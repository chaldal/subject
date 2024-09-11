module LibAutoUi.Components.DialogInputFormStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "dialog-contents" => [
        minWidth  200
        minHeight 200
        Overflow.Visible
        JustifyContent.SpaceBetween
    ]

    "buttons" => [
        FlexDirection.Row
        JustifyContent.Center
        marginTop 12
    ]
])

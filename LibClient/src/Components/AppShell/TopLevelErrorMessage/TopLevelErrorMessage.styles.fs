module LibClient.Components.AppShell.TopLevelErrorMessageStyles

open ReactXP.LegacyStyles
open LibClient.Responsive

let styles = lazy (compile [
    "view" => [
        flex    1
        padding 80
        FlexDirection.Column
        AlignItems.Center
        JustifyContent.Center
        backgroundColor Color.White
    ]

    "center" => [
        AlignItems.Center
    ]

    "icon" => [
        fontSize 320
    ] && [
        ScreenSize.Handheld.Class => [
            fontSize  220
            marginTop -100
        ]
    ]

    "error-heading" => [
        marginTop 20
        color     (Color.Hex "#db5f5f")
        FontWeight.W400
    ]

    "error-title" => [
        marginTop 15
        TextAlign.Center
    ]

    "error-subtitle" => [
        marginTop    10
        marginBottom 20
        TextAlign.Center
    ]

    "button" => [
        marginBottom 50
    ]
])

module AppEggShellGallery.Components.Route.HomeStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy (compile [
    "content" => [
        FlexDirection.Column
        AlignItems.Center
    ]

    "content-text" => [
        fontSize   16
        lineHeight (16. * 1.5 |> int)
    ]

    "logo-image" => [
        height 300
        width  400
    ]

    "subtitle" => [
        fontSize     16
        marginTop    20
        marginBottom 20
    ]

    "table" => [
        width 500
        marginRight 32 // for visual vertical centring
    ]

    "row" => [
        FlexDirection.Row
        marginBottom 16
    ]

    "cell-left" => [
        flex  0
        width 120
        marginRight 20
        JustifyContent.Center
    ]

    "label" => [
        FontWeight.Bold
        TextAlign.Right
    ]

    "cell-right" => [
        flex 1
    ]
])

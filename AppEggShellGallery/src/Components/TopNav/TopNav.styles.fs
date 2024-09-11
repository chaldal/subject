module AppEggShellGallery.Components.TopNavStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy (compile [
    "logo" => [
        marginRight 20
        paddingTop  5 // icon vertical alignment
    ]

    "logo-icon" => [
        fontSize 64
        color    colors.Primary.B050
    ]

    "sample-visuals-screen-size" => [
        FlexDirection.Row
        AlignItems.Center
        marginRight 30
    ]

    "label" => [
        marginRight 6
        color Color.White
    ]
])

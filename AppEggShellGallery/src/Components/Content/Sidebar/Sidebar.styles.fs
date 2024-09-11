module AppEggShellGallery.Components.Content.SidebarStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy (compile [
    "sidebar" => [
        height 600
    ]

    "profile" => [
        padding 18
    ]

    "name" => [
        FontWeight.Bold
        fontSize  18
        color     colors.Neutral.B600
        marginTop 12
    ]

    "email" => [
        fontSize 14
        color    colors.Neutral.B500
    ]
])

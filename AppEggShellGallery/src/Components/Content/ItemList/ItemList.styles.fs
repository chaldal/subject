module AppEggShellGallery.Components.Content.ItemListStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy (compile [
    "empty-message" => [
        paddingVertical 20
    ]

    "empty-message-text" => [
        TextAlign.Center
        color    colors.Caution.Main
        fontSize 18
    ]

    "custom-see-all" => [
        JustifyContent.Center
    ]
])
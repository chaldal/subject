module AppEggShellGallery.Components.Content.ImageCardStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "metadata" => [
        FlexDirection.Row
        JustifyContent.SpaceBetween
        paddingHV 20 10
    ]

    "title" => [
        FontWeight.Bold
        fontSize 18
        color    Color.White
    ]

    "author" => [
        fontSize 14
        color    Color.White
    ]
])

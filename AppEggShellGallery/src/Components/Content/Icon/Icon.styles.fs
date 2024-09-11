module AppEggShellGallery.Components.Content.IconStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "a" => [
        color    (Color.Hex "#fa6e1b")
        fontSize 50
    ]

    "b" => [
        color (Color.Hex "#9b48db")
    ]
])

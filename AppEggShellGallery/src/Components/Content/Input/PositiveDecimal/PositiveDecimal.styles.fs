module AppEggShellGallery.Components.Content.Input.PositiveDecimalStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        borderWidth 4
        borderColor Color.DevRed
    ]
])

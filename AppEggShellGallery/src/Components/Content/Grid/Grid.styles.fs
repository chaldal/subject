module AppEggShellGallery.Components.Content.GridStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        borderWidth 4
        borderColor Color.DevRed
    ]
])

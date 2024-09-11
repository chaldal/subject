module AppEggShellGallery.Components.Content.FormsStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        borderWidth 4
        borderColor Color.DevRed
    ]
])

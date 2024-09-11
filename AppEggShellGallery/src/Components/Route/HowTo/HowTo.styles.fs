module AppEggShellGallery.Components.Route.HowToStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "message" => [
        TextAlign.Center
        padding   10
    ]
])

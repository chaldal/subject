module AppEggShellGallery.Components.Route.ComponentsStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "message" => [
        TextAlign.Center
        padding   10
    ]
])

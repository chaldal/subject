module AppEggShellGallery.Components.Route.DesignStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "message" => [
        TextAlign.Center
        padding   10
    ]
])

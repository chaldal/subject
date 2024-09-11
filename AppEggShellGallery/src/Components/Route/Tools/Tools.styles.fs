module AppEggShellGallery.Components.Route.ToolsStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "message" => [
        TextAlign.Center
        padding   10
    ]
])

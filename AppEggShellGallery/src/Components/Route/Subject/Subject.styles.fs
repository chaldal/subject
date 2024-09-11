module AppEggShellGallery.Components.Route.SubjectStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "message" => [
        TextAlign.Center
        padding   10
    ]
])

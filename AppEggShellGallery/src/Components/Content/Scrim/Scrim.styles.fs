module AppEggShellGallery.Components.Content.ScrimStyles

open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let scrim =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
        }

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "sample-block" => [
        width  300
        height 300
    ]
])

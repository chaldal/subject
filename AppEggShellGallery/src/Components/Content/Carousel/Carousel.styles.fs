module AppEggShellGallery.Components.Content.CarouselStyles

open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let carousel =
        makeViewStyles {
            height 200
        }

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "image" => [
        height 200
    ]
])

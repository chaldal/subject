module AppEggShellGallery.Components.Content.PreStyles

open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let pre =
        makeTextStyles {
            backgroundColor (Color.Grey "42")
            color           Color.White
            padding         10
        }

open ReactXP.LegacyStyles

let styles = lazy RuntimeStyles.None

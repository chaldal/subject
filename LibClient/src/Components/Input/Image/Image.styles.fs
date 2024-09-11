module LibClient.Components.Input.ImageStyles

open ReactXP.Styles

module Styles =
    let imageThumbs =
        makeViewStyles {
            AlignItems.Center
            JustifyContent.Center
        }

open ReactXP.LegacyStyles

let styles = lazy RuntimeStyles.None
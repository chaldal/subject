module AppEggShellGallery.Components.Content.IconButtonStyles

open LibClient.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let specialTheme (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = Color.DevBlue
                    IconSize = 42
                }
        }

open ReactXP.LegacyStyles

let styles = lazy RuntimeStyles.None
module AppEggShellGallery.Components.Content.ButtonStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "btn-small" ==> LibClient.Components.ButtonStyles.Theme.IconSize 15

    "caution" ==>
        LibClient.Components.ButtonStyles.Theme.One (
            level              = LibClient.Components.Button.Cautionary,
            state              = LibClient.Input.ButtonLowLevelState.Actionable ignore,
            theTextColor       = Color.Black,
            theBorderColor     = Color.White,
            theBackgroundColor = Color.DevOrange
        )

    "small-icon" ==> LibClient.Components.ButtonStyles.Theme.IconSize 8
    
    "button-with-badge" ==> LibClient.Components.ButtonStyles.Theme.BadgeColor(Color.DevGreen)
])
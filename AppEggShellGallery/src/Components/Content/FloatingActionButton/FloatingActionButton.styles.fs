module AppEggShellGallery.Components.Content.FloatingActionButtonStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "special" ==> LibClient.Components.FloatingActionButtonStyles.Theme.One (LibClient.Input.ButtonLowLevelState.Actionable ignore, theBackgroundColor = Color.White, iconColor = Color.DevBlue,  iconSize = 32)
])

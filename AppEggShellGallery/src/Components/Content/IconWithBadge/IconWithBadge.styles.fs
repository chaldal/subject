module AppEggShellGallery.Components.Content.IconWithBadgeStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "icon-with-badge" ==> LibClient.Components.IconWithBadgeStyles.Theme.One (theIconColor = Color.Black, theIconSize = 26)
])

module AppEggShellGallery.Components.Content.DateSelectorStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy (compile [
    "special" ==> LibClient.Components.DateSelectorStyles.Theme.One (Color.Hex "#6200ee", Color.Hex "#a7aeff")
])

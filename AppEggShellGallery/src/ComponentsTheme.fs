module AppEggShellGallery.ComponentsTheme

open AppEggShellGallery.Colors
open LibClient
open LibClient.Components

let applyTheme () : unit =
    LibClient.DefaultComponentsTheme.ApplyTheme.primarySecondary colors
    LibRouter.DefaultComponentsTheme.ApplyTheme.primarySecondary colors

    Themes.Set<LC.Text.Theme> {
        FontFamily = "Montserrat"
    }

    Themes.Set<LC.UiText.Theme> {
        FontFamily = "Montserrat"
    }

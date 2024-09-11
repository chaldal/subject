module AppEggShellGallery.Colors

open LibClient.ColorScheme

type AppEggshellGalleryColorScheme() =
    inherit ColorSchemeWithDefaults()

    override this.Primary   : Variants = MaterialDesignColors.Purple
    override this.Secondary : Variants = MaterialDesignColors.Green

let colors = AppEggshellGalleryColorScheme()

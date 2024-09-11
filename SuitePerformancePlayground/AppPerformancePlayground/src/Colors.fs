module AppPerformancePlayground.Colors

open LibClient.ColorModule
open LibClient.ColorScheme

type AppPerformancePlaygroundColorScheme() =
    inherit ColorSchemeWithDefaults()

    override this.Primary   : Variants = MaterialDesignColors.``Light Green``
    override this.Secondary : Variants = MaterialDesignColors.Cyan

    // example of how to add additional colours outside of the regular scheme
    member _.SomeSpecialColor = Color.Hex "#abc123"

let colors = AppPerformancePlaygroundColorScheme()

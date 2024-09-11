module LibUiAdmin.DefaultComponentsTheme

open LibClient
open ReactXP.Styles
open LibUiAdmin.Components


let private applyTheme (_primary: Variants) (_secondary: Variants) (_neutral: Variants) (_attention: Variants) (_caution: Variants) : unit =
    Themes.Set<UiAdmin.LabelledValues.Theme>(
        {
            LabelColor  = Color.Black
            LabelIsBold = true
        }
    )

module ApplyTheme =
    let primaryPrimary (scheme: ColorScheme) : unit =
        applyTheme scheme.Primary scheme.Primary scheme.Neutral scheme.Attention scheme.Caution

    let primarySecondary (scheme: ColorScheme) : unit =
        applyTheme scheme.Primary scheme.Secondary scheme.Neutral scheme.Attention scheme.Caution

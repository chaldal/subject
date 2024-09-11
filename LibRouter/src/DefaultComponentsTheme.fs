module LibRouter.DefaultComponentsTheme

open LibClient
open LibRouter.Components
open ReactXP.LegacyStyles

type RFontWeight = RulesRestricted.FontWeight

let private applyTheme (primary: Variants) (_secondary: Variants) (_neutral: Variants) (_attention: Variants) (_caution: Variants) : unit =
    Themes.Set<LR.Legacy.TopNav.BackButtonTypes.Theme> {
        IconColor = Color.White
    }

    Themes.Set<LR.Nav.Top.BackButtonTypes.Theme> {
        Actionable = {
            BaseColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = primary.MainMinus2
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed
            }
            HoveredColors = {
                Label = primary.Main
                LabelWeight = RFontWeight.Normal
                Background = primary.B050
                Border = primary.MainMinus2
                Icon = primary.Main
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed
            }
            DepressedColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = primary.MainMinus2
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed
            }
        }
        InProgress = {
            BaseColors = {
                Label = Color.Transparent
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = Color.Transparent
                Icon = Color.Transparent
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            HoveredColors = {
                Label = Color.Transparent
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = Color.Transparent
                Icon = Color.Transparent
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            DepressedColors = {
                Label = Color.Transparent
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = Color.Transparent
                Icon = Color.Transparent
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
        }
        Disabled = {
            BaseColors = {
                Label = primary.MainMinus2
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = Color.Transparent
                Icon = primary.MainMinus2
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            HoveredColors = {
                Label = primary.MainMinus2
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = Color.Transparent
                Icon = primary.MainMinus2
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            DepressedColors = {
                Label = primary.MainMinus2
                LabelWeight = RFontWeight.Normal
                Background = primary.Main
                Border = Color.Transparent
                Icon = primary.MainMinus2
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
        }
        Selected = {
            BaseColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Bold
                Background = primary.Main
                Border = primary.B050
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            HoveredColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Bold
                Background = primary.Main
                Border = primary.B050
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            DepressedColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Bold
                Background = primary.Main
                Border = primary.B050
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
        }
        SelectedActionable = {
            BaseColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Bold
                Background = primary.Main
                Border = primary.B050
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            HoveredColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Bold
                Background = primary.Main
                Border = primary.B050
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
            DepressedColors = {
                Label = primary.B050
                LabelWeight = RFontWeight.Bold
                Background = primary.Main
                Border = primary.B050
                Icon = primary.B050
                BadgeFontWeight = RulesRestricted.FontWeight.Bold
                BadgeFontColor = Color.White
                BadgeBackgroundColor = Color.DevRed;
            }
        }
    }

module ApplyTheme =
    let primaryPrimary (scheme: ColorScheme) : unit =
        applyTheme scheme.Primary scheme.Primary scheme.Neutral scheme.Attention scheme.Caution

    let primarySecondary (scheme: ColorScheme) : unit =
        applyTheme scheme.Primary scheme.Secondary scheme.Neutral scheme.Attention scheme.Caution

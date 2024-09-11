module AppEggShellGallery.Components.Content.TagStyles

open LibClient.Components
open ReactXP.Styles

let cautionTheme (theme: LC.Tag.Theme) : LC.Tag.Theme =
    { theme with
        Tags =
            { theme.Tags with
                Selected =
                    { theme.Tags.Selected with
                        TextColor = Color.White
                        BackgroundColor = Color.DevRed
                    }
                Unselected =
                    { theme.Tags.Selected with
                        TextColor = Color.White
                        BackgroundColor = Color.DevOrange
                    }
            }
        Sizes =
            { theme.Sizes with
                Desktop =
                    { theme.Sizes.Desktop with
                        FontSize = 20
                    }
            }
    }

open ReactXP.LegacyStyles

let styles = lazy RuntimeStyles.None
module LibClient.DefaultComponentsTheme


open ReactXP.LegacyStyles
open LibClient
open LibClient.Components
open LibClient.ColorModule
open LibClient.ColorScheme
open LibClient.Responsive

type RFontWeight = RulesRestricted.FontWeight

let private applyTheme (primary: Variants) (secondary: Variants) (neutral: Variants) (attention: Variants) (caution: Variants) : unit =
    let defaultIconSize = 32

    Themes.Set<LC.Avatar.Theme> {
        Size = 60
    }

    Themes.Set<LC.Text.Theme> {
        FontFamily = "sans-serif"
    }

    Themes.Set<LC.UiText.Theme> {
        FontFamily = "sans-serif"
    }

    Themes.Set<LC.HeaderCell.Theme> {
        FontColor = Color.Grey "45"
        FontSize = 18
    }

    BadgeStyles.Theme.All(
        theFontSize        = 14,
        theFontColor       = Color.White,
        theBackgroundColor = caution.Main,
        theFontWeight      = RulesRestricted.FontWeight.Normal
    )

    ButtonStyles.Theme.Customize [
        //                                                                   theTextColor    theBorderColor     theBackgroundColor
        (ButtonStyles.Theme.Part(Button.Level.Primary,    Actionable ignore, Color.White,    secondary.Main,    secondary.Main))
        (ButtonStyles.Theme.Part(Button.Level.Primary,    Disabled,          Color.White,    secondary.Main,    secondary.Main))
        (ButtonStyles.Theme.Part(Button.Level.Primary,    InProgress,        Color.White,    secondary.Main,    secondary.Main))

        (ButtonStyles.Theme.Part(Button.Level.Secondary,  Actionable ignore, secondary.Main, secondary.Main,    Color.White))
        (ButtonStyles.Theme.Part(Button.Level.Secondary,  Disabled,          secondary.Main, secondary.Main,    Color.White))
        (ButtonStyles.Theme.Part(Button.Level.Secondary,  InProgress,        secondary.Main, secondary.Main,    Color.White))

        (ButtonStyles.Theme.Part(Button.Level.Tertiary,   Actionable ignore, secondary.Main, Color.Transparent, Color.Transparent))
        (ButtonStyles.Theme.Part(Button.Level.Tertiary,   Disabled,          secondary.Main, Color.Transparent, Color.Transparent))
        (ButtonStyles.Theme.Part(Button.Level.Tertiary,   InProgress,        secondary.Main, Color.Transparent, Color.Transparent))

        (ButtonStyles.Theme.Part(Button.Level.Cautionary, Actionable ignore, Color.White,    caution.Main,       caution.Main))
        (ButtonStyles.Theme.Part(Button.Level.Cautionary, Disabled,          Color.White,    caution.Main,       caution.Main))
        (ButtonStyles.Theme.Part(Button.Level.Cautionary, InProgress,        Color.White,    caution.Main,       caution.Main))
    ]

    DateSelectorStyles.Theme.All(
        headerBackgroundColor       = primary.Main,
        selectedDateBackgroundColor = primary.B100
    )

    Themes.Set<LC.TextButton.Theme>(
        {
            Primary = {
                Actionable = {
                    TextColor = secondary.Main
                    FontSize = 14
                    Opacity = 1.0
                }
                Disabled = {
                    TextColor = neutral.B500
                    FontSize = 14
                    Opacity = 0.5
                }
                InProgress = {
                    TextColor = neutral.B500
                    FontSize = 14
                    Opacity = 1.0
                }
            }
            Secondary = {
                Actionable = {
                    TextColor = secondary.Main
                    FontSize = 10
                    Opacity = 1.0
                }
                Disabled = {
                    TextColor = secondary.Main
                    FontSize = 10
                    Opacity = 0.5
                }
                InProgress = {
                    TextColor = secondary.Main
                    FontSize = 10
                    Opacity = 1.0
                }
            }
        }
    )

    Themes.Set<LC.ToggleButton.Theme>(
        {
            Selected =
                {
                    TextColor = Color.White
                    BorderColor = secondary.MainPlus1
                    BackgroundColor = secondary.Main
                }
            Unselected =
                {
                    TextColor = Color.White
                    BorderColor = neutral.B400
                    BackgroundColor = neutral.B300
                }
        }
    )

    FloatingActionButtonStyles.Theme.Customize [
        FloatingActionButtonStyles.Theme.Part (Actionable ignore, theBackgroundColor = secondary.Main, iconColor = Color.White,    iconSize = defaultIconSize)
        FloatingActionButtonStyles.Theme.Part (InProgress,        theBackgroundColor = secondary.Main, iconColor = secondary.Main, iconSize = defaultIconSize)
        FloatingActionButtonStyles.Theme.Part (Disabled,          theBackgroundColor = secondary.Main, iconColor = Color.White,    iconSize = defaultIconSize)
    ]

    Themes.Set<LC.IconButton.Theme> {
        Actionable =
            {
                IconColor = secondary.Main
                IconSize = defaultIconSize
                TapTargetMargin = (-10, -10, -10, -10)
            }
        Disabled =
            {
                IconColor = secondary.Main
                IconSize = defaultIconSize
                TapTargetMargin = (-10, -10, -10, -10)
            }
        InProgress =
            {
                IconColor = secondary.Main
                IconSize = defaultIconSize
                TapTargetMargin = (-10, -10, -10, -10)
            }
    }

    Themes.Set<LC.Tag.Theme>(
        {
            Tags = {
                Selected = {
                    TextColor = Color.White
                    BackgroundColor = secondary.Main
                }
                Unselected = {
                    TextColor    = secondary.Main
                    BackgroundColor = secondary.B100
                }
            }
            Sizes = {
                Desktop = {
                    FontSize = 14
                    PaddingHorizontal = 15
                    PaddingVertical = 6
                }
                Handheld = {
                    FontSize = 12
                    PaddingHorizontal = 10
                    PaddingVertical = 4
                }
            }
        }
    )

    Themes.Set<LC.Input.DateTypes.Theme> {
        BorderLabelBlurredColor = Color.Grey "44"
        BorderLabelFocusedColor = Color.DevGreen
        BorderLabelInvalidColor = Color.DevRed
        TextColor = Color.Grey "44"
        NoneditableTextColor = Color.Grey "22"
        NoneditableBackgroundColor = Color.Grey "66"
        InvalidReasonColor = Color.DevRed
        PlaceholderColor = Color.Grey "aa"
        TheVerticalPadding = 10
        CalendarButtonColor = Color.DevGreen
        CalendarButtonBackgroundColor = Color.Transparent
        CalendarButtonIconSize = 20
    }

    Themes.Set<LC.Input.EmailAddressTypes.Theme> {
        BorderLabelBlurredColor    = neutral.B300
        BorderLabelFocusedColor    = secondary.Main
        BorderLabelInvalidColor    = caution.Main
        TextColor                  = neutral.Main
        NoneditableTextColor       = neutral.B500
        NoneditableBackgroundColor = neutral.B200
        InvalidReasonColor         = caution.Main
        PlaceholderColor           = neutral.B300
        TheVerticalPadding         = 10
    }

    Themes.Set<LC.Input.PhoneNumberTypes.Theme> {
        BorderLabelBlurredColor    = neutral.B300
        BorderLabelFocusedColor    = secondary.Main
        BorderLabelInvalidColor    = caution.Main
        TextColor                  = neutral.Main
        NoneditableTextColor       = neutral.B500
        NoneditableBackgroundColor = neutral.B200
        InvalidReasonColor         = caution.Main
        PlaceholderColor           = neutral.B300
        TheVerticalPadding         = 10
    }

    Themes.Set<LC.Input.QuantityTypes.Theme> {
        NormalColors = {
            Border = secondary.Main
            Value = neutral.Main
            Icons = secondary.Main
        }
        InvalidColors = {
            Border = caution.Main
            Value = neutral.Main
            Icons = caution.Main
        }
        InvalidMessageColor = caution.Main
    }

    Input.DayOfTheWeekStyles.Theme.All(
        unselectedTextColor       = neutral.Main,
        unselectedBackgroundColor = neutral.B050,
        selectedTextColor         = Color.White,
        selectedBackgroundColor   = secondary.Main,
        labelColor                = neutral.Main,
        invalidColor              = caution.Main
    )

    Input.FileStyles.Theme.All(
        invalidColor = caution.Main
    )

    Legacy.CardStyles.Theme.All(theBorderColor = neutral.B300)

    Themes.Set<LC.Card.Theme> LC.Card.Theme.ShadowedCard

    Themes.Set<LC.Thumb.Theme>({Size = 60; SelectedBorderColor = secondary.Main})

    Legacy.TopNav.BaseStyles.Theme.All(primary.Main, theTextColor = Color.White)

    Themes.Set<LC.Legacy.TopNav.IconButtonTypes.Theme> {
        IconColor = Color.White
        IconSize = 30
    }

    Themes.Set<LC.Carousel.Theme> {
        DotColor = Color.White
        SelectedDotColor = secondary.Main
        NavigationButtonColor = Color.White
        NavigationButtonBackgroundColor = Color.Transparent
        ButtonIconSize = defaultIconSize
        NavigationButtonStyle = None
        DotContainerStyle = None
        InactiveDotStyle = None
        ActiveDotStyle = None
    }

    Themes.Set<LC.Dialog.ImageViewer.Theme> {
        DotColor = Color.White
        SelectedDotColor = secondary.Main
        NavigationButtonColor = Color.White
        NavigationButtonBackgroundColor = Color.Grey "44"
        ButtonIconSize = 26
    }

    Input.TextStyles.Theme.All(
        borderLabelBlurredColor    = neutral.B500,
        borderLabelFocusedColor    = secondary.Main,
        borderLabelInvalidColor    = caution.Main,
        textColor                  = neutral.Main,
        noneditableTextColor       = neutral.B500,
        noneditableBackgroundColor = neutral.B200,
        invalidReasonColor         = caution.Main,
        placeholderColor           = neutral.B300,
        theVerticalPadding         = 10
    )

    Input.DecimalStyles.Theme.All(
        borderLabelBlurredColor    = neutral.B500,
        borderLabelFocusedColor    = secondary.Main,
        borderLabelInvalidColor    = caution.Main,
        textColor                  = neutral.Main,
        noneditableTextColor       = neutral.B500,
        noneditableBackgroundColor = neutral.B200,
        invalidReasonColor         = caution.Main,
        placeholderColor           = neutral.B300,
        theVerticalPadding         = 10
    )

    Input.PositiveIntegerStyles.Theme.All(
        borderLabelBlurredColor    = neutral.B500,
        borderLabelFocusedColor    = secondary.Main,
        borderLabelInvalidColor    = caution.Main,
        textColor                  = neutral.Main,
        noneditableTextColor       = neutral.B500,
        noneditableBackgroundColor = neutral.B200,
        invalidReasonColor         = caution.Main,
        placeholderColor           = neutral.B300,
        theVerticalPadding         = 10
    )

    Input.UnsignedDecimalStyles.Theme.All(
        borderLabelBlurredColor    = neutral.B500,
        borderLabelFocusedColor    = secondary.Main,
        borderLabelInvalidColor    = caution.Main,
        textColor                  = neutral.Main,
        noneditableTextColor       = neutral.B500,
        noneditableBackgroundColor = neutral.B200,
        invalidReasonColor         = caution.Main,
        placeholderColor           = neutral.B300,
        theVerticalPadding         = 10
    )

    Input.LocalTimeStyles.Theme.All(
        labelBlurredColor  = neutral.B500,
        labelFocusedColor  = secondary.Main,
        labelInvalidColor  = caution.Main,
        invalidReasonColor = caution.Main
    )

    Legacy.Input.PickerStyles.Theme.All(
        borderLabelColor        = neutral.B500,
        borderLabelInvalidColor = caution.Main,
        textColor               = neutral.Main,
        invalidReasonColor      = caution.Main,
        iconSize                = 20
    )

    Input.PickerInternals.FieldStyles.Theme.All(
        borderLabelColor        = neutral.B500,
        borderLabelFocusColor   = secondary.Main,
        borderLabelInvalidColor = caution.Main,
        textColor               = neutral.Main,
        invalidReasonColor      = caution.Main,
        placeholderColor        = neutral.B300,
        iconSize                = 20,
        theVerticalPadding      = 10
    )

    Input.CheckboxStyles.Theme.All(
        iconCheckedColor   = secondary.Main,
        iconUncheckedColor = secondary.Main,
        labelColor         = neutral.Main
    )

    InfoMessageStyles.Theme.All({
        Info      = neutral.Main
        Attention = attention.Main
        Caution   = caution.Main
    })

    Themes.Set<LC.Stars.Theme>(
        {
            OnColor = secondary.Main
            OffColor = neutral.B300
            IconSize = 20
        }
    )

    Themes.Set<LC.Tab.Theme>(
        {
            SelectedColor = secondary.Main
            UnselectedColor = neutral.B500
        }
    )

    TabsStyles.Theme.Customize [
        backgroundColor Color.White
        borderBottom    1 neutral.B700
    ]

    Sidebar.HeadingStyles.Theme.Customize [
        (Sidebar.HeadingStyles.Theme.Part(Sidebar.Heading.Level.Primary,   textColor = neutral.Main))
        (Sidebar.HeadingStyles.Theme.Part(Sidebar.Heading.Level.Primary,   textColor = neutral.B500))
        (Sidebar.HeadingStyles.Theme.Part(Sidebar.Heading.Level.Primary,   textColor = neutral.B500))

        (Sidebar.HeadingStyles.Theme.Part(Sidebar.Heading.Level.Secondary, textColor = neutral.Main))
        (Sidebar.HeadingStyles.Theme.Part(Sidebar.Heading.Level.Secondary, textColor = neutral.B500))
        (Sidebar.HeadingStyles.Theme.Part(Sidebar.Heading.Level.Secondary, textColor = neutral.B500))
    ]

    Sidebar.ItemStyles.Theme.Customize [
        (Sidebar.ItemStyles.Theme.Static(iconSize = 24, theFontSize = 16))
        (Sidebar.ItemStyles.Theme.Part(Sidebar.Item.State.Actionable ignore,
            baseColors      = { Label = neutral.Main; LabelWeight = RFontWeight.Normal; Background = Color.White;  Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; },
            hoveredColors   = { Label = neutral.Main; LabelWeight = RFontWeight.Normal; Background = primary.B050; Border = Color.Transparent; LeftIcon = primary.MainMinus2; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; },
            depressedColors = { Label = neutral.Main; LabelWeight = RFontWeight.Normal; Background = Color.White;  Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; }
        ))
        (Sidebar.ItemStyles.Theme.Part(Sidebar.Item.State.Selected,
            baseColors      = { Label = neutral.B700; LabelWeight = RFontWeight.Bold; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; },
            hoveredColors   = { Label = neutral.B700; LabelWeight = RFontWeight.Bold; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; },
            depressedColors = { Label = neutral.B700; LabelWeight = RFontWeight.Bold; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; }
        ))
        (Sidebar.ItemStyles.Theme.Part(Sidebar.Item.State.Disabled,
            baseColors      = { Label = neutral.B200; LabelWeight = RFontWeight.Normal; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B100; RightIcon = neutral.B100; BadgeBackground = caution.B200; BadgeText = Color.White; },
            hoveredColors   = { Label = neutral.B200; LabelWeight = RFontWeight.Normal; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B100; RightIcon = neutral.B100; BadgeBackground = caution.B200; BadgeText = Color.White; },
            depressedColors = { Label = neutral.B200; LabelWeight = RFontWeight.Normal; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B100; RightIcon = neutral.B100; BadgeBackground = caution.B200; BadgeText = Color.White; }
        ))
        (Sidebar.ItemStyles.Theme.Part(Sidebar.Item.State.InProgress,
            baseColors      = { Label = neutral.B200; LabelWeight = RFontWeight.Normal; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; },
            hoveredColors   = { Label = neutral.B200; LabelWeight = RFontWeight.Normal; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; },
            depressedColors = { Label = neutral.B200; LabelWeight = RFontWeight.Normal; Background = Color.White; Border = Color.Transparent; LeftIcon = neutral.B400; RightIcon = neutral.B400; BadgeBackground = caution.Main; BadgeText = Color.White; }
        ))
    ]

    Themes.Set<LC.AppGlobalStatus.Theme> {
        BackgroundColor = caution.Main
    }

    Nav.Top.BaseStyles.Theme.All(
        screenSizeToSizes = (function
            | ScreenSize.Desktop  -> { Height = 72; }
            | ScreenSize.Handheld -> { Height = 55; }
        ),
        theBackgroundColor = primary.Main
    )

    Nav.Top.HeadingStyles.Theme.All(
        screenSizeToSizes = (function
            | ScreenSize.Desktop  -> {| FontSize = 24; |}
            | ScreenSize.Handheld -> {| FontSize = 16; |}
        ),
        theColor = Color.White
    )

    Nav.Top.ItemStyles.Theme.Customize [
        Nav.Top.ItemStyles.Theme.Size(function
            | ScreenSize.Desktop  -> { IconSize = 24; FontSize = 16; Height = 72; MaxWidth = None; BadgeFontSize = 14; BadgeTop = -10; BadgeLeft = -10 }
            | ScreenSize.Handheld -> { IconSize = 20; FontSize = 14; Height = 42; MaxWidth = None; BadgeFontSize = 12; BadgeTop = -6;  BadgeLeft = -10 }
        )
        (Nav.Top.ItemStyles.Theme.Part(Nav.Top.Item.State.Actionable ignore,
            baseColors      = { Label = primary.B050; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = primary.MainMinus2; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.Main; LabelWeight = RFontWeight.Normal; Background = primary.B050; Border = primary.MainMinus2; Icon = primary.Main; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.B050; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = primary.MainMinus2; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Top.ItemStyles.Theme.Part(Nav.Top.Item.State.Selected,
            baseColors      = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Top.ItemStyles.Theme.Part(Nav.Top.Item.State.SelectedActionable ignore,
            baseColors      = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Top.ItemStyles.Theme.Part(Nav.Top.Item.State.Disabled,
            baseColors      = { Label = primary.MainMinus2; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = primary.MainMinus2; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.MainMinus2; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = primary.MainMinus2; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.MainMinus2; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = primary.MainMinus2; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Top.ItemStyles.Theme.Part(Nav.Top.Item.State.InProgress,
            baseColors      = { Label = Color.Transparent; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = Color.Transparent; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = Color.Transparent; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = Color.Transparent; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = Color.Transparent; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = Color.Transparent; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
    ]

    Nav.Bottom.BaseStyles.Theme.All(
        screenSizeToSizes = (function
            | ScreenSize.Desktop  -> { Height = 72; }
            | ScreenSize.Handheld -> { Height = 48; }
        ),
        theBackgroundColor = primary.Main
    )

    Nav.Bottom.ItemStyles.Theme.Customize [
        Nav.Bottom.ItemStyles.Theme.Size(function
            | ScreenSize.Desktop  -> { IconSize = 24; FontSize = 16; Height = 72; MaxWidth = None; BadgeFontSize = 14; BadgeTop = -10; BadgeLeft = -10 }
            | ScreenSize.Handheld -> { IconSize = 20; FontSize = 14; Height = 42; MaxWidth = None; BadgeFontSize = 12; BadgeTop = -6;  BadgeLeft = -10 }
        )
        (Nav.Bottom.ItemStyles.Theme.Part(Nav.Bottom.Item.State.Actionable ignore,
            baseColors      = { Label = primary.B050; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = primary.MainMinus2; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.Main; LabelWeight = RFontWeight.Normal; Background = primary.B050; Border = primary.MainMinus2; Icon = primary.Main; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.B050; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = primary.MainMinus2; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Bottom.ItemStyles.Theme.Part(Nav.Bottom.Item.State.Selected,
            baseColors      = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Bottom.ItemStyles.Theme.Part(Nav.Bottom.Item.State.SelectedActionable ignore,
            baseColors      = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.B050; LabelWeight = RFontWeight.Bold; Background = primary.Main; Border = primary.B050; Icon = primary.B050; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Bottom.ItemStyles.Theme.Part(Nav.Bottom.Item.State.Disabled,
            baseColors      = { Label = primary.MainMinus2; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = primary.MainMinus2; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = primary.MainMinus2; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = primary.MainMinus2; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = primary.MainMinus2; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = primary.MainMinus2; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
        (Nav.Bottom.ItemStyles.Theme.Part(Nav.Bottom.Item.State.InProgress,
            baseColors      = { Label = Color.Transparent; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = Color.Transparent; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            hoveredColors   = { Label = Color.Transparent; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = Color.Transparent; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; },
            depressedColors = { Label = Color.Transparent; LabelWeight = RFontWeight.Normal; Background = primary.Main; Border = Color.Transparent; Icon = Color.Transparent; BadgeFontWeight = RulesRestricted.FontWeight.Bold; BadgeFontColor = Color.White; BadgeBackgroundColor = Color.DevRed; }
        ))
    ]

    Input.WeeklyCalendarStyles.Theme.All ({
        DayOfWeekText       = neutral.Main
        DateTextUnavailable = neutral.B200
        DateTextAvailable   = primary.Main
        DateTextSelected    = Color.White
        Circle              = secondary.Main
        InvalidReason       = caution.Main
    })

    Themes.Set<LC.ItemList.Theme> { SeeAll = { Height = 50; MarginVertical = 0 } }

    Input.NamedFileStyles.Theme.All (
        invalidColor        = Color.DevRed,
        dropZoneBorderColor = primary.B200
    )

module ApplyTheme =
    let primaryPrimary (scheme: ColorScheme) : unit =
        applyTheme scheme.Primary scheme.Primary scheme.Neutral scheme.Attention scheme.Caution

    let primarySecondary (scheme: ColorScheme) : unit =
        applyTheme scheme.Primary scheme.Secondary scheme.Neutral scheme.Attention scheme.Caution

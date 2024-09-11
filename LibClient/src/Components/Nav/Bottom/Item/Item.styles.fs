module LibClient.Components.Nav.Bottom.ItemStyles

open LibClient.Components.Nav.Bottom.Item
open LibClient.Responsive
open ReactXP.LegacyStyles

type State = Item.State

let private baseStyles = lazy (asBlocks [
    "item" => [
        AlignItems.Center
        JustifyContent.SpaceAround
        marginHorizontal 5
        borderRadius 4
    ]

    "spinner" => [
        Position.Absolute
    ]

    "item-content-container" => [
       Overflow.Visible
       AlignItems.Center
    ] && [
        LabelPosition.SideLabel.ToString() => [
           FlexDirection.Row
        ]

        LabelPosition.BottomLabel.ToString() => [
           FlexDirection.Column
        ]
    ]

    "label-content" => [
        Overflow.Visible
        FlexDirection.Row
        paddingRight 5    //padding space is essential to show full text while it is selected
        marginLeft 5
    ]

    "label-content-with-icon-badge" => [
        left -10
    ]

    "item-content-container-with-badge" => [
        left 5
    ]

    "label" => [
        trbl 0 0 0 0
    ]

    "badge" => [] && [
        ScreenSize.Handheld.Class => [
            minHeight         16
            minWidth          16
            borderRadius      8
            paddingHorizontal 4
        ]
    ]
])

type Colors = {
    Label:                Color
    LabelWeight:          RulesRestricted.FontWeight
    Background:           Color
    Border:               Color
    Icon:                 Color
    BadgeFontColor:       Color
    BadgeFontWeight:      RulesRestricted.FontWeight
    BadgeBackgroundColor: Color
}

let private allSameColors (color: Color) : Colors = {
    Label                = color
    LabelWeight          = RulesRestricted.FontWeight.Normal
    Background           = color
    Border               = color
    Icon                 = color
    BadgeFontColor       = color
    BadgeFontWeight      = RulesRestricted.FontWeight.Normal
    BadgeBackgroundColor = color
}

type Sizes = {
    IconSize:      int
    FontSize:      int
    Height:        int
    MaxWidth:      Option<int>
    BadgeFontSize: int
    BadgeTop:      int
    BadgeLeft:     int
}

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Nav.Bottom.Item", baseStyles)

    static member IconVerticalPositionAdjustment (value: int) : Styles =
        let blocks: List<ISheetBuildingBlock> =
            [
                "adjust-icon-vertical-position" => [
                    bottom value
                ]
            ]
        blocks |> makeSheet

    static member Size (screenSizeToSizes : ScreenSize -> Sizes ) : List<ISheetBuildingBlock> =
        let makeStyles (screenSize: ScreenSize) (sizes: Sizes) : List<ISheetBuildingBlock> =
            [
                "label && " + screenSize.Class => [
                    fontSize sizes.FontSize
                ]

                "icon && " + screenSize.Class => [
                    fontSize sizes.IconSize
                ]

                "item && " + screenSize.Class => [
                    height (sizes.Height - 2)
                ]
                sprintf "state-%s && %s" State.InProgress.Name screenSize.Class => [
                    height (sizes.Height - 2)
                ]
                "badge && " + screenSize.Class ==> LibClient.Components.BadgeStyles.Theme.FontSize(sizes.BadgeFontSize)
                "badge && " + screenSize.Class => [
                    top      sizes.BadgeTop
                    left     sizes.BadgeLeft
                ]
            ]

        makeStyles ScreenSize.Desktop  (screenSizeToSizes ScreenSize.Desktop) @
        makeStyles ScreenSize.Handheld (screenSizeToSizes ScreenSize.Handheld)

    static member Part (state: State, baseColors: Colors, hoveredColors: Colors, depressedColors: Colors) : List<ISheetBuildingBlock> =
        let withHoveredAndDepressed (baseSelector: string) (colorsToRules: Colors -> List<RuleFunctionReturnedStyleRules>) =
            baseSelector => (colorsToRules baseColors) && [
                "hovered"   => colorsToRules hoveredColors
                "depressed" => colorsToRules depressedColors
            ]

        [
            withHoveredAndDepressed (sprintf "item && state-%s" state.Name) (fun colors -> [
                borderColor     colors.Border
                backgroundColor colors.Background
            ])

            withHoveredAndDepressed (sprintf "label && state-%s" state.Name) (fun colors -> [
                RulesRestricted.fontWeight colors.LabelWeight
                color                      colors.Label
            ])

            withHoveredAndDepressed (sprintf "label-sentinel && state-%s" state.Name) (fun colors -> [
                color colors.Background
            ])

            withHoveredAndDepressed (sprintf "icon && state-%s" state.Name) (fun colors -> [
                color colors.Icon
            ])

            withHoveredAndDepressed (sprintf "icon && state-%s" state.Name) (fun colors -> [
                color colors.Icon
            ])

            "badge && state-"              + state.Name ==> LibClient.Components.BadgeStyles.Theme.Colors(baseColors.BadgeFontWeight, baseColors.BadgeFontColor, baseColors.BadgeBackgroundColor)
            "badge && hovered   && state-" + state.Name ==> LibClient.Components.BadgeStyles.Theme.Colors(hoveredColors.BadgeFontWeight, hoveredColors.BadgeFontColor, hoveredColors.BadgeBackgroundColor)
            "badge && depressed && state-" + state.Name ==> LibClient.Components.BadgeStyles.Theme.Colors(depressedColors.BadgeFontWeight, depressedColors.BadgeFontColor, depressedColors.BadgeBackgroundColor)
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Size(function
        | ScreenSize.Desktop  -> { IconSize = 24; FontSize = 16; Height = 72; MaxWidth = None; BadgeFontSize = 14; BadgeTop = 0; BadgeLeft = 0; }
        | ScreenSize.Handheld -> { IconSize = 24; FontSize = 16; Height = 72; MaxWidth = None; BadgeFontSize = 14; BadgeTop = 0; BadgeLeft = 0; }
    )
    Theme.Part(State.Actionable ignore,         allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
    Theme.Part(State.Disabled,                  allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
    Theme.Part(State.InProgress,                allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
    Theme.Part(State.Selected,                  allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
    Theme.Part(State.SelectedActionable ignore, allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
]))

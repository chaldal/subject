module LibClient.Components.Sidebar.ItemStyles

open ReactXP.LegacyStyles

type State = Item.State

let private defaultIconSize = 24
let private defaultBadgeFontSize = 16

// TODO deal with proper palette, either this one is really supposed to be custom,
// or I need to derive proper Neutral palette out of the colours used in the new mocks
let private baseStyles = lazy (asBlocks [
    "item" => [
        FlexDirection.Row
        flex              1
        height            42
        paddingHorizontal 18
    ]

    "left" => [
        centerContentVH
        flex        0
        width       32
        marginRight 10
    ]

    "middle" => [
        flex 1
        FlexDirection.Row
        AlignItems.Center
    ]

    "right" => [
        centerContentVH
        flex  0
    ]

    "badge" => [
        centerContentVH
        flex              0
        fontSize          16
        height            28
        minWidth          28
        paddingHorizontal 8
        borderRadius      14
    ]
])

type Colors = {
    Label:           Color
    LabelWeight:     RulesRestricted.FontWeight
    Background:      Color
    Border:          Color
    LeftIcon:        Color
    RightIcon:       Color
    BadgeBackground: Color
    BadgeText:       Color
}

let private allSameColors (color: Color) : Colors = {
    Label           = color
    LabelWeight     = RulesRestricted.FontWeight.Normal
    Background      = color
    Border          = color
    LeftIcon        = color
    RightIcon       = color
    BadgeBackground = color
    BadgeText       = color
}

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Sidebar.Item", baseStyles)

    static member itemHeight (itemHeight: int) : List<ISheetBuildingBlock> =
        [
            "item" => [
                height itemHeight
            ]
        ]

    static member Static (iconSize: int, theFontSize: int, ?theBadgeFontSize: int) : List<ISheetBuildingBlock> =
        let theBadgeFontSize = defaultArg theBadgeFontSize defaultBadgeFontSize
        [
            "label" => [
                fontSize theFontSize
            ]

            "icon-left" => [
                fontSize iconSize
            ]

            "icon-right" => [
                fontSize iconSize
            ]

            "badge" => [
                fontSize theBadgeFontSize
            ]
        ]

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
                color                      colors.Label
                RulesRestricted.fontWeight colors.LabelWeight
            ])

            withHoveredAndDepressed (sprintf "icon-left && state-%s" state.Name) (fun colors -> [
                color colors.LeftIcon
            ])

            withHoveredAndDepressed (sprintf "icon-right && state-%s" state.Name) (fun colors -> [
                color colors.LeftIcon
            ])

            withHoveredAndDepressed (sprintf "badge && state-%s" state.Name) (fun colors -> [
                backgroundColor colors.BadgeBackground
            ])

            withHoveredAndDepressed (sprintf "badge-text && state-%s" state.Name) (fun colors -> [
                color           colors.BadgeText
                backgroundColor colors.BadgeBackground
            ])
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Static(iconSize = 24, theFontSize = 16)
    Theme.Part(State.Actionable ignore, allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
    Theme.Part(State.Disabled,          allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
    Theme.Part(State.InProgress,        allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
    Theme.Part(State.Selected,          allSameColors Color.DevRed, allSameColors Color.DevGreen, allSameColors Color.DevOrange)
]))

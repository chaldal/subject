module LibClient.Components.IconWithBadgeStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "icon-with-count" => [
        FlexDirection.Row
        AlignItems.Center
    ]

    "badge" => [
        marginLeft 4
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.IconWithBadge", baseStyles)

    static member All (theIconColor: Color, theIconSize: int) : unit =
        customize [Theme.Rules(theIconColor, theIconSize)]

    static member One (theIconColor: Color, theIconSize: int) : Styles =
        Theme.Rules(theIconColor, theIconSize) |> makeSheet

    static member Rules (theIconColor: Color, theIconSize: int) : List<ISheetBuildingBlock> =
        [
            "icon" => [
                fontSize theIconSize
                color    theIconColor
            ]
        ]
        
    static member BadgeRules (theBadgeFontColor: Color, theBadgeFontSize: int, theBadgeFontWeight: RulesRestricted.FontWeight, theBadgeBackgroundColor: Color, theMarginLeft: int) : List<ISheetBuildingBlock> =
        [
            "badge" => [
                marginLeft theMarginLeft
            ]
            "badge" ==> BadgeStyles.Theme.One(
                theFontSize = theBadgeFontSize,
                theFontWeight = theBadgeFontWeight,
                theFontColor = theBadgeFontColor,
                theBackgroundColor = theBadgeBackgroundColor
            )
        ]
        
    static member Part (theIconColor: Color, theIconSize: int, theBadgeFontColor: Color, theBadgeFontSize: int, theBadgeFontWeight: RulesRestricted.FontWeight, theBadgeBackgroundColor: Color, theBadgeMarginLeft: int) : Styles =
        Theme.BadgeRules(theBadgeFontColor, theBadgeFontSize, theBadgeFontWeight, theBadgeBackgroundColor, theBadgeMarginLeft)
        |> List.append (Theme.Rules(theIconColor, theIconSize))
        |> makeSheet

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (theIconColor = Color.DevRed, theIconSize = 16)
]))

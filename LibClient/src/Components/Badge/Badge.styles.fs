module LibClient.Components.BadgeStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        flex              0
        minHeight         22
        minWidth          22
        paddingHorizontal 6
        borderRadius      11
        JustifyContent.Center
    ]

    "no-content" => [
        minHeight    10
        minWidth     10
        borderRadius 5
        JustifyContent.Center
    ]

    "text" => [
        TextAlign.Center
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Badge", baseStyles)

    static member All (theFontSize: int, theFontWeight: RulesRestricted.FontWeight, theFontColor: Color, theBackgroundColor: Color) : unit =
        customize [
            Theme.Rules(theFontSize, theFontWeight, theFontColor, theBackgroundColor)
        ]

    static member One (theFontSize: int, theFontWeight: RulesRestricted.FontWeight, theFontColor: Color, theBackgroundColor: Color) : Styles =
        Theme.Rules(theFontSize, theFontWeight, theFontColor, theBackgroundColor) |> makeSheet

    static member One (theFontSize: int, theFontColor: Color) : Styles =
        Theme.Rules(theFontSize, theFontColor) |> makeSheet
    
    static member FontSize (theFontSize: int) : Styles =
        let blocks : List<ISheetBuildingBlock> = [
            "text" => [
                fontSize theFontSize
            ]
        ]
        makeSheet blocks

    static member Colors (theFontWeight: RulesRestricted.FontWeight, theFontColor: Color, theBackgroundColor: Color) : Styles =
        let blocks : List<ISheetBuildingBlock> = [
            "view" => [
                backgroundColor theBackgroundColor
            ]

            "no-content" => [
                backgroundColor theBackgroundColor
            ]

            "text" => [
                RulesRestricted.fontWeight theFontWeight
                color                      theFontColor
            ]
        ]
        makeSheet blocks

    static member Rules (theFontSize: int, theFontWeight: RulesRestricted.FontWeight, theFontColor: Color, theBackgroundColor: Color) : List<ISheetBuildingBlock> = [
        "view" => [
            backgroundColor theBackgroundColor
        ]

        "no-content" => [
            backgroundColor theBackgroundColor
        ]

        "text" => [
            fontSize                   theFontSize
            RulesRestricted.fontWeight theFontWeight
            color                      theFontColor
        ]
    ]
    
    static member Rules (theFontSize: int, theFontColor: Color) : List<ISheetBuildingBlock> = [
            "view" => [
            ]
    
            "no-content" => [
                
            ]
    
            "text" => [
                fontSize                   theFontSize
                color                      theFontColor
            ]
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (14, RulesRestricted.FontWeight.Bold, Color.White, Color.DevRed)
]))


module LibClient.Components.Legacy.TopNav.BaseStyles

open ReactXP.LegacyStyles
open LibClient.ColorModule

let private baseStyles = lazy (asBlocks [
    "view" => [
        FlexDirection.Row
        JustifyContent.SpaceBetween
        AlignContent.Center
        AlignItems.Center
        Overflow.Visible
        height       44
        shadow       (Color.BlackAlpha 0.2) 3 (0, 2)
        borderBottom 1 (Color.Grey "dc")
    ]

    "left" => [
        flex 0
        Overflow.Visible
        padding 8
    ]

    "center" => [
        flex 1
        FlexDirection.Row
        JustifyContent.Center
        Overflow.Visible
    ]

    "right" => [
        flex 0
        Overflow.Visible
        padding 8
    ]

    "heading" => [
        fontSize 20
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Legacy.TopNav.Base", baseStyles)

    static member All (theBackgroundColor: Color, theTextColor: Color) : unit =
        customize [Theme.Rules(theBackgroundColor, theTextColor)]

    static member One (theBackgroundColor: Color, theTextColor: Color) : Styles =
        Theme.Rules(theBackgroundColor, theTextColor) |> makeSheet

    static member Height (theHeight: int) : Styles =
        makeSheet [
            "view" => [
                height theHeight
            ]
        ]

    static member Rules (theBackgroundColor: Color, theTextColor: Color) : List<ISheetBuildingBlock> =
        [
            "view" => [
                backgroundColor theBackgroundColor
            ]

            "heading" => [
                color theTextColor
            ]
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (theBackgroundColor = Color.Hex "#ffae00", theTextColor = Color.White)
]))

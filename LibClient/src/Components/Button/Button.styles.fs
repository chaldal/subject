module LibClient.Components.ButtonStyles

open LibClient.Input
open LibClient.Responsive
open ReactXP.LegacyStyles

type Level = Button.Level
type State = ButtonLowLevelState

[<RequireQualifiedAccess>]
type Sizes = {
    IconSize: int
    FontSize: int
    Height:   int
}

let mutable private defaultIconSize = 20
let mutable private defaultFontSize = 20

let private baseStyles = lazy (asBlocks [
    "view" => [
        FlexDirection.Column
        JustifyContent.Center
        AlignItems.Center
        borderWidth  1
        borderRadius 4

        // margin doesn't actually belong on a component,
        // its a concern of the outside, we're putting it
        // here during the bootstrapping of the components lib
        margin 4
    ]
    && [
        // NOTE this is pretty hacky, but will do for now
        "non-tertiary" => [
            paddingHV 12 4
            shadow    (Color.BlackAlpha 0.2) 5 (0, 2)
        ] && [
            "is-hovered" => [
                shadow (Color.BlackAlpha 0.2) 5 (0, 3)
                top    -1
            ]

            "is-depressed" => [
                shadow (Color.BlackAlpha 0.2) 3 (0, 0)
                top    1
            ]
        ]

        (sprintf "state-%O" Disabled) => [
            opacity 0.5
        ]

        (sprintf "state-%s" (Actionable ignore).GetName) => [
            Cursor.Pointer
        ]

        ScreenSize.Desktop.Class => [
            height        46
            paddingBottom 5 // vertical alignment hack
        ]

        ScreenSize.Handheld.Class => [
            height   38
            fontSize 14
        ]
    ]

    "label-block" => [
        AlignItems.Center
        FlexDirection.Row
        JustifyContent.Center
    ]

    "spinner-block" => [
        trbl            0 0 0 0
        backgroundColor (Color.WhiteAlpha 0.5)

        Position.Absolute
        AlignItems.Center
        JustifyContent.Center
    ]

    "label-text" => [
        TextAlign.Center
        flex 1
    ]
    && [
        ScreenSize.Desktop.Class => [
            fontSize defaultFontSize
        ]

        ScreenSize.Handheld.Class => [
            fontSize 14
        ]
    ]

    "left-icon" => [
        marginRight 5
    ]

    "right-icon" => [
        marginLeft 5
    ]

    "badge" => [
        marginLeft 5
    ]
])

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Button", baseStyles)

    static member Part (level: Level, state: ButtonLowLevelState, textColor: Color, theBorderColor: Color, theBackgroundColor: Color, ?maybeIconSize: int, ?maybeFontSize: int, ?maybeFontWeight: RulesRestricted.FontWeight) : List<ISheetBuildingBlock> =
        let iconSize      = defaultArg maybeIconSize   defaultIconSize
        let theFontWeight = defaultArg maybeFontWeight RulesRestricted.FontWeight.Normal

        [
            (sprintf "view && state-%s && level-%O" state.GetName level) => [
                borderColor     theBorderColor
                backgroundColor theBackgroundColor
            ]

            (sprintf "label-text && state-%s && level-%O" state.GetName level) => [
                color    textColor
                RulesRestricted.fontWeight theFontWeight

                match maybeFontSize with
                | None -> ()
                | Some theFontSize -> fontSize theFontSize
            ]

            (sprintf "icon && state-%s && level-%O" state.GetName level) => [
                color    textColor
                fontSize iconSize
            ]

            "badge" => [
                marginLeft 5
            ]
        ]

    static member LabelTextStyles (labelTextStyles: seq<RuleFunctionReturnedStyleRules>) =
        let textStyles: List<List<ISheetBuildingBlock>> =
            [[
                "label-text" => labelTextStyles
            ]]

        List.concat (baseStyles.Value :: textStyles) |> makeSheet

    static member BorderRadius (value: int) : List<ISheetBuildingBlock> =
        [
            "view" => [
                borderRadius value
            ]
        ]

    static member Depressed ( rules: seq<RuleFunctionReturnedStyleRules> ) : List<ISheetBuildingBlock> =
        [
            "is-depressed" => rules
        ]

    static member Level (level: Level, rules: seq<RuleFunctionReturnedStyleRules>) : List<ISheetBuildingBlock> =
        [
            (sprintf "view && level-%O" level) => rules
        ]

    static member One (level: Level, state: ButtonLowLevelState, theTextColor: Color, theBorderColor: Color, theBackgroundColor: Color, ?maybeIconSize: int, ?maybeFontSize: int) : Styles =
        let iconSize = defaultArg maybeIconSize defaultIconSize
        let fontSize = defaultArg maybeFontSize defaultFontSize

        Theme.Part (level, state, theTextColor, theBorderColor, theBackgroundColor, iconSize, fontSize) |> makeSheet

    static member IconSize (iconSize: int) : Styles =
        let blocks: List<ISheetBuildingBlock> =
            [
                "icon" => [
                    fontSize iconSize
                ]
            ]
        blocks |> makeSheet

    static member Badge (theFontSize: int, theFontWeight: RulesRestricted.FontWeight, textColor: Color, theBackgroundColor: Color, thePaddingHV: int * int) : List<ISheetBuildingBlock> =
        let thePaddingH, thePaddingV = thePaddingHV
        [
            "badge" => [
                paddingHV thePaddingH thePaddingV
            ]
            "badge" ==> LibClient.Components.BadgeStyles.Theme.One (theFontSize, theFontWeight, textColor, theBackgroundColor)
        ]

    static member BadgeColor (badgeColor: Color, ?textColor: Color) : Styles =
        let textColor = defaultArg textColor Color.White

        let blocks: List<ISheetBuildingBlock> =
            [
                "badge" ==> LibClient.Components.BadgeStyles.Theme.One (14, RulesRestricted.FontWeight.Bold, textColor, badgeColor)
            ]
        blocks |> makeSheet

    static member Size (screenSizeToSizes : ScreenSize -> Sizes ) : List<ISheetBuildingBlock> =
        let makeStyles (screenSize: ScreenSize) (sizes: Sizes) : List<ISheetBuildingBlock> =
            defaultFontSize <- sizes.FontSize
            defaultIconSize <- sizes.IconSize
            [
                "view && " + screenSize.Class => [
                    height sizes.Height
                ]

                "label-text && " + screenSize.Class => [
                    fontSize sizes.FontSize
                ]

                "icon" => [
                    fontSize sizes.IconSize
                ]
            ]

        makeStyles ScreenSize.Desktop  (screenSizeToSizes ScreenSize.Desktop) @
        makeStyles ScreenSize.Handheld (screenSizeToSizes ScreenSize.Handheld)

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.BorderRadius 4
    //                                              textColor      borderColor        backgroundColor
    Theme.Part(Level.Primary,    Actionable ignore, Color.White,   Color.DevGreen,    Color.DevGreen)
    Theme.Part(Level.Primary,    Disabled,          Color.White,   Color.DevGrey,     Color.DevGrey)
    Theme.Part(Level.Primary,    InProgress,        Color.White,   Color.DevGreen,    Color.DevGreen)

    Theme.Part(Level.Cautionary, Actionable ignore, Color.White,   Color.DevOrange,   Color.DevOrange)
    Theme.Part(Level.Cautionary, Disabled,          Color.White,   Color.DevGrey,     Color.DevGrey)
    Theme.Part(Level.Cautionary, InProgress,        Color.White,   Color.DevOrange,   Color.DevOrange)

    Theme.Part(Level.Secondary,  Actionable ignore, Color.Black,   Color.White,       Color.White)
    Theme.Part(Level.Secondary,  Disabled,          Color.DevGrey, Color.White,       Color.White)
    Theme.Part(Level.Secondary,  InProgress,        Color.Black,   Color.White,       Color.White)

    Theme.Part(Level.Tertiary,   Actionable ignore, Color.Black,   Color.Transparent, Color.Transparent)
    Theme.Part(Level.Tertiary,   Disabled,          Color.DevGrey, Color.Transparent, Color.Transparent)
    Theme.Part(Level.Tertiary,   InProgress,        Color.Black,   Color.Transparent, Color.Transparent)
]))
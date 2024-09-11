module LibClient.Components.HeadingStyles

open LibClient.Responsive
open ReactXP.LegacyStyles

type Level = LibClient.Components.Heading.Level

let private baseStyles = lazy (asBlocks [
    "text && " + ScreenSize.Handheld.Class => [
        FontWeight.W700
    ]
])

type Sizes = {|
    FontSize: int
|}

type (* class to enable named parameters *) Theme() =
    static let Customize = makeCustomize("LibClient.Components.Heading", baseStyles)

    static member All (screenSizeToSizes : ScreenSize -> Level -> Sizes, theColor: Color) : unit =
        Customize [
            Theme.Rules (screenSizeToSizes, theColor)
        ]

    static member One (screenSizeToSizes : ScreenSize -> Level -> Sizes, theColor: Color) : Styles =
        Theme.Rules (screenSizeToSizes, theColor) |> makeSheet

    static member Rules (screenSizeToSizes: ScreenSize -> Level -> Sizes, theColor: Color) : List<ISheetBuildingBlock> =
        let makeStyles (screenSize: ScreenSize) (level: Level) (sizes: Sizes) : List<ISheetBuildingBlock> =
            [
                "text && level-" + (level.ToString()) + " && " + screenSize.Class => [
                    fontSize sizes.FontSize
                ]
            ]

        [
            "text" => [
                color theColor
            ]
        ] @
        makeStyles ScreenSize.Desktop  Level.Primary   (screenSizeToSizes ScreenSize.Desktop  Level.Primary) @
        makeStyles ScreenSize.Desktop  Level.Secondary (screenSizeToSizes ScreenSize.Desktop  Level.Secondary) @
        makeStyles ScreenSize.Desktop  Level.Tertiary  (screenSizeToSizes ScreenSize.Desktop  Level.Tertiary) @
        makeStyles ScreenSize.Handheld Level.Primary   (screenSizeToSizes ScreenSize.Handheld Level.Primary) @
        makeStyles ScreenSize.Handheld Level.Secondary (screenSizeToSizes ScreenSize.Handheld Level.Secondary) @
        makeStyles ScreenSize.Handheld Level.Tertiary  (screenSizeToSizes ScreenSize.Handheld Level.Tertiary)

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        screenSizeToSizes = (function
            | ScreenSize.Desktop -> function
                | Level.Primary   -> {| FontSize = 36 |}
                | Level.Secondary -> {| FontSize = 24 |}
                | Level.Tertiary  -> {| FontSize = 14 |}
            | ScreenSize.Handheld -> function
                | Level.Primary   -> {| FontSize = 18 |}
                | Level.Secondary -> {| FontSize = 16 |}
                | Level.Tertiary  -> {| FontSize = 14 |}
        ),
        theColor = Color.Grey "45"
    )
]))

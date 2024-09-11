module LibClient.Components.Nav.Top.BaseStyles

open ReactXP.LegacyStyles
open LibClient.Responsive

let private baseStyles = lazy (asBlocks [
    "view" => [
        FlexDirection.Row
        AlignContent.Center
        AlignItems.Center
        Overflow.Visible

        shadow (Color.BlackAlpha 0.2) 3 (0, 2)
        border 1 (Color.Grey "cc")
    ] && [
        ScreenSize.Desktop.Class => [
            paddingHorizontal 16
        ]
    ]
])

type Sizes = {
    Height: int
}

type (* class to enable named parameters *) Theme() =
    static let Customize = makeCustomize("LibClient.Components.Nav.Top.Base", baseStyles)

    static member All (screenSizeToSizes: ScreenSize -> Sizes, theBackgroundColor: Color) : unit =
        Customize [
            Theme.Rules (screenSizeToSizes, theBackgroundColor)
        ]

    static member One (screenSizeToSizes: ScreenSize -> Sizes, theBackgroundColor: Color) : Styles =
            Theme.Rules (screenSizeToSizes, theBackgroundColor) |> makeSheet

    static member Rules (screenSizeToSizes: ScreenSize -> Sizes, theBackgroundColor: Color) : List<ISheetBuildingBlock> =
        let makeStyles (screenSize: ScreenSize) (sizes: Sizes) : List<ISheetBuildingBlock> =
            [
                "view && " +  screenSize.Class => [
                    height sizes.Height
                ]
            ]

        [
            "view" => [
                backgroundColor theBackgroundColor
            ]
        ] @
        makeStyles ScreenSize.Desktop  (screenSizeToSizes ScreenSize.Desktop) @
        makeStyles ScreenSize.Handheld (screenSizeToSizes ScreenSize.Handheld)

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        screenSizeToSizes = (function
            | ScreenSize.Desktop  -> { Height = 72; }
            | ScreenSize.Handheld -> { Height = 72; }
        ),
        theBackgroundColor = Color.DevBlue
    )
]))

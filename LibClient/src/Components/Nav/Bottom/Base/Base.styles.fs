module LibClient.Components.Nav.Bottom.BaseStyles

open ReactXP.LegacyStyles
open LibClient.Responsive

let private baseStyles = lazy (asBlocks [
    "view" => [
        FlexDirection.Row
        AlignContent.Center
        AlignItems.Center
        Overflow.Visible
    ]
])

type Sizes = {
    Height: int
}

type (* class to enable named parameters *) Theme() =
    static let Customize = makeCustomize("LibClient.Components.Nav.Bottom.Base", baseStyles)

    static member All (screenSizeToSizes: ScreenSize -> Sizes, theBackgroundColor: Color, ?hideShadow: bool) : unit =
        Customize [
            Theme.Rules (screenSizeToSizes, theBackgroundColor, ?hideShadow = hideShadow)
        ]

    static member One (screenSizeToSizes: ScreenSize -> Sizes, theBackgroundColor: Color, ?hideShadow: bool) : Styles =
        Theme.Rules (screenSizeToSizes, theBackgroundColor, ?hideShadow = hideShadow) |> makeSheet

    static member Rules (screenSizeToSizes: ScreenSize -> Sizes, theBackgroundColor: Color, ?hideShadow: bool) : List<ISheetBuildingBlock> =
        let hideShadow = defaultArg hideShadow false
        let makeStyles (screenSize: ScreenSize) (sizes: Sizes) : List<ISheetBuildingBlock> =
            [
                "view && " +  screenSize.Class => [
                    height sizes.Height
                ]
            ]

        [
            "view" => [
                backgroundColor theBackgroundColor

                if not hideShadow then
                    shadow (Color.BlackAlpha 0.2) 3 (0, -2)
                    borderTop 1 (Color.Grey "cc")
            ]
        ] @
        makeStyles ScreenSize.Desktop  (screenSizeToSizes ScreenSize.Desktop) @
        makeStyles ScreenSize.Handheld (screenSizeToSizes ScreenSize.Handheld)

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        screenSizeToSizes = (function
            | ScreenSize.Desktop  -> { Height = 72; }
            | ScreenSize.Handheld -> { Height = 48; }
        ),
        theBackgroundColor = Color.DevBlue
    )
]))

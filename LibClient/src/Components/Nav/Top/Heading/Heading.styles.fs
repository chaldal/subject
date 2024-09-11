module LibClient.Components.Nav.Top.HeadingStyles

open LibClient.Responsive
open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
       flex 1
       FontWeight.Normal
    ] && [
        ScreenSize.Handheld.Class => [
            AlignSelf.Center
            AlignItems.Center
            TextAlign.Center
        ]
    ]
])

type Sizes = {|
    FontSize: int
|}

type (* class to enable named parameters *) Theme() =
    static let Customize = makeCustomize("LibClient.Components.Nav.Top.Heading", baseStyles)

    static member All (screenSizeToSizes : ScreenSize -> Sizes, theColor: Color) : unit =
        Customize [
            Theme.Rules (screenSizeToSizes, theColor)
        ]

    static member One (screenSizeToSizes : ScreenSize -> Sizes, theColor: Color) : Styles =
        Theme.Rules (screenSizeToSizes, theColor) |> makeSheet

    static member Rules (screenSizeToSizes: ScreenSize -> Sizes, theColor: Color) : List<ISheetBuildingBlock> =
        [
            "view" ==> LibClient.Components.HeadingStyles.Theme.One (
                screenSizeToSizes = (function
                    | ScreenSize.Desktop  -> fun _ -> screenSizeToSizes ScreenSize.Desktop
                    | ScreenSize.Handheld -> fun _ -> screenSizeToSizes ScreenSize.Handheld
                ),
                theColor = theColor
            )
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        screenSizeToSizes = (function
            | ScreenSize.Desktop  -> {| FontSize = 24; |}
            | ScreenSize.Handheld -> {| FontSize = 24; |}
        ),
        theColor = Color.White
    )
]))

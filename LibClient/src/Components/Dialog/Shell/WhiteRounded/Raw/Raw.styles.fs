module LibClient.Components.Dialog.Shell.WhiteRounded.RawStyles

open LibClient.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let closeButton =
        makeViewStyles {
            backgroundColor Color.White
            borderRadius    100
            width           42
            height          42
            Position.Absolute
            top   2
            right 2
        }

    let closeButtonTheme (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = Color.Grey "50"
                    IconSize = 22
                }
        }

open LibClient.Components.Dialog.Shell.WhiteRounded.Raw
open ReactXP.LegacyStyles
open LibClient.Responsive

let private baseStyles = lazy (asBlocks [
    "max-size-limiter" => [
        AlignItems.Center
        AlignContent.Center
        AlignSelf.Stretch
        flex    1
        padding 20
    ] && [
        (sprintf "position-%O" DialogPosition.Top)    => [ JustifyContent.FlexStart ]
        (sprintf "position-%O" DialogPosition.Center) => [ JustifyContent.Center    ]
        (sprintf "position-%O" DialogPosition.Bottom) => [ JustifyContent.FlexEnd   ]
    ]

    "white-rounded-base" => [
        backgroundColor Color.White
        padding         20
        flex            -1
    ] && [
        ScreenSize.Desktop.Class => [
            borderRadius 10
        ]

        ScreenSize.Handheld.Class => [
            borderRadius 4
            AlignSelf.Stretch
        ]
    ]

    "content" => [
        Overflow.Hidden
        flex              1
    ]

    "in-progress" => [
        Position.Absolute
        JustifyContent.Center
        AlignItems.Center
        trbl            0 0 0 0
        backgroundColor (Color.WhiteAlpha 0.7)
    ]

])

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Dialog.Shell.WhiteRounded.Raw", baseStyles)

    static member Width (value: int) : Styles = Theme.WidthRules value |> makeSheet

    static member WidthRules (value: int) : List<ISheetBuildingBlock> = [
        "white-rounded-base" => [
            width value
            AlignSelf.Auto
        ]
    ]
    
    static member Padding (maxSizeLimiterValue: int) (whiteRoundedBaseValue: int) : Styles =
        Theme.PaddingRules maxSizeLimiterValue whiteRoundedBaseValue
        |> makeSheet
        
    static member PaddingRules (maxSizeLimiterValue: int) (whiteRoundedBaseValue: int) : List<ISheetBuildingBlock> = [
            "max-size-limiter"   => [
                padding maxSizeLimiterValue
            ]
            "white-rounded-base" => [
                padding whiteRoundedBaseValue
            ]
        ]
    
    static member BoundaryRadius (topLeft: int) (topRight: int) (bottomLeft: int) (bottomRight: int) : Styles =
        Theme.BoundaryRadiusRules topLeft topRight bottomLeft bottomRight
        |> makeSheet
        
    static member BoundaryRadiusRules (topLeft: int) (topRight: int) (bottomLeft: int) (bottomRight: int) : List<ISheetBuildingBlock> = [
        "white-rounded-base" => [
            borderTopLeftRadius     topLeft
            borderTopRightRadius    topRight
            borderBottomLeftRadius  bottomLeft
            borderBottomRightRadius bottomRight
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
]))
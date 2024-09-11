module LibClient.Components.FloatingActionButtonStyles

open ReactXP.Styles

[<RequireQualifiedAccessAttribute>]
module Styles =
    let tapCapture =
        makeViewStyles {
            trbl -4 -12 -4 -12
        }

open ReactXP.LegacyStyles

type State = LibClient.Input.ButtonLowLevelState

let private baseStyles = lazy (asBlocks [
    "view" => [
        Overflow.VisibleForTapCapture
        FlexDirection.Row
        AlignItems.Center
        JustifyContent.SpaceAround

        AlignSelf.FlexStart // do not stretch

        shadow (Color.BlackAlpha 0.3) 5 (0, 1)
    ] && [
        (sprintf "state-%O" State.Disabled) => [
            opacity 0.5
        ]

        (sprintf "state-%s" (State.Actionable ignore).GetName) => [
            Cursor.Pointer
        ]

        "is-hovered" => [
            shadow (Color.BlackAlpha 0.3) 6 (0, 3)
        ]

        "is-depressed" => [
            shadow (Color.BlackAlpha 0.2) 3 (0, 0)
            opacity 0.5
        ]

    ]

    "label" => [
        marginLeft 8
    ]

    "label-text" => [
        fontSize 16
    ]

    "spinner-block" => [
        Position.Absolute
        trbl 0 0 0 0
        AlignItems.Center
        JustifyContent.Center
        backgroundColor (Color.WhiteAlpha 0.5)
    ]
])

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.FloatingActionButton", baseStyles)

    static member One (state: State, theBackgroundColor: Color, iconColor: Color, iconSize: int, ?size: int) : Styles =
        Theme.Part(state, theBackgroundColor, iconColor, iconSize, ?size = size) |> makeSheet

    static member Part (state: State, theBackgroundColor: Color, iconColor: Color, iconSize: int, ?size: int) : List<ISheetBuildingBlock> =
        let size = size |> Option.getOrElse 56
        [
            (sprintf "view && state-%s" state.GetName) => [
                backgroundColor theBackgroundColor
            ]

            (sprintf "icon && state-%s" state.GetName) => [
                color    iconColor
                fontSize iconSize
            ]

            (sprintf "label-text && state-%s" state.GetName) => [
                color iconColor
            ]

            "view" => [
                height   size
                minWidth size
                borderRadius (size / 2)
            ]

            "with-label" => [
                paddingHorizontal (size / 4)
            ]
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Part (State.Actionable ignore, theBackgroundColor = Color.DevRed, iconColor = Color.White, iconSize = 32)
    Theme.Part (State.Disabled,          theBackgroundColor = Color.DevRed, iconColor = Color.White, iconSize = 32)
    Theme.Part (State.InProgress,        theBackgroundColor = Color.DevRed, iconColor = Color.White, iconSize = 32)
]))

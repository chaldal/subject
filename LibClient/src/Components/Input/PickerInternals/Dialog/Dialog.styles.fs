module LibClient.Components.Input.PickerInternals.DialogStyles

open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let itemList =
        makeViewStyles {
            Overflow.VisibleForScrolling
        }

open ReactXP.LegacyStyles

module Button       = LibClient.Components.Button
module ButtonStyles = LibClient.Components.ButtonStyles

open LibClient.Input
open LibClient.Responsive

let styles = lazy (compile [
    "text-input" => [
        paddingHV    12 4
        marginBottom 10
        borderRadius 4
        border 1 (Color.Grey "cc")
    ]

    "item" => [
        paddingLeft     16
        paddingRight    8
        paddingVertical 12
        FlexDirection.Row
    ]

    "item-selectedness" => [
        width 24
        flex  0
    ]

    "item-selected-icon" => [
        fontSize 16
        color (Color.Grey "cc")
    ]

    ScreenSize.Desktop.Class => [
    ] && [
        "text-input" => [
            height        46
            fontSize      20
        ]
    ]

    ScreenSize.Handheld.Class => [
    ] && [
        "text-input" => [
            height   40
            fontSize 16
        ]
    ]

    "button-normal" ==> ButtonStyles.Theme.One (
         Button.Level.Primary,
         ButtonLowLevelState.Actionable ignore,
         theTextColor       = Color.Hex "#004eff",
         theBorderColor     = Color.White,
         theBackgroundColor = Color.White
    )

    "button-normal && selected" ==> ButtonStyles.Theme.One (
         Button.Level.Primary,
         ButtonLowLevelState.Actionable ignore,
         theTextColor       = Color.Grey "59",
         theBorderColor     = Color.White,
         theBackgroundColor = Color.White
    )

    "activity-indicator-block" => [
        FlexDirection.Row
        JustifyContent.Center
        AlignItems.Center
        paddingVertical 12
    ]

    "activity-indicator-overlay" => [
        Position.Absolute
        trbl 0 0 0 0
        FlexDirection.Row
        JustifyContent.Center
        AlignItems.Center
        backgroundColor (Color.WhiteAlpha 0.5)
    ]
])

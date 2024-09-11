module LibClient.Components.ContextMenu.DialogStyles

module Button       = LibClient.Components.Button
module ButtonStyles = LibClient.Components.ButtonStyles

open LibClient.Input
open ReactXP.LegacyStyles

let styles = lazy (compile [
    "dialog-contents" => [
        Position.Absolute
        trbl 0 0 0 0
        FlexDirection.Column
        JustifyContent.FlexEnd
        padding 12
    ]

    "scroll-view" => [
        flex -1
    ]

    "divider" => [
        height 20
    ]

    "heading" => [
        color Color.White
        TextAlign.Center
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

    "button-cautionary" ==> ButtonStyles.Theme.One (
         Button.Level.Cautionary,
         ButtonLowLevelState.Actionable ignore,
         theTextColor       = Color.DevRed,
         theBorderColor     = Color.White,
         theBackgroundColor = Color.White
    )
])

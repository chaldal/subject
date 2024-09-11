module AppEggShellGallery.Components.Content.IconsStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy (compile [
    "view" => [
        FlexDirection.Row
        FlexWrap.Wrap
        paddingVertical 10
    ]

    "icon-container" => [
        margin  5
        padding 5
        // HACK Increase this if the icon-label overflows
        width   130
        Overflow.Visible
        AlignItems.Center
    ]

    "icon" => [
        color    colors.Neutral.B600
        fontSize 45
    ]

    "icon-label" => [
        fontSize       10
        marginVertical 2
    ]
])

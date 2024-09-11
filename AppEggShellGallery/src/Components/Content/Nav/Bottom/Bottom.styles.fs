module AppEggShellGallery.Components.Content.Nav.BottomStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "image" => [
        height       32
        width        32
        borderRadius 16
        marginRight  10
    ]

    "heading" => [
        marginVertical 30
    ]

    "handheld" => [
        width 480
        AlignSelf.Center
    ]

    "code-and-notes" => [
        paddingVertical 30
        minWidth        300
    ]

    "adjust-icon" ==> LibClient.Components.Nav.Bottom.ItemStyles.Theme.IconVerticalPositionAdjustment 10
])
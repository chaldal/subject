module AppEggShellGallery.Components.ComponentContentHeadingStyles

open ReactXP.LegacyStyles
open LibClient.Responsive

let styles = lazy (compile [
    "view" => [
        FlexDirection.Row
        AlignItems.Center
    ]

    "text" => [
        color (Color.Grey "45") // TODO make this themeable
    ] && [
        ScreenSize.Desktop.Class => [
            fontSize 36
        ]

        ScreenSize.Handheld.Class => [
            fontSize 18
        ]
    ]
])

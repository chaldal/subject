module AppEggShellGallery.Components.SidebarStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "gallery-visuals" => [
        FlexDirection.Row
        AlignItems.Center
        padding 18
    ]

    "label" => [
        flex 1
    ]

    "buttons" => [
        FlexDirection.Row
        flex 0
    ]
])

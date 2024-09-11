module ThirdParty.Map.Components.Native.MapStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        flex   1
        trbl   0 0 0 0
        Position.Relative
        JustifyContent.Center
    ]

    "full-screen" => [
        trbl   0 0 0 0
        Position.Absolute
        JustifyContent.Center
    ]

    "image" => [
        height 40
        AlignItems.Center
        JustifyContent.Center
    ]
])
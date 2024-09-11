module AppEggShellGallery.Components.Content.ColorVariantsStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "color-variant-container" => [
        FlexDirection.Row
        marginVertical 20
        Overflow.VisibleForScrolling
    ]

    "variant" => [
        marginVertical 5
    ]
])

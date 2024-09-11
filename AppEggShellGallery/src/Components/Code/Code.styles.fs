module AppEggShellGallery.Components.CodeStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        // HACK this is a cheap hack, I don't have a nice way of
        // giving in-between margin to adjacent Code blocks...
        // Vertical and not just Bottom so as to keep blocks centred vertically
        marginVertical 20
    ]

    "heading" => [
        fontSize 14
    ]
])

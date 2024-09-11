module LibClient.Components.HandheldListItemStyles

open ReactXP.LegacyStyles

// TODO style disabled and other states
// TODO export a themeing function

let styles = lazy (compile [
    "view" => [
        FlexDirection.Row
        AlignItems.Center
        minHeight    42
        padding      8
        borderBottom 1 (Color.Grey "ee")
        borderTop    1 (Color.Grey "ee")
        marginTop    -1 // to collapse adjacent borders
    ]

    "left-icon" => [
        flex 0
    ]

    "label" => [
        FlexDirection.Row
        flex 1
    ]

    "right" => [
        FlexDirection.Row
        AlignItems.Center
        flex 0
    ]

    "right-icon" => [
        marginLeft 12
    ]

    "number" => [
        paddingHV       6 2
        borderRadius    20
        backgroundColor (Color.Grey "dd")
        color           Color.White
    ]
])

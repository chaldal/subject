module LibAutoUi.Components.InputFormElementStyles

open ReactXP.LegacyStyles

let private labelColor       = Color.Hex "#999999"
let private inputBorderColor = Color.Hex "#cccccc"

let styles = lazy (compile [
    "view" => [
        padding      4
        paddingRight 0
    ]

    "primitive" => [
        FlexDirection.Row
        AlignItems.Center
    ]

    "inline-label" => [
        flex        1
        flexBasis   0
        marginRight 10
        color       labelColor
    ]

    "inline-value" => [
        flex      3
        flexBasis 0
    ]

    "text-input" => [
        border 1 inputBorderColor
    ]

    "record" => [
        marginVertical 8
    ]

    "record-label" => [
        color labelColor
    ]

    "indented-fields" => [
        paddingLeft 24
    ]

    "union-case-selection" => [
        FlexDirection.Row
    ]

    "picker" => [
        border          1 inputBorderColor
        borderRadius    0
        backgroundColor Color.White
    ]

    "range-value-selection" => [
        FlexDirection.Row
    ]

    "option" => [
        FlexDirection.Row
    ]
])

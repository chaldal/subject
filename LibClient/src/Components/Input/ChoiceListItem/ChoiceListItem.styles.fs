module LibClient.Components.Input.ChoiceListItemStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        FlexDirection.Row
        AlignItems.Center
        flex    1
        padding 4
    ]

    "label" => [
        flex 1
    ] && [
        "icon-position-Right" => [
            paddingRight 12
        ]

        "icon-position-Left" => [
            paddingLeft 12
        ]
    ]

    "icon" => [
        fontSize 20
    ] && [
        "checked" => [
            color Color.DevGreen
        ]

        "unchecked" => [
            color (Color.Grey "aa")
        ]
    ]
])

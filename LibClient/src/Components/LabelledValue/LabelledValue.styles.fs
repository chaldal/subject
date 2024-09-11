module LibClient.Components.LabelledValueStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "labelled-value" => [
       FlexDirection.Row
       JustifyContent.SpaceBetween
       marginVertical 2
    ]

    "label" => [
        FontWeight.Bold
    ]

    "value" => [
        FlexDirection.Column
        TextAlign.Right
    ]
])

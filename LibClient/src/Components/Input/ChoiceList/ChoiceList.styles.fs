module LibClient.Components.Input.ChoiceListStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "invalid-reason" => [
        color   Color.DevRed
        padding 4
    ]
])

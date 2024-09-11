module LibClient.Components.ScrollViewStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "inner-div" => [
        Overflow.Visible
    ]
])

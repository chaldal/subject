module LibClient.Components.DraggableStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "gesture-view" => [
        Overflow.Visible
    ]

    "contents-view" => [
        Overflow.Visible
    ]
])

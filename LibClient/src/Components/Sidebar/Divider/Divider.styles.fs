module LibClient.Components.Sidebar.DividerStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "divider" => [
        marginVertical 20
        borderBottom 1 (Color.Grey "cc")
    ]
])

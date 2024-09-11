module LibClient.Components.AppShell.NetworkFailureMessageStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        margin 20
        AlignItems.Center
    ]

    "second-line" => [
        marginVertical 20
    ]
])

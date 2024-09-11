module LibClient.Components.Legacy.TopNav.ShowSidebarButtonStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        borderWidth 4
        borderColor Color.DevRed
    ]
])

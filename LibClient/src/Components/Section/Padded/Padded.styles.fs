module LibClient.Components.Section.PaddedStyles

open LibClient.Responsive
open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [] && [
        ScreenSize.Desktop.Class => [
            padding 24
        ]

        ScreenSize.Handheld.Class => [
            padding 16
        ]
    ]
])

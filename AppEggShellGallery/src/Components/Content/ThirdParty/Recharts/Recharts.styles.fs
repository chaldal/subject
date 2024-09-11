module AppEggShellGallery.Components.Content.ThirdParty.RechartsStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        borderWidth 4
        borderColor Color.DevRed
    ]
])

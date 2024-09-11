module LibClient.Components.Dialog.ConfirmStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "details" => [
        minWidth 300
    ]
    
    "details-text" => [
        color (Color.Grey "66")
    ]
])

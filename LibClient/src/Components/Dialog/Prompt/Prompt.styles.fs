module LibClient.Components.Dialog.PromptStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "dialog-contents" => [
        minWidth  200
        minHeight 200
        Overflow.Visible
    ]
])

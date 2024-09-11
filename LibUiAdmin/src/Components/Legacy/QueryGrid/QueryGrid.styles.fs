module LibUiAdmin.Components.Legacy.QueryGridStyles

open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let buttonsBlock =
        makeViewStyles {
            width 300
        }

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "query-block" => [
        width 300
    ]
])

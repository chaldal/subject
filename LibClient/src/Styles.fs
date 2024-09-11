module LibClient.Styles

open ReactXP.LegacyStyles

// NOTE this file is a scratchpad for developing a library
// of shared styles that will eventually become standard
// for all apps.

let styles = compile [
    "lc-heading" => [
        fontSize 32
        color    Color.Black
    ]
]

module AppEggShellGallery.Components.ComponentPropsStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy (compile [
    "view" => [
        marginBottom 16 // TODO this sucks, but we don't have a way to control just the space _between_ blocks
    ]

    "content && indented" => [
        marginLeft 16
    ]

    "error" => [
        marginVertical 10
    ]

    "error-text" => [
        color colors.Caution.Main
    ]

    "heading" => [
        marginTop    8
        marginBottom 8
    ]

    "meta-content" => [
        color (Color.Grey "cc")
    ]
    
    "props" => [
        borderBottom 1 (Color.Grey "cc")
        marginBottom 5
        paddingBottom 5
    ]
])

addCss (sprintf """
.aesg-ComponentProps-table {
    border-collapse: collapse;
    width:           100%%;
}

.aesg-ComponentProps-table th {
    padding:     0px 8px;
    text-align:  left;
    color:       #cccccc;
    font-weight: normal;
}

.aesg-ComponentProps-table tr:nth-child(even) {
    background-color: #fafafa;
}

.aesg-ComponentProps-table td {
    padding:     4px 8px;
    color:       #666;
    font-family: monospace;
}

.aesg-ComponentProps-table td.name {
    color: #000080;
}

.aesg-ComponentProps-table td.type {
    color: #990073;
}

.aesg-ComponentProps-table td.autowrapped {
    font-family: sans-serif;
    color: #cccccc;
    padding-left: 16px;
}

.aesg-ComponentProps-table td.value {
    color: #219161;
}

.aesg-ComponentProps-table td.description {
    color: #999999;
}
"""
)
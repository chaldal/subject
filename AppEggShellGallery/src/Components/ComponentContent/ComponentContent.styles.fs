module AppEggShellGallery.Components.ComponentContentStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        Overflow.Visible
    ]

    "heading-secondary" => [
        marginTop    40
        marginBottom 10
    ]

    "heading-secondary-text" => [
        color (Color.Grey "66")
    ]
])

addCss ("""

.aesg-ContentComponent-table {
    border-collapse: collapse;
    width:           100%;
}

.aesg-ContentComponent-table > tbody > tr {
    border-top: 1px solid #cccccc;
}

"""
)

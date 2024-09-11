module AppEggShellGallery.Components.ComponentSampleStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "visuals" => [
        paddingVertical 16
        minWidth        300
        Overflow.Visible
    ]

    "code-and-notes" => [
        marginLeft      40
        paddingVertical 30
        minWidth        300
    ]

    "code" => [
        JustifyContent.Center
        Overflow.Visible
    ]
])

addCss (sprintf """

.aesg-ContentComponent-table .cs-heading {
    border-top:    24px solid transparent;
    border-bottom: 2px  solid transparent;
}

.aesg-ContentComponent-table .vertical-align-top {
    vertical-align: top;
}

.aesg-ContentComponent-table .vertical-align-middle {
    vertical-align: middle;
}

.aesg-ContentComponent-table .layout-code-below-samples {
    display: block;
}

"""
)
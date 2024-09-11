module AppEggShellGallery.Components.Route.DocsStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "message" => [
        TextAlign.Center
        padding   10
    ]
])

addCss (sprintf """
.url-unsorted-directory-structure_md code {
    font-size:   12px; // works best for the tree visualization
    line-height: 14px;
}
""")
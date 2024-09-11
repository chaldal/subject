module ThirdParty.Map.Components.Web.MapStyles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "view" => [
        flex 1
    ]
])

addCss ("""
.map-anchor {
    position: absolute;
    top:      0;
    left:     0;
    bottom:   0;
    right:    0;
}
""")

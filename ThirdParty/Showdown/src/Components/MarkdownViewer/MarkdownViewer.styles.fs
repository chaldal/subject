module ThirdParty.Showdown.Components.MarkdownViewerStyles

open ReactXP.LegacyStyles

let styles = lazy RuntimeStyles.None

addCss (sprintf """

.markdown-global-link {
    color:  #006ebdfc;
    cursor: pointer;
}

"""
)
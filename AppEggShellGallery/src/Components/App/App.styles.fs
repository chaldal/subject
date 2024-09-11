module AppEggShellGallery.Components.AppStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Colors

let styles = lazy RuntimeStyles.None

addCss (sprintf """

.markdown-tag {
    background-color: %s;
    color:            %s;
    border-radius:    20px;
    padding:          4px 10px;
    margin:           0px 10px;
    font-size:        12px;
}

"""
colors.Secondary.B200.ToCssString
colors.Secondary.Main.ToCssString
)
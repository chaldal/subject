module AppEggShellGallery.Components.ComponentSampleGroupStyles

open ReactXP.LegacyStyles

let styles = lazy RuntimeStyles.None

(*
    - specifying .aesg-ContentComponent-table because it is overriding
    .aesg-ContentComponent-table tr style.

    - a thick transparent border mimics margin/padding
*)

addCss (sprintf """

.aesg-ContentComponent-table .csg-heading {
    border-top:    24px solid transparent;
    border-bottom: 2px  solid transparent;
}

.aesg-ContentComponent-table .csg-notes {
    border-top:    24px solid transparent;
    border-bottom: 24px solid transparent;
}

.aesg-ContentComponent-table .csg-vertical-padding {
    height:        36px;
    border-bottom: 24px solid transparent;
    border-top:    24px solid transparent;
}

"""
)
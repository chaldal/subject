module LibUiSubjectAdmin.Components.Audit.GenericStyles

open ReactXP.LegacyStyles

let styles = lazy RuntimeStyles.None

addCss """
td.audit-operation-str div div {
    white-space: pre !important;
}
"""

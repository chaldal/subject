// AUTO-GENERATED DO NOT EDIT
module LibUiSubjectAdmin.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "LibUiSubjectAdmin.Components.Audit.Generic" LibUiSubjectAdmin.Components.Audit.GenericRender.render
    LibClient.ComponentRegistry.RegisterRender "LibUiSubjectAdmin.Components.Subject" LibUiSubjectAdmin.Components.SubjectRender.render

    LibClient.ComponentRegistry.RegisterStyles ("LibUiSubjectAdmin.Components.Audit.Generic", LibUiSubjectAdmin.Components.Audit.GenericStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("LibUiSubjectAdmin.Components.Subject", LibUiSubjectAdmin.Components.SubjectStyles.styles)

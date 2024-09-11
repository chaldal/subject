// AUTO-GENERATED DO NOT EDIT
module LibLifeCycleUi.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "LibLifeCycleUi.Components.IndexQuery" LibLifeCycleUi.Components.IndexQueryRender.render

    LibClient.ComponentRegistry.RegisterStyles ("LibLifeCycleUi.Components.IndexQuery", LibLifeCycleUi.Components.IndexQueryStyles.styles)

// AUTO-GENERATED DO NOT EDIT
module LibUiAdmin.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "LibUiAdmin.Components.Grid" LibUiAdmin.Components.GridRender.render
    LibClient.ComponentRegistry.RegisterRender "LibUiAdmin.Components.Legacy.QueryGrid" LibUiAdmin.Components.Legacy.QueryGridRender.render
    LibClient.ComponentRegistry.RegisterRender "LibUiAdmin.Components.QueryGrid" LibUiAdmin.Components.QueryGridRender.render

    LibClient.ComponentRegistry.RegisterStyles ("LibUiAdmin.Components.Grid", LibUiAdmin.Components.GridStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("LibUiAdmin.Components.Legacy.QueryGrid", LibUiAdmin.Components.Legacy.QueryGridStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("LibUiAdmin.Components.QueryGrid", LibUiAdmin.Components.QueryGridStyles.styles)

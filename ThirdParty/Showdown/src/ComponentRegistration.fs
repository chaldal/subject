// AUTO-GENERATED DO NOT EDIT
module ThirdParty.Showdown.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.Showdown.Components.MarkdownViewer" ThirdParty.Showdown.Components.MarkdownViewerRender.render

    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.Showdown.Components.MarkdownViewer", ThirdParty.Showdown.Components.MarkdownViewerStyles.styles)

// AUTO-GENERATED DO NOT EDIT
module LibRouter.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "LibRouter.Components.Dialogs" LibRouter.Components.DialogsRender.render
    LibClient.ComponentRegistry.RegisterRender "LibRouter.Components.LogRouteTransitions" LibRouter.Components.LogRouteTransitionsRender.render
    LibClient.ComponentRegistry.RegisterRender "LibRouter.Components.NativeBackButton" LibRouter.Components.NativeBackButtonRender.render
    LibClient.ComponentRegistry.RegisterRender "LibRouter.Components.With.Location" LibRouter.Components.With.LocationRender.render
    LibClient.ComponentRegistry.RegisterRender "LibRouter.Components.With.Route" LibRouter.Components.With.RouteRender.render

    LibClient.ComponentRegistry.RegisterStyles ("LibRouter.Components.Dialogs", LibRouter.Components.DialogsStyles.styles)

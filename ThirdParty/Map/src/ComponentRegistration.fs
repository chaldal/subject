// AUTO-GENERATED DO NOT EDIT
module ThirdParty.Map.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.Map.Components.Base" ThirdParty.Map.Components.BaseRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.Map.Components.Native.LatLngFromAddress" ThirdParty.Map.Components.Native.LatLngFromAddressRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.Map.Components.Native.Map" ThirdParty.Map.Components.Native.MapRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.Map.Components.Web.LatLngFromAddress" ThirdParty.Map.Components.Web.LatLngFromAddressRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.Map.Components.Web.Map" ThirdParty.Map.Components.Web.MapRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.Map.Components.With.LatLng" ThirdParty.Map.Components.With.LatLngRender.render

    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.Map.Components.Base", ThirdParty.Map.Components.BaseStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.Map.Components.Native.LatLngFromAddress", ThirdParty.Map.Components.Native.LatLngFromAddressStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.Map.Components.Native.Map", ThirdParty.Map.Components.Native.MapStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.Map.Components.Web.LatLngFromAddress", ThirdParty.Map.Components.Web.LatLngFromAddressStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.Map.Components.Web.Map", ThirdParty.Map.Components.Web.MapStyles.styles)

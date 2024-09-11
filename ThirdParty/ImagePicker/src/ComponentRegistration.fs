// AUTO-GENERATED DO NOT EDIT
module ThirdParty.ImagePicker.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.ImagePicker.Components.Base" ThirdParty.ImagePicker.Components.BaseRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.ImagePicker.Components.Native.ImagePicker" ThirdParty.ImagePicker.Components.Native.ImagePickerRender.render

    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.ImagePicker.Components.Base", ThirdParty.ImagePicker.Components.BaseStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("ThirdParty.ImagePicker.Components.Native.ImagePicker", ThirdParty.ImagePicker.Components.Native.ImagePickerStyles.styles)

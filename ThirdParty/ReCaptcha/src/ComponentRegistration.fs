// AUTO-GENERATED DO NOT EDIT
module ThirdParty.ReCaptcha.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.ReCaptcha.Components.With.Base" ThirdParty.ReCaptcha.Components.With.BaseRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.ReCaptcha.Components.With.Native" ThirdParty.ReCaptcha.Components.With.NativeRender.render
    LibClient.ComponentRegistry.RegisterRender "ThirdParty.ReCaptcha.Components.With.Web" ThirdParty.ReCaptcha.Components.With.WebRender.render



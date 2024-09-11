// AUTO-GENERATED DO NOT EDIT
module LibAutoUi.ComponentRegistration

let registerAllTheThings () : unit =
    LibClient.ComponentRegistry.RegisterRender "LibAutoUi.Components.DialogInputForm" LibAutoUi.Components.DialogInputFormRender.render
    LibClient.ComponentRegistry.RegisterRender "LibAutoUi.Components.InputFieldDateTime" LibAutoUi.Components.InputFieldDateTimeRender.render
    LibClient.ComponentRegistry.RegisterRender "LibAutoUi.Components.InputFieldString" LibAutoUi.Components.InputFieldStringRender.render
    LibClient.ComponentRegistry.RegisterRender "LibAutoUi.Components.InputForm" LibAutoUi.Components.InputFormRender.render
    LibClient.ComponentRegistry.RegisterRender "LibAutoUi.Components.InputFormElement" LibAutoUi.Components.InputFormElementRender.render

    LibClient.ComponentRegistry.RegisterStyles ("LibAutoUi.Components.DialogInputForm", LibAutoUi.Components.DialogInputFormStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("LibAutoUi.Components.InputFieldDateTime", LibAutoUi.Components.InputFieldDateTimeStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("LibAutoUi.Components.InputFieldString", LibAutoUi.Components.InputFieldStringStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("LibAutoUi.Components.InputForm", LibAutoUi.Components.InputFormStyles.styles)
    LibClient.ComponentRegistry.RegisterStyles ("LibAutoUi.Components.InputFormElement", LibAutoUi.Components.InputFormElementStyles.styles)

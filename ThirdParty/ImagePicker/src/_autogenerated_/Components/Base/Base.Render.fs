module ThirdParty.ImagePicker.Components.BaseRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.ImagePicker.Components

open LibLang
open LibClient.RenderHelpers
open LibClient

open ThirdParty.ImagePicker.Components.Base



let render(children: array<ReactElement>, props: ThirdParty.ImagePicker.Components.Base.Props, estate: ThirdParty.ImagePicker.Components.Base.Estate, pstate: ThirdParty.ImagePicker.Components.Base.Pstate, actions: ThirdParty.ImagePicker.Components.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = "view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                #if EGGSHELL_PLATFORM_IS_WEB
                let __parentFQN = Some "LibClient.Components.Input.Image"
                let __currClass = (System.String.Format("{0}", TopLevelBlockClass))
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                LibClient.Components.Constructors.LC.Input.Image(
                    onChange = (props.OnChange),
                    selectionMode = (props.SelectionMode),
                    showPreview = (props.ShowPreview),
                    ?maxFileSize = (props.MaxFileSize),
                    ?maxFileCount = (props.MaxFileCount),
                    validity = (props.Validity),
                    value = (props.Value),
                    ?styles = (props.styles),
                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                )
                #else
                let __parentFQN = Some "ThirdParty.ImagePicker.Components.Native.ImagePicker"
                let __currClass = (System.String.Format("{0}", TopLevelBlockClass))
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ThirdParty.ImagePicker.Components.Constructors.ImagePicker.Native.ImagePicker(
                    onChange = (props.OnChange),
                    selectionMode = (props.SelectionMode),
                    showPreview = (props.ShowPreview),
                    ?maxFileSize = (props.MaxFileSize),
                    ?maxFileCount = (props.MaxFileCount),
                    validity = (props.Validity),
                    value = (props.Value),
                    ?styles = (props.styles),
                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                )
                #endif
            |]
    )

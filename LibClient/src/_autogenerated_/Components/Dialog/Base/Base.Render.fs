module LibClient.Components.Dialog.BaseRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibClient.Components

open LibClient
open LibClient.RenderHelpers
open LibClient.Icons
open LibClient.Chars
open LibClient.ColorModule
open LibClient.LocalImages
open LibClient.Responsive

open LibClient.Components.Dialog.Base



let render(children: array<ReactElement>, props: LibClient.Components.Dialog.Base.Props, estate: LibClient.Components.Dialog.Base.Estate, pstate: LibClient.Components.Dialog.Base.Pstate, actions: LibClient.Components.Dialog.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let onPress = if ReactXP.Runtime.isNative() then Undefined else (actions.OnBackgroundPress)
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = "background"
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
            children =
                [|
                    let __parentFQN = Some "ReactXP.Components.View"
                    let __currClass = "dialog"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        onKeyPress = (actions.OnKeyPress),
                        onPress = (onPress),
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                match (props.ContentPosition) with
                                | Free ->
                                    [|
                                        castAsElement children
                                    |]
                                | _ ->
                                    [|
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = (System.String.Format("{0}{1}{2}", "position-wrapper position-", (props.ContentPosition), ""))
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            children = (children),
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                        )
                                    |]
                                |> castAsElementAckingKeysWarning
                            |]
                    )
                |]
        )
    )

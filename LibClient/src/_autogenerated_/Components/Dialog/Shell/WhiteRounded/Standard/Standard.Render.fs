module LibClient.Components.Dialog.Shell.WhiteRounded.StandardRender

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

open LibClient.Components.Dialog.Shell.WhiteRounded.Standard



let render(children: array<ReactElement>, props: LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Props, estate: LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Estate, pstate: LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Pstate, actions: LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.ScreenSize"
    LibClient.Components.Constructors.LC.With.ScreenSize(
        ``with`` =
            (fun (screenSize) ->
                    (castAsElementAckingKeysWarning [|
                        let __parentFQN = Some "LibClient.Components.Dialog.Shell.WhiteRounded.Raw"
                        LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded.Raw(
                            canClose = (props.CanClose),
                            children =
                                [|
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "dialog-contents"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        onPress = (fun e -> e.stopPropagation()),
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                (
                                                    (props.Heading)
                                                    |> Option.map
                                                        (fun heading ->
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            let __currClass = (System.String.Format("{0}{1}{2}", "heading ", (screenSize.Class), ""))
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            ReactXP.Components.Constructors.RX.View(
                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                children =
                                                                    [|
                                                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                                                            children =
                                                                                [|
                                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", heading))
                                                                                |]
                                                                        )
                                                                    |]
                                                            )
                                                        )
                                                    |> Option.getOrElse noElement
                                                )
                                                let __parentFQN = Some "ReactXP.Components.ScrollView"
                                                let __currClass = "scroll-view"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.ScrollView(
                                                    vertical = (true),
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            let __currClass = "body"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            ReactXP.Components.Constructors.RX.View(
                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                children =
                                                                    [|
                                                                        props.Body
                                                                    |]
                                                            )
                                                            (
                                                                (match props.Mode with | Error message -> Some message | _ -> None)
                                                                |> Option.map
                                                                    (fun message ->
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "error"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", message))
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                        )
                                                                    )
                                                                |> Option.getOrElse noElement
                                                            )
                                                            (
                                                                if (not (props.Buttons = noElement)) then
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "buttons"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                        children =
                                                                            [|
                                                                                props.Buttons
                                                                            |]
                                                                    )
                                                                else noElement
                                                            )
                                                        |]
                                                )
                                                (
                                                    if (props.Mode = InProgress) then
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "in-progress"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                        )
                                                    else noElement
                                                )
                                            |]
                                    )
                                |]
                        )
                    |])
            )
    )

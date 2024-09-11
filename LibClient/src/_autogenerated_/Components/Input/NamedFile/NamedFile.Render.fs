module LibClient.Components.Input.NamedFileRender

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

open LibClient.Components.Input.NamedFile
open LibLifeCycleTypes.File


let render(children: array<ReactElement>, props: LibClient.Components.Input.NamedFile.Props, estate: LibClient.Components.Input.NamedFile.Estate, pstate: LibClient.Components.Input.NamedFile.Pstate, actions: LibClient.Components.Input.NamedFile.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "view ", (TopLevelBlockClass), " ", (screenSize.Class), ""))
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    let __parentFQN = Some "LibClient.Components.With.RefDom"
                                    LibClient.Components.Constructors.LC.With.RefDom(
                                        onInitialize = (actions.OnDropZoneInitialize),
                                        ``with`` =
                                            (fun (bindDivRef, _) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        FRS.div
                                                            [(FRP.Style (([Fable.React.Props.CSSProp.Width "100%"; Fable.React.Props.CSSProp.Height "100%"]))); (FRP.Ref ((bindDivRef)))]
                                                            ([|
                                                                let __parentFQN = Some "LibClient.Components.With.RefDom"
                                                                LibClient.Components.Constructors.LC.With.RefDom(
                                                                    onInitialize = (actions.OnInputInitialize),
                                                                    ``with`` =
                                                                        (fun (bindRef, input) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    FRS.input
                                                                                        [(FRP.Accept ((props.AcceptedTypes |> Set.map (fun item -> item.Value) |> String.concat ", "))); (FRP.Multiple ((true))); (FRP.Hidden ((true))); (FRP.Ref ((bindRef))); (FRP.Type (("file")))]
                                                                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                                                                    LibClient.Components.Constructors.LC.Buttons(
                                                                                        align = (Align.Center),
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                                                LibClient.Components.Constructors.LC.Button(
                                                                                                    styles = ([| NamedFileStyles.Styles.selectFile |]),
                                                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (actions.OnSelectPress input))),
                                                                                                    label = ("Select File")
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                |])
                                                                        )
                                                                )
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "drag-and-drop-message"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                            let __currClass = "text-center"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            LibClient.Components.Constructors.LC.LegacyText(
                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                children =
                                                                                    [|
                                                                                        match (props.MaxFileCount) with
                                                                                        | Some maxFileCount when maxFileCount.Value = 1 ->
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN "Paste or drag and drop file here"
                                                                                            |]
                                                                                        | _ ->
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN "Paste or drag and drop files here"
                                                                                            |]
                                                                                        |> castAsElementAckingKeysWarning
                                                                                    |]
                                                                            )
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "constrain-message"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                            let __currClass = "text-center"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            LibClient.Components.Constructors.LC.LegacyText(
                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                children =
                                                                                    [|
                                                                                        match (constrainMessage props.MaxFileCount props.MaxFileSize props.MaxTotalFileSize) with
                                                                                        | Some message ->
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", message.Value))
                                                                                            |]
                                                                                        | None ->
                                                                                            [|
                                                                                                noElement
                                                                                            |]
                                                                                        |> castAsElementAckingKeysWarning
                                                                                    |]
                                                                            )
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "message-container"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                children =
                                                                                    [|
                                                                                        (
                                                                                            if (props.Value.Length = 1) then
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                                                let __currClass = "text-center info-message"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.LegacyText(
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "", (props.Value.Length), " file selected"))
                                                                                                        |]
                                                                                                )
                                                                                            else noElement
                                                                                        )
                                                                                        (
                                                                                            if (props.Value.Length > 1) then
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                                                let __currClass = "text-center info-message"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.LegacyText(
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "", (props.Value.Length), " files selected"))
                                                                                                        |]
                                                                                                )
                                                                                            else noElement
                                                                                        )
                                                                                    |]
                                                                            )
                                                                            (
                                                                                (if estate.InternalValidity.InvalidReason.IsSome then estate.InternalValidity.InvalidReason else props.Validity.InvalidReason)
                                                                                |> Option.map
                                                                                    (fun reason ->
                                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                                        ReactXP.Components.Constructors.RX.View(
                                                                                            children =
                                                                                                [|
                                                                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                                                    let __currClass = "invalid-reason"
                                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                        children =
                                                                                                            [|
                                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", reason))
                                                                                                            |]
                                                                                                    )
                                                                                                |]
                                                                                        )
                                                                                    )
                                                                                |> Option.getOrElse noElement
                                                                            )
                                                                            (
                                                                                if (props.Validity = InputValidity.Missing) then
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        children =
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                                                let __currClass = "invalid-reason"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.LegacyText(
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "This field is required"
                                                                                                        |]
                                                                                                )
                                                                                            |]
                                                                                    )
                                                                                else noElement
                                                                            )
                                                                        |]
                                                                )
                                                            |])
                                                    |])
                                            )
                                    )
                                |]
                        )
                    |])
            )
    )

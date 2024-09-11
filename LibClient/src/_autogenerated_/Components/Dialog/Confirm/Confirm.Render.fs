module LibClient.Components.Dialog.ConfirmRender

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

open LibClient.Components.Dialog.Confirm



let render(children: array<ReactElement>, props: LibClient.Components.Dialog.Confirm.Props, estate: LibClient.Components.Dialog.Confirm.Estate, pstate: LibClient.Components.Dialog.Confirm.Pstate, actions: LibClient.Components.Dialog.Confirm.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Dialog.Shell.WhiteRounded.Standard"
    LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded.Standard(
        canClose = (LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Never),
        mode = (match estate.Mode with | Mode.InProgress -> LibClient.Components.Dialog.Shell.WhiteRounded.Standard.InProgress | Mode.Error m -> LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Error m | _ -> LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Default),
        ?heading = (props.Parameters.MaybeHeading),
        body =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "ReactXP.Components.View"
                    let __currClass = "details"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                let __currClass = "details-text"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                LibClient.Components.Constructors.LC.LegacyText(
                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                    children =
                                        [|
                                            makeTextNode2 __parentFQN (System.String.Format("{0}", props.Parameters.Details))
                                        |]
                                )
                            |]
                    )
                |]),
        buttons =
                (castAsElementAckingKeysWarning [|
                    (
                        (props.Parameters.Buttons)
                        |> Seq.map
                            (fun button ->
                                (castAsElementAckingKeysWarning [|
                                    match (button) with
                                    | Cancel (label, level, callback) ->
                                        [|
                                            let __parentFQN = Some "LibClient.Components.Button"
                                            LibClient.Components.Constructors.LC.Button(
                                                state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (match estate.Mode with | Mode.InProgress -> LibClient.Components.Button.Disabled | _ -> LibClient.Components.Button.Actionable (fun e -> callback(); actions.TryCancel e))),
                                                level = (level),
                                                label = (label)
                                            )
                                        |]
                                    | Confirm (label, level, callback) ->
                                        [|
                                            let __parentFQN = Some "LibClient.Components.Button"
                                            LibClient.Components.Constructors.LC.Button(
                                                state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (match estate.Mode with | Mode.InProgress -> LibClient.Components.Button.Disabled | _ -> LibClient.Components.Button.Actionable (fun e -> callback(); actions.Hide e))),
                                                level = (level),
                                                label = (label)
                                            )
                                        |]
                                    | AsyncConfirm (label, level, work) ->
                                        [|
                                            let __parentFQN = Some "LibClient.Components.Button"
                                            LibClient.Components.Constructors.LC.Button(
                                                state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (match estate.Mode with | Mode.InProgress -> LibClient.Components.Button.InProgress | _ -> LibClient.Components.Button.Actionable (actions.AsyncConfirm work))),
                                                level = (level),
                                                label = (label)
                                            )
                                        |]
                                    |> castAsElementAckingKeysWarning
                                |])
                            )
                        |> Array.ofSeq |> castAsElement
                    )
                |])
    )

module LibClient.Components.Dialog.PromptRender

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

open LibClient.Components.Dialog.Prompt



let render(children: array<ReactElement>, props: LibClient.Components.Dialog.Prompt.Props, estate: LibClient.Components.Dialog.Prompt.Estate, pstate: LibClient.Components.Dialog.Prompt.Pstate, actions: LibClient.Components.Dialog.Prompt.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Form.Base"
    LibClient.Components.Constructors.LC.Form.Base(
        submit = (actions.Submit),
        accumulator = (LibClient.Components.Form.Base.ManageInternallyInitializingWith Acc.Initial),
        content =
            (fun (form) ->
                    (castAsElementAckingKeysWarning [|
                        let __parentFQN = Some "LibClient.Components.Dialog.Shell.WhiteRounded.Standard"
                        LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded.Standard(
                            canClose = (LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Never),
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
                                        let __parentFQN = Some "LibClient.Components.Input.Text"
                                        LibClient.Components.Constructors.LC.Input.Text(
                                            onChange = (fun value -> form.UpdateAcc (fun acc -> { acc with Value = value })),
                                            value = (form.Acc.Value),
                                            validity = (form.FieldValidity Field.Value),
                                            label = ("Value")
                                        )
                                    |]),
                            buttons =
                                    (castAsElementAckingKeysWarning [|
                                        let __parentFQN = Some "LibClient.Components.Button"
                                        LibClient.Components.Constructors.LC.Button(
                                            state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable actions.TryCancel)),
                                            label = ("Cancel"),
                                            level = (LibClient.Components.Button.Secondary)
                                        )
                                        let __parentFQN = Some "LibClient.Components.Button"
                                        LibClient.Components.Constructors.LC.Button(
                                            state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (if form.IsSubmitInProgress then LibClient.Components.Button.InProgress else LibClient.Components.Button.Actionable form.TrySubmitLowLevel)),
                                            label = ("Submit")
                                        )
                                    |])
                        )
                    |])
            )
    )

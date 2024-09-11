module LibRouter.Components.DialogsRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ReactXP.Components
open LibRouter.Components

open LibClient
open LibClient.RenderHelpers

open LibRouter.Components.Dialogs
open LibRouter.RoutesSpec
open LibClient.SystemDialogs


let render(children: array<ReactElement>, props: LibRouter.Components.Dialogs.Props<'Route, 'ResultlessDialog, 'ResultfulDialog>, estate: LibRouter.Components.Dialogs.Estate<'Route, 'ResultlessDialog, 'ResultfulDialog>, pstate: LibRouter.Components.Dialogs.Pstate, actions: LibRouter.Components.Dialogs.Actions<'Route, 'ResultlessDialog, 'ResultfulDialog>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        if (props.Dialogs.IsNonempty) then
            let __parentFQN = Some "ReactXP.Components.View"
            let __currClass = "view"
            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
            ReactXP.Components.Constructors.RX.View(
                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                children =
                    [|
                        (*  switching between 1 dialog and 2 or more dialogs
             is a fundamental switch that breaks continuity,
             i.e. the first dialog is reconstructed, instead of
             being kept around and just re-rendered when required,
             and thus loses state if it had any. Having this div
             as a sentinel keeps React thinking that multiple children
             are present regardless of the dialog count.  *)
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = "sentinel"
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                        )
                        (
                            (List.rev props.Dialogs)
                            |> Seq.map
                                (fun dialog ->
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "frame"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                match (dialog) with
                                                | NavigationDialog.Resultless (token, resultlessDialog) ->
                                                    [|
                                                        props.MakeResultless (resultlessDialog, props.Nav.Close token)
                                                    |]
                                                | NavigationDialog.Resultful token ->
                                                    [|
                                                        (
                                                            (props.DialogsState.TryGetResultful token)
                                                            |> Option.map
                                                                (fun resultfulDialog ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        props.MakeResultful (resultfulDialog, props.Nav.Close token)
                                                                    |])
                                                                )
                                                            |> Option.getOrElse noElement
                                                        )
                                                    |]
                                                | NavigationDialog.AdHoc token ->
                                                    [|
                                                        (
                                                            (props.DialogsState.TryGetAdHoc token)
                                                            |> Option.map
                                                                (fun adHocCloseToDialog ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        adHocCloseToDialog (props.Nav.Close token)
                                                                    |])
                                                                )
                                                            |> Option.getOrElse noElement
                                                        )
                                                    |]
                                                | NavigationDialog.System token ->
                                                    [|
                                                        (
                                                            (props.DialogsState.TryGetSystem token)
                                                            |> Option.map
                                                                (fun systemDialog ->
                                                                    (
                                                                        let close = props.Nav.Close token
                                                                        (castAsElementAckingKeysWarning [|
                                                                            match (systemDialog) with
                                                                            | Alert (maybeHeading, details)                                          ->
                                                                                [|
                                                                                    LibClient.Components.Dialog.Confirm.OpenAsAlert maybeHeading details                                   close 
                                                                                |]
                                                                            | ImageViewer (sources, initialIndex)                                    ->
                                                                                [|
                                                                                    LC.Dialog.OpenImageViewer(sources, close, initialIndex) 
                                                                                |]
                                                                            | ImageViewerCustom (sources, initialIndex, resizeMode)                  ->
                                                                                [|
                                                                                    LC.Dialog.OpenImageViewer(sources, close, initialIndex, resizeMode)
                                                                                |]
                                                                            | ConfirmCustom (maybeHeading, details, buttons)                         ->
                                                                                [|
                                                                                    LibClient.Components.Dialog.Confirm.Open maybeHeading details buttons                                  close 
                                                                                |]
                                                                            | Confirm       (maybeHeading, details, cancelLabel, okLabel, onResult)  ->
                                                                                [|
                                                                                    LibClient.Components.Dialog.Confirm.OpenSimple maybeHeading details cancelLabel okLabel onResult       close 
                                                                                |]
                                                                            | ConfirmAsync  (maybeHeading, details, cancelLabel, okLabel, onConfirm) ->
                                                                                [|
                                                                                    LibClient.Components.Dialog.Confirm.OpenSimpleAsync maybeHeading details cancelLabel okLabel onConfirm close 
                                                                                |]
                                                                            | Prompt        (maybeHeading, details, onResult)                        ->
                                                                                [|
                                                                                    LibClient.Components.Dialog.Prompt.Open maybeHeading details onResult                                  close 
                                                                                |]
                                                                            |> castAsElementAckingKeysWarning
                                                                        |])
                                                                    )
                                                                )
                                                            |> Option.getOrElse noElement
                                                        )
                                                    |]
                                                |> castAsElementAckingKeysWarning
                                            |]
                                    )
                                )
                            |> Array.ofSeq |> castAsElement
                        )
                    |]
            )
        else noElement
    )

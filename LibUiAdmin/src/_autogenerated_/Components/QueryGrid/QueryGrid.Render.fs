module LibUiAdmin.Components.QueryGridRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibUiAdmin.Components
open LibUiAdmin.Components

open LibClient
open LibClient.RenderHelpers
open LibClient.Chars

open LibUiAdmin.Components.QueryGrid



let render(children: array<ReactElement>, props: LibUiAdmin.Components.QueryGrid.Props<'Item, 'QueryFormField, 'QueryAcc, 'Query>, estate: LibUiAdmin.Components.QueryGrid.Estate<'Item, 'QueryFormField, 'QueryAcc, 'Query>, pstate: LibUiAdmin.Components.QueryGrid.Pstate, actions: LibUiAdmin.Components.QueryGrid.Actions<'Item, 'QueryFormField, 'QueryAcc, 'Query>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let form = (
                (castAsElementAckingKeysWarning [|
                    match (props.QueryForm) with
                    | Some queryForm ->
                        [|
                            let __parentFQN = Some "LibClient.Components.Form.Base"
                            LibClient.Components.Constructors.LC.Form.Base(
                                submit = (actions.Submit),
                                accumulator = (LibClient.Components.Form.Base.ManageInternallyInitializingWith props.InitialQueryAcc),
                                content =
                                    (fun (form: LibClient.Components.Form.Base.FormHandle<'QueryFormField, 'QueryAcc, 'Query>) ->
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "query-block"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            queryForm form
                                                        |]
                                                )
                                                let __parentFQN = Some "LibClient.Components.Buttons"
                                                LibClient.Components.Constructors.LC.Buttons(
                                                    align = (LibClient.Components.Buttons.Left),
                                                    styles = ([| QueryGridStyles.Styles.buttonsBlock |]),
                                                    children =
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.Button"
                                                            let __currClass = "anitest"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            LibClient.Components.Constructors.LC.Button(
                                                                state = (LibClient.Components.Button.PropStateFactory.Make form.TrySubmit),
                                                                label = ("Submit"),
                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                            )
                                                        |]
                                                )
                                            |])
                                    )
                            )
                        |]
                    | None ->
                        [||]
                    |> castAsElementAckingKeysWarning
                |])
        )
        let grid = (
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "ReactXP.Components.View"
                    let __currClass = "grid-block"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                match (props.HandheldRow) with
                                | None ->
                                    [|
                                        let __parentFQN = Some "LibUiAdmin.Components.Grid"
                                        LibUiAdmin.Components.Constructors.UiAdmin.Grid(
                                            headers = (props.Headers),
                                            input = (LibUiAdmin.Components.Grid.Paginated (estate.GridData, (fun item -> props.Row (item, estate.MaybeCurrentQueryPage, actions.Refresh)), None))
                                        )
                                    |]
                                | Some handheldRow ->
                                    [|
                                        let __parentFQN = Some "LibUiAdmin.Components.Grid"
                                        LibUiAdmin.Components.Constructors.UiAdmin.Grid(
                                            headers = (props.Headers),
                                            input = (LibUiAdmin.Components.Grid.Paginated (estate.GridData, (fun item -> props.Row (item, estate.MaybeCurrentQueryPage, actions.Refresh)), Some (fun item -> handheldRow (item, estate.MaybeCurrentQueryPage, actions.Refresh))))
                                        )
                                    |]
                                |> castAsElementAckingKeysWarning
                            |]
                    )
                |])
        )
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
            children =
                [|
                    match (props.CustomRender) with
                    | None ->
                        [|
                            form
                            grid
                        |]
                    | Some customRender ->
                        [|
                            customRender (form, grid)
                        |]
                    |> castAsElementAckingKeysWarning
                |]
        )
    |])

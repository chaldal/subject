module LibUiAdmin.Components.GridRender

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

open LibUiAdmin.Components.Grid



let render(children: array<ReactElement>, props: LibUiAdmin.Components.Grid.Props<'T>, estate: LibUiAdmin.Components.Grid.Estate<'T>, pstate: LibUiAdmin.Components.Grid.Pstate, actions: LibUiAdmin.Components.Grid.Actions<'T>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let gridView(isHandheldMode: bool) = (
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "ReactXP.Components.View"
                    let __currClass = "view" + System.String.Format(" {0}", (if (props.HandleVerticalScrolling) then "full-height" else ""))
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                let navRow = (
                                        (castAsElementAckingKeysWarning [|
                                            match (props.Input) with
                                            | Paginated (data, _, _) ->
                                                [|
                                                    let quadState = (
                                                            (castAsElementAckingKeysWarning [|
                                                                let __parentFQN = Some "LibClient.Components.QuadStateful"
                                                                LibClient.Components.Constructors.LC.QuadStateful(
                                                                    act = (goToPageAdapter data.GoToPage data.PageSize),
                                                                    validate = (match data.MaybePageCount with | None -> id | Some pageCount -> Option.flatMap (fun v -> if v.Value > pageCount.Value then None else Some v)),
                                                                    initialInputAcc = (LC.QuadStateful.Sync (Some data.PageNumber)),
                                                                    initial =
                                                                        (fun (_edit) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(if data.PageNumber = PositiveInteger.One then LC.IconButton.Disabled else LC.IconButton.Actionable (Some >> data.GoToPage data.PageSize PositiveInteger.One))),
                                                                                        icon = (LibClient.Icons.Icon.First),
                                                                                        styles = ([| GridStyles.Styles.navButton |]),
                                                                                        theme = (GridStyles.Styles.navButtonTheme)
                                                                                    )
                                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(match PositiveInteger.ofInt (data.PageNumber.Value - 1) with | None -> LC.IconButton.Disabled | Some newPageNumber -> LC.IconButton.Actionable (Some >> data.GoToPage data.PageSize newPageNumber))),
                                                                                        icon = (LibClient.Icons.Icon.Previous),
                                                                                        styles = ([| GridStyles.Styles.navButton |]),
                                                                                        theme = (GridStyles.Styles.navButtonTheme)
                                                                                    )
                                                                                    match (data.MaybePageCount) with
                                                                                    | None ->
                                                                                        [|
                                                                                            match (data.Items) with
                                                                                            | Available items when items |> Seq.length = 0 ->
                                                                                                [|
                                                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.Disabled)),
                                                                                                        icon = (LibClient.Icons.Icon.Next),
                                                                                                        styles = ([| GridStyles.Styles.navButton |]),
                                                                                                        theme = (GridStyles.Styles.navButtonTheme)
                                                                                                    )
                                                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.Disabled)),
                                                                                                        icon = (LibClient.Icons.Icon.Last),
                                                                                                        styles = ([| GridStyles.Styles.navButton |]),
                                                                                                        theme = (GridStyles.Styles.navButtonTheme)
                                                                                                    )
                                                                                                |]
                                                                                            | _ ->
                                                                                                [|
                                                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.Actionable (Some >> data.GoToPage data.PageSize (data.PageNumber + PositiveInteger.One)))),
                                                                                                        icon = (LibClient.Icons.Icon.Next),
                                                                                                        styles = ([| GridStyles.Styles.navButton |]),
                                                                                                        theme = (GridStyles.Styles.navButtonTheme)
                                                                                                    )
                                                                                                    match (isHandheldMode) with
                                                                                                    | true ->
                                                                                                        [|
                                                                                                            let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                                            LibClient.Components.Constructors.LC.IconButton(
                                                                                                                state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.Disabled)),
                                                                                                                icon = (LibClient.Icons.Icon.Last),
                                                                                                                styles = ([| GridStyles.Styles.navButton |]),
                                                                                                                theme = (GridStyles.Styles.navButtonTheme)
                                                                                                            )
                                                                                                        |]
                                                                                                    | _ ->
                                                                                                        [|
                                                                                                            let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                            LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                                children =
                                                                                                                    [|
                                                                                                                        makeTextNode2 __parentFQN "Unknown Pages"
                                                                                                                    |]
                                                                                                            )
                                                                                                        |]
                                                                                                    |> castAsElementAckingKeysWarning
                                                                                                |]
                                                                                            |> castAsElementAckingKeysWarning
                                                                                        |]
                                                                                    | Some pageCount ->
                                                                                        [|
                                                                                            (
                                                                                                let pagesToShow = actions.GeneratePagesToShow data.PageNumber.Value pageCount.Value
                                                                                                let pagesToShowCount = pagesToShow |> List.length
                                                                                                (castAsElementAckingKeysWarning [|
                                                                                                    (
                                                                                                        (pagesToShow)
                                                                                                        |> Seq.mapi
                                                                                                            (fun index pageNumber ->
                                                                                                                (
                                                                                                                    let isCurrentPage = pageNumber = data.PageNumber.Value
                                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                                                                        ReactXP.Components.Constructors.RX.View(
                                                                                                                            styles = ([| GridStyles.Styles.navPageNumber; if isCurrentPage then GridStyles.Styles.navCurrentPage |]),
                                                                                                                            children =
                                                                                                                                [|
                                                                                                                                    let __parentFQN = Some "LibClient.Components.TextButton"
                                                                                                                                    LibClient.Components.Constructors.LC.TextButton(
                                                                                                                                        label = (System.String.Format("{0}", pageNumber)),
                                                                                                                                        state = (LibClient.Components.TextButton.PropStateFactory.MakeLowLevel(match isCurrentPage with false -> LC.TextButton.Actionable (Some >> data.GoToPage data.PageSize (PositiveInteger.ofLiteral pageNumber)) | true -> LC.TextButton.Disabled))
                                                                                                                                    )
                                                                                                                                |]
                                                                                                                        )
                                                                                                                        (
                                                                                                                            if (pagesToShowCount > 1 && ((index = 0 && pagesToShow.[0] + 1 <> pagesToShow.[1]) || (index = pagesToShowCount - 2 && (pagesToShow.[pagesToShowCount - 2] + 1 <> pagesToShow.[pagesToShowCount - 1])))) then
                                                                                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                                                let __currClass = "nav-page-internal-dot"
                                                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                                                    children =
                                                                                                                                        [|
                                                                                                                                            makeTextNode2 __parentFQN "•••"
                                                                                                                                        |]
                                                                                                                                )
                                                                                                                            else noElement
                                                                                                                        )
                                                                                                                    |])
                                                                                                                )
                                                                                                            )
                                                                                                        |> Array.ofSeq |> castAsElement
                                                                                                    )
                                                                                                    let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                                    LibClient.Components.Constructors.LC.IconButton(
                                                                                                        state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(if data.PageNumber.Value >= pageCount.Value then LC.IconButton.Disabled else LC.IconButton.Actionable (Some >> data.GoToPage data.PageSize (data.PageNumber + PositiveInteger.One)))),
                                                                                                        icon = (LibClient.Icons.Icon.Next),
                                                                                                        styles = ([| GridStyles.Styles.navButton |]),
                                                                                                        theme = (GridStyles.Styles.navButtonTheme)
                                                                                                    )
                                                                                                    match (PositiveInteger.ofUnsignedInteger pageCount) with
                                                                                                    | None ->
                                                                                                        [||]
                                                                                                        (*  no last page to go to  *)
                                                                                                    | Some lastPage ->
                                                                                                        [|
                                                                                                            let __parentFQN = Some "LibClient.Components.IconButton"
                                                                                                            LibClient.Components.Constructors.LC.IconButton(
                                                                                                                state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(if data.PageNumber.Value >= pageCount.Value then LC.IconButton.Disabled else LC.IconButton.Actionable (Some >> data.GoToPage data.PageSize lastPage))),
                                                                                                                icon = (LibClient.Icons.Icon.Last),
                                                                                                                styles = ([| GridStyles.Styles.navButton |]),
                                                                                                                theme = (GridStyles.Styles.navButtonTheme)
                                                                                                            )
                                                                                                        |]
                                                                                                    |> castAsElementAckingKeysWarning
                                                                                                |])
                                                                                            )
                                                                                        |]
                                                                                    |> castAsElementAckingKeysWarning
                                                                                    match (data.MaybeTotalItemCount, isHandheldMode) with
                                                                                    | Some totalItemCount, false ->
                                                                                        [|
                                                                                            let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                            let __currClass = "result-count"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                children =
                                                                                                    [|
                                                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Total ", (pluralize (uint32 totalItemCount.Value) "result" "results"), ""))
                                                                                                    |]
                                                                                            )
                                                                                        |]
                                                                                    | _ ->
                                                                                        [|
                                                                                            noElement
                                                                                        |]
                                                                                    |> castAsElementAckingKeysWarning
                                                                                |])
                                                                        ),
                                                                    input =
                                                                        (fun (_, setInput, maybeSave, cancel) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    let __parentFQN = Some "LibClient.Components.With.Ref"
                                                                                    LibClient.Components.Constructors.LC.With.Ref(
                                                                                        onInitialize = (actions.OnJumpToPageInitialize),
                                                                                        ``with`` =
                                                                                            (fun (bindJumpToPageRef, _) ->
                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                        let __parentFQN = Some "LibClient.Components.Legacy.Input.PositiveInteger"
                                                                                                        LibClient.Components.Constructors.LC.Legacy.Input.PositiveInteger(
                                                                                                            onKeyPress = (actions.OnJumpToPageNumberKeyPress maybeSave cancel),
                                                                                                            onChange = (Result.toOption >> setInput),
                                                                                                            initialValue = (data.PageNumber),
                                                                                                            ref = (bindJumpToPageRef)
                                                                                                        )
                                                                                                    |])
                                                                                            )
                                                                                    )
                                                                                |])
                                                                        )
                                                                )
                                                            |])
                                                    )
                                                    match (isHandheldMode) with
                                                    | true ->
                                                        [|
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            let __currClass = "paginationHandheld"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            ReactXP.Components.Constructors.RX.View(
                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                children =
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "navigation"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    quadState
                                                                                |]
                                                                        )
                                                                    |]
                                                            )
                                                        |]
                                                    | false ->
                                                        [|
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            let __currClass = "pagination"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            ReactXP.Components.Constructors.RX.View(
                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                children =
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "navigation"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    quadState
                                                                                |]
                                                                        )
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "page-info-container"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    match (props.Input) with
                                                                                    | Paginated (data, _, _) ->
                                                                                        [|
                                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                                            let __currClass = "page-size"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                                children =
                                                                                                    [|
                                                                                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                        let __currClass = "page-size-text"
                                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                            children =
                                                                                                                [|
                                                                                                                    makeTextNode2 __parentFQN "Page Size"
                                                                                                                |]
                                                                                                        )
                                                                                                        let __parentFQN = Some "LibClient.Components.Legacy.Input.Picker"
                                                                                                        let __currClass = "picker"
                                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        LibClient.Components.Constructors.LC.Legacy.Input.Picker(
                                                                                                            validity = (Valid),
                                                                                                            onChange = (LibClient.Components.Legacy.Input.Picker.CannotUnselect (fun (index, _) -> actions.ResizePage data (props.PageSizeChoices.Item index))),
                                                                                                            value = (LibClient.Components.Legacy.Input.Picker.ByItem data.PageSize |> Some),
                                                                                                            items = (props.PageSizeChoices |> List.map (fun size -> { Label = size.Value.ToString(); Item = size })),
                                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                        )
                                                                                                    |]
                                                                                            )
                                                                                        |]
                                                                                    | _ ->
                                                                                        [|
                                                                                            noElement
                                                                                        |]
                                                                                    |> castAsElementAckingKeysWarning
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    let __currClass = "curr-page-info"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                        children =
                                                                                            [|
                                                                                                match (data.Items) with
                                                                                                | Available items when items |> Seq.length = 0 ->
                                                                                                    [||]
                                                                                                | _ ->
                                                                                                    [|
                                                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}{3}{4}", "Showing ", (data.PageNumber.Value), " of ", (data.MaybePageCount |> Option.mapOrElse "unknown" (fun p -> string(p.Value))), " pages"))
                                                                                                    |]
                                                                                                |> castAsElementAckingKeysWarning
                                                                                            |]
                                                                                    )
                                                                                    let __parentFQN = Some "LibClient.Components.Input.PositiveInteger"
                                                                                    let __currClass = "goto-page-input"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.Input.PositiveInteger(
                                                                                        validity = (Valid),
                                                                                        onChange = (fun v -> actions.UpdateJumpToPage v),
                                                                                        value = (estate.JumpToPage),
                                                                                        placeholder = ("Page no"),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                    )
                                                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                                                    let __currClass = "goto-page-btn"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.Button(
                                                                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (match estate.JumpToPage.Result with | Ok (Some page) -> LibClient.Components.Button.Actionable (Some >> data.GoToPage data.PageSize page) | _ -> LibClient.Components.Button.Disabled)),
                                                                                        label = ("Go"),
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |]
                                                            )
                                                        |]
                                                    |> castAsElementAckingKeysWarning
                                                |]
                                            | _ ->
                                                [|
                                                    noElement
                                                |]
                                            |> castAsElementAckingKeysWarning
                                        |])
                                )
                                if isHandheldMode then noElement else navRow
                                let __parentFQN = Some "ReactXP.Components.ScrollView"
                                let __currClass = "scroll-view-vertical"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.ScrollView(
                                    scrollEnabled = (props.HandleVerticalScrolling),
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.ScrollView"
                                            let __currClass = "scroll-view-horizontal"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.ScrollView(
                                                horizontal = (true),
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let maybeHeaders = (
                                                                (castAsElementAckingKeysWarning [|
                                                                    match ((props.Headers, props.HeadersRaw)) with
                                                                    | (_, Some headersRaw) ->
                                                                        [|
                                                                            FRS.thead
                                                                                [(FRP.ClassName ("headers"))]
                                                                                ([|
                                                                                    headersRaw
                                                                                |])
                                                                        |]
                                                                    | (Some headers, None) ->
                                                                        [|
                                                                            FRS.thead
                                                                                [(FRP.ClassName ("headers"))]
                                                                                ([|
                                                                                    FRS.tr
                                                                                        []
                                                                                        ([|
                                                                                            headers
                                                                                        |])
                                                                                |])
                                                                        |]
                                                                    | (None, None) ->
                                                                        [||]
                                                                    |> castAsElementAckingKeysWarning
                                                                |])
                                                        )
                                                        match (props.Input) with
                                                        | Static (rows, maybeHandheldRows) ->
                                                            [|
                                                                match (isHandheldMode, maybeHandheldRows) with
                                                                | true, Some handheldRows ->
                                                                    [|
                                                                        handheldRows
                                                                    |]
                                                                | _ ->
                                                                    [|
                                                                        FRS.table
                                                                            [(FRP.ClassName ("la-table"))]
                                                                            ([|
                                                                                maybeHeaders
                                                                                FRS.tbody
                                                                                    [(FRP.ClassName ("rows"))]
                                                                                    ([|
                                                                                        rows
                                                                                    |])
                                                                            |])
                                                                    |]
                                                                |> castAsElementAckingKeysWarning
                                                            |]
                                                        | Everything (asyncItems, makeDesktopRow, maybeMakeHandheldRow) | Paginated ({ Items = asyncItems }, makeDesktopRow, maybeMakeHandheldRow) ->
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.AsyncData"
                                                                LibClient.Components.Constructors.LC.AsyncData(
                                                                    data = (asyncItems),
                                                                    whenAvailable =
                                                                        (fun (items: seq<'T>) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    match (items |> Seq.isEmpty) with
                                                                                    | true ->
                                                                                        [|
                                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                                            let __currClass = "empty-message"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                                children =
                                                                                                    [|
                                                                                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                                        let __currClass = "empty-message-text"
                                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                            children =
                                                                                                                [|
                                                                                                                    makeTextNode2 __parentFQN "No Rows"
                                                                                                                |]
                                                                                                        )
                                                                                                    |]
                                                                                            )
                                                                                        |]
                                                                                    | false ->
                                                                                        [|
                                                                                            match (isHandheldMode, maybeMakeHandheldRow) with
                                                                                            | true, Some makeHandheldRows ->
                                                                                                [|
                                                                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                                                                    LibClient.Components.Constructors.LC.ItemList(
                                                                                                        style = (LibClient.Components.ItemList.Raw),
                                                                                                        items = (items),
                                                                                                        whenNonempty =
                                                                                                            (fun (items) ->
                                                                                                                    (castAsElementAckingKeysWarning [|
                                                                                                                        (
                                                                                                                            (items)
                                                                                                                            |> Seq.mapi
                                                                                                                                (fun index item ->
                                                                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                                                                        key = (props.ItemKey |> Option.map (fun f -> f item) |> Option.getOrElse $"{index}"),
                                                                                                                                        children =
                                                                                                                                            [|
                                                                                                                                                makeHandheldRows item
                                                                                                                                            |]
                                                                                                                                    )
                                                                                                                                )
                                                                                                                            |> Array.ofSeq |> castAsElement
                                                                                                                        )
                                                                                                                    |])
                                                                                                            )
                                                                                                    )
                                                                                                |]
                                                                                            | _ ->
                                                                                                [|
                                                                                                    FRS.table
                                                                                                        [(FRP.ClassName ("la-table"))]
                                                                                                        ([|
                                                                                                            maybeHeaders
                                                                                                            FRS.tbody
                                                                                                                [(FRP.ClassName ("rows"))]
                                                                                                                ([|
                                                                                                                    (* 
                                                        Rows are explicitly positioned as relative to allow for absolute positioning within that row.
                                                        Without this, absolute positioning will search up the tree for the first element with explicit
                                                        positioning and position relative to that rather than relative to the row.
                                                         *)
                                                                                                                    (
                                                                                                                        (items)
                                                                                                                        |> Seq.mapi
                                                                                                                            (fun index item ->
                                                                                                                                FRS.tr
                                                                                                                                    [unbox("key", (props.ItemKey |> Option.map (fun f -> f item) |> Option.getOrElse $"{index}")); (FRP.ClassName ("row" + System.String.Format(" {0}", (if (index % 2 = 0) then "row-alt" else ""))))]
                                                                                                                                    ([|
                                                                                                                                        makeDesktopRow item
                                                                                                                                    |])
                                                                                                                            )
                                                                                                                        |> Array.ofSeq |> castAsElement
                                                                                                                    )
                                                                                                                |])
                                                                                                        |])
                                                                                                |]
                                                                                            |> castAsElementAckingKeysWarning
                                                                                        |]
                                                                                    |> castAsElementAckingKeysWarning
                                                                                |])
                                                                        ),
                                                                    whenFailed =
                                                                        (fun (error) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    let __parentFQN = Some "LibClient.Components.InfoMessage"
                                                                                    LibClient.Components.Constructors.LC.InfoMessage(
                                                                                        level = (LibClient.Components.InfoMessage.Caution),
                                                                                        message = (error.ToString())
                                                                                    )
                                                                                |])
                                                                        )
                                                                )
                                                            |]
                                                        |> castAsElementAckingKeysWarning
                                                    |]
                                            )
                                        |]
                                )
                                navRow
                            |]
                    )
                |])
        )
        #if EGGSHELL_PLATFORM_IS_WEB
        gridView(false)
        #else
        gridView(true)
        #endif
    |])

module LibUiSubjectAdmin.Components.AuditRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard

open LibClient.RenderResultModule

open LibUiAdmin.Components
open LibClient.Components
open LibUiSubject.Components
open LibUiSubjectAdmin.Components
open ReactXP.Components
open LibUiSubjectAdmin.Components

open LibClient
open LibClient.RenderHelpers

open LibUiSubjectAdmin.Components.Audit



let render(props: LibUiSubjectAdmin.Components.Audit.Props, estate: LibUiSubjectAdmin.Components.Audit.Estate, pstate: LibUiSubjectAdmin.Components.Audit.Pstate, actions: LibUiSubjectAdmin.Components.Audit.Actions, __componentStyles: ReactXP.Styles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.Styles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (fun (__sibI, __sibC, __pFQN) ->
        let __parentFQN = Some "ReactXP.Components.View"
        (
            (
                let __currClass = "view" + (nthChildClasses __sibI __sibC)
                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                (ReactXP.Components.TypeExtensions.TEs.View(
                    ?xClassName = (if __currClass <> "" then Some __currClass else None),
                    ?style = (if (not __currStyles.IsEmpty) then (ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        (
                            let __list = [
                                (fun (__sibI, __sibC, __pFQN) ->
                                    let __parentFQN = Some "LibUiAdmin.Components.Grid"
                                    (
                                        (
                                            let makeRow(entry: UntypedSubjectAuditData) =
                                                FRH.fragment []
                                                    (
                                                        let defaults() = (
                                                            FRH.fragment []
                                                                (
                                                                    let __list = [
                                                                        (
                                                                            FRS.td
                                                                                []
                                                                                (
                                                                                    let __list = [
                                                                                        (
                                                                                            match (props.Timestamp) with
                                                                                            | None ->
                                                                                                (
                                                                                                    let __list = [
                                                                                                        (fun (__sibI, __sibC, __pFQN) ->
                                                                                                            let __parentFQN = Some "LibClient.Components.Timestamp"
                                                                                                            (
                                                                                                                (
                                                                                                                    let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                                    let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                                    (LibClient.Components.TypeExtensions.TEs.Timestamp(
                                                                                                                        value = (LibClient.Components.Timestamp.PropValueFactory.Make (entry.AsOf)),
                                                                                                                        ?xClassName = (if __currClass <> "" then Some __currClass else None),
                                                                                                                        ?xStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                                    ))
                                                                                                                )
                                                                                                            )
                                                                                                        ) |> RenderResult.Make
                                                                                                    ]
                                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                                )
                                                                                                |> RenderResult.Make
                                                                                            | Some timestampRender ->
                                                                                                (
                                                                                                    let __list = [
                                                                                                        ((timestampRender entry.AsOf) |> RenderResult.Make)
                                                                                                    ]
                                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                                )
                                                                                                |> RenderResult.Make
                                                                                        )
                                                                                    ]
                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                )
                                                                        ) |> RenderResult.Make
                                                                        (
                                                                            FRS.td
                                                                                []
                                                                                (
                                                                                    let __list = [
                                                                                        (
                                                                                            match (NonemptyString.ofString entry.By) with
                                                                                            | Some nonemptyId ->
                                                                                                (
                                                                                                    let __list = [
                                                                                                        ((props.User nonemptyId) |> RenderResult.Make)
                                                                                                    ]
                                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                                )
                                                                                                |> RenderResult.Make
                                                                                            | None ->
                                                                                                (
                                                                                                    let __list = [
                                                                                                        ((makeTextNode "System") |> RenderResult.Make)
                                                                                                    ]
                                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                                )
                                                                                                |> RenderResult.Make
                                                                                        )
                                                                                    ]
                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                )
                                                                        ) |> RenderResult.Make
                                                                        (
                                                                            FRS.td
                                                                                [(FRP.ClassName ("audit-operation-str"))]
                                                                                (
                                                                                    let __list = [
                                                                                        (fun (__sibI, __sibC, __pFQN) ->
                                                                                            let __parentFQN = Some "LibClient.Components.Pre"
                                                                                            (
                                                                                                (
                                                                                                    let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                    let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                    (LibClient.Components.TypeExtensions.TEs.Pre(
                                                                                                        ?xClassName = (if __currClass <> "" then Some __currClass else None),
                                                                                                        ?xStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                        children =
                                                                                                            (
                                                                                                                let __list = [
                                                                                                                    ((makeTextNode (System.String.Format("{0}", entry.OperationStr))) |> RenderResult.Make)
                                                                                                                ]
                                                                                                                __list |> RenderResult.ToRawElementsAsFragment __parentFQN
                                                                                                            )
                                                                                                    ))
                                                                                                )
                                                                                            )
                                                                                        ) |> RenderResult.Make
                                                                                    ]
                                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                                )
                                                                        ) |> RenderResult.Make
                                                                    ]
                                                                    __list |> RenderResult.ToRawElements __parentFQN
                                                                )
                                                        )
                                                        let __list = [
                                                            (
                                                                match (props.HeadersAndFields) with
                                                                | Default ->
                                                                    (
                                                                        let __list = [
                                                                            (( defaults()) |> RenderResult.Make)
                                                                        ]
                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                    )
                                                                    |> RenderResult.Make
                                                                | WithAdditional (_, additionalFields) ->
                                                                    (
                                                                        let __list = [
                                                                            (( defaults()) |> RenderResult.Make)
                                                                            (( additionalFields entry) |> RenderResult.Make)
                                                                        ]
                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                    )
                                                                    |> RenderResult.Make
                                                                | Custom (_, fields) ->
                                                                    (
                                                                        let __list = [
                                                                            (( fields entry) |> RenderResult.Make)
                                                                        ]
                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                    )
                                                                    |> RenderResult.Make
                                                            )
                                                        ]
                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                    )
                                            let __currClass = (nthChildClasses __sibI __sibC)
                                            let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            (LibUiAdmin.Components.TypeExtensions.TEs.Grid(
                                                input = (LibUiAdmin.Components.Grid.Paginated (estate.CurrentPage, makeRow)),
                                                headers =
                                                    FRH.fragment []
                                                        (
                                                            let defaults() = (
                                                                FRH.fragment []
                                                                    (
                                                                        let __list = [
                                                                            (
                                                                                FRS.td
                                                                                    []
                                                                                    (
                                                                                        let __list = [
                                                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                                                let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                                                                (
                                                                                                    (
                                                                                                        let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        (LibClient.Components.TypeExtensions.TEs.HeaderCell(
                                                                                                            label = ("When"),
                                                                                                            ?xClassName = (if __currClass <> "" then Some __currClass else None),
                                                                                                            ?xStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                        ))
                                                                                                    )
                                                                                                )
                                                                                            ) |> RenderResult.Make
                                                                                        ]
                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                    )
                                                                            ) |> RenderResult.Make
                                                                            (
                                                                                FRS.td
                                                                                    []
                                                                                    (
                                                                                        let __list = [
                                                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                                                let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                                                                (
                                                                                                    (
                                                                                                        let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        (LibClient.Components.TypeExtensions.TEs.HeaderCell(
                                                                                                            label = ("Who"),
                                                                                                            ?xClassName = (if __currClass <> "" then Some __currClass else None),
                                                                                                            ?xStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                        ))
                                                                                                    )
                                                                                                )
                                                                                            ) |> RenderResult.Make
                                                                                        ]
                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                    )
                                                                            ) |> RenderResult.Make
                                                                            (
                                                                                FRS.td
                                                                                    []
                                                                                    (
                                                                                        let __list = [
                                                                                            (fun (__sibI, __sibC, __pFQN) ->
                                                                                                let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                                                                (
                                                                                                    (
                                                                                                        let __currClass = (nthChildClasses __sibI __sibC)
                                                                                                        let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                        (LibClient.Components.TypeExtensions.TEs.HeaderCell(
                                                                                                            label = ("What"),
                                                                                                            ?xClassName = (if __currClass <> "" then Some __currClass else None),
                                                                                                            ?xStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                        ))
                                                                                                    )
                                                                                                )
                                                                                            ) |> RenderResult.Make
                                                                                        ]
                                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                                    )
                                                                            ) |> RenderResult.Make
                                                                        ]
                                                                        __list |> RenderResult.ToRawElements __parentFQN
                                                                    )
                                                            )
                                                            let __list = [
                                                                (
                                                                    match (props.HeadersAndFields) with
                                                                    | Default ->
                                                                        (
                                                                            let __list = [
                                                                                (( defaults()) |> RenderResult.Make)
                                                                            ]
                                                                            __list |> RenderResult.ToRawElements __parentFQN
                                                                        )
                                                                        |> RenderResult.Make
                                                                    | WithAdditional (additionalHeaders, _) ->
                                                                        (
                                                                            let __list = [
                                                                                (( defaults()) |> RenderResult.Make)
                                                                                ((additionalHeaders) |> RenderResult.Make)
                                                                            ]
                                                                            __list |> RenderResult.ToRawElements __parentFQN
                                                                        )
                                                                        |> RenderResult.Make
                                                                    | Custom (headers, _) ->
                                                                        (
                                                                            let __list = [
                                                                                ((headers) |> RenderResult.Make)
                                                                            ]
                                                                            __list |> RenderResult.ToRawElements __parentFQN
                                                                        )
                                                                        |> RenderResult.Make
                                                                )
                                                            ]
                                                            __list |> RenderResult.ToRawElements __parentFQN
                                                        ),
                                                ?xClassName = (if __currClass <> "" then Some __currClass else None),
                                                ?xStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                            ))
                                        )
                                    )
                                ) |> RenderResult.Make
                            ]
                            __list |> RenderResult.ToRawElementsAsFragment __parentFQN
                        )
                ))
            )
        )
    ) |> RenderResult.Make
    |> RenderResult.ToRawElementsSingle __parentFQN |> RenderResult.ToSingleElement
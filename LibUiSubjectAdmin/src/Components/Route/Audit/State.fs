[<AutoOpen>]
module LibUiSubjectAdmin.Components.Route_Audit_State

open Fable.React
open LibClient
open LibClient.Services.DateService
open ReactXP.Styles
open ReactXP.Components
open LibClient.Components
open LibRouter.Components
open LibUiSubjectAdmin.Components
open LibUiAdmin.Components

module dom = Fable.React.Standard

module UiSubjectAdmin =
    module Route =
        module Audit =
            type Render<'Subject> =
            | AsJson      of ('Subject -> string)
            | AsComponent of ('Subject -> ReactElement)

            type Buttons = {
                AuditLog:   Option<ReactEvent.Action -> unit>
                RunAction:  Option<ReactEvent.Action -> unit>
                Additional: list<ReactElement>
            }

open UiSubjectAdmin.Route.Audit

type UiSubjectAdmin.Route.Audit with
    static member State (
        service:    LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        render:     'Subject -> ReactElement,
        idString:   string,
        version:    uint64,
        ?timestamp: System.DateTimeOffset -> ReactElement
    ) : ReactElement =
        UiSubjectAdmin.Route.Audit.State (service, Render.AsComponent render, idString, version, ?timestamp = timestamp)

    static member State (
        service:    LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        render:     'Subject -> string,
        idString:   string,
        version:    uint64,
        ?timestamp: System.DateTimeOffset -> ReactElement
    ) : ReactElement =
        UiSubjectAdmin.Route.Audit.State (service, Render.AsJson render, idString, version, ?timestamp = timestamp)

    [<Component>]
    static member State (
        service:    LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>,
        render:     Render<'Subject>,
        idString:   string,
        version:    uint64,
        ?timestamp: System.DateTimeOffset -> ReactElement
    ) : ReactElement = element {
        let dataState = Hooks.useState AsyncData.Uninitialized

        Hooks.useEffect (
            (fun () ->
                async {
                    let! data = service.GetAuditSnapshot idString version

                    dataState.update data
                } |> startSafely
            ),
            [||]
        )

        LR.Route (scroll = ScrollView.Vertical, children = [|
            LC.ScrollView (
                scroll        = LibClient.Components.ScrollView.Scroll.Horizontal,
                restoreScroll = LibClient.Components.ScrollView.RestoreScroll.No,
                children = [|
                    LC.Section.Padded (styles = [|Styles.Content|], children = [|
                        RX.View [|
                            LC.AsyncData (
                                data = dataState.current,
                                whenAvailable = fun snapshot -> element {
                                    LC.Heading [|LC.Text $"{service.LifeCycleKey.LocalLifeCycleName} Subject State Snapshot"|]

                                    LC.Card (outerStyles = [|Styles.Card|], children = [|
                                        dom.table [(Props.ClassName "la-table-keyvalue")] [|
                                            dom.tbody [] [|
                                                UiAdmin.Table.KeyValue (
                                                    "Who",
                                                    match NonemptyString.ofString snapshot.By with
                                                    | None      -> "System"
                                                    | Some user -> user.Value
                                                )
                                                UiAdmin.Table.KeyValue (
                                                    "When",
                                                    match timestamp with
                                                    | Some timestamp -> timestamp snapshot.AsOf
                                                    | None           -> LC.Timestamp (UniDateTime.Of snapshot.AsOf)
                                                )
                                                UiAdmin.Table.KeyValue ("Version",  $"{snapshot.Version}")
                                            |]
                                        |]
                                    |])
                                    match render with
                                    | AsJson toString ->
                                        LC.Pre(text = toString snapshot.Subject)
                                    | AsComponent toComponent ->
                                        toComponent snapshot.Subject
                                }
                            )
                        |]
                    |])
                |]
            )
        |])
    }

and private Styles() =
    static member val Card = makeViewStyles {
        marginBottom 40
    }

    static member val Content = makeViewStyles {
        FlexDirection.Row
        Overflow.VisibleForScrolling
    }

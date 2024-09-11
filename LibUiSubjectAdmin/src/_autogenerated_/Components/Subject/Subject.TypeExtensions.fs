namespace LibUiSubjectAdmin.Components

open LibClient
open LibUiSubjectAdmin.Components.Subject
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module SubjectTypeExtensions =
    type LibUiSubjectAdmin.Components.Constructors.UiSubjectAdmin with
        static member Subject(data: 'Projection -> ReactElement, actions: 'Projection -> ReactElement, id: 'Id, service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Data = data
                    Actions = actions
                    Id = id
                    Service = service
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiSubjectAdmin.Components.Subject.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
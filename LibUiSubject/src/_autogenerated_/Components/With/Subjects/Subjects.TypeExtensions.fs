namespace LibUiSubject.Components

open LibClient
open Fable.React
open LibClient.Components
open LibUiSubject
open LibClient.Components.Subscribe
open LibUiSubject.Components.With.Subjects
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_SubjectsTypeExtensions =
    type LibUiSubject.Components.Constructors.UiSubject.With with
        static member Subjects(service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, by: By<'Subject, 'Projection, 'Id, 'Index, 'Action, 'Event, 'OpError>, ``with``: WithSubjects<'Id, 'Projection>, ?children: ReactChildrenProp, ?useCache: UseCache, ?onByChange: LibClient.Components.Subscribe.OnSubscriptionKeyChange, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Service = service
                    By = by
                    With = ``with``
                    UseCache = defaultArg useCache (UseCache.IfReasonablyFresh)
                    OnByChange = defaultArg onByChange (LibClient.Components.Subscribe.OnSubscriptionKeyChange.ShowCurrentDataAsFetching)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiSubject.Components.With.Subjects.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
namespace LibUiSubject.Components

open LibClient
open Fable.React
open LibClient.Components
open LibUiSubject
open LibUiSubject.Components.With.Subject
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_SubjectTypeExtensions =
    type LibUiSubject.Components.Constructors.UiSubject.With with
        static member Subject(service: LibUiSubject.Services.SubjectService.ISubjectService<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, id: 'Id, ``with``: WithSubject<'Projection>, ?children: ReactChildrenProp, ?useCache: UseCache, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Service = service
                    Id = id
                    With = ``with``
                    UseCache = defaultArg useCache (UseCache.IfReasonablyFresh)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiSubject.Components.With.Subject.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
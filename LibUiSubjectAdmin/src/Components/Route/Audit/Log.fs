[<AutoOpen>]
module LibUiSubjectAdmin.Components.Route_Audit_Log

open Fable.React
open LibClient
open ReactXP.Components
open LibClient.Components
open LibRouter.Components
open LibUiSubjectAdmin.Components

type UiSubjectAdmin.Route.Audit with
    [<Component>]
    static member Log (
        backendUrl:                  string,
        subjectId:                   LibUiSubjectAdmin.Components.Audit.Types.AuditSubjectId,
        user:                        NonemptyString -> ReactElement,
        ?timestamp:                  (System.DateTimeOffset -> ReactElement),
        ?additionalHeadersAndFields: ReactElement * (UntypedSubjectAuditData -> ReactElement)
    ) : ReactElement =
        LR.Route (scroll = ScrollView.Vertical, children = [|
            LC.Section.Padded (elements {
                LC.Heading [|LC.Text $"{subjectId.Name} Subject Audit Log"|]

                UiSubjectAdmin.AuditLog.Untyped (
                    baseUrl                     = backendUrl,
                    auditSubjectId              = subjectId,
                    user                        = user,
                    ?timestamp                  = timestamp,
                    ?additionalHeadersAndFields = additionalHeadersAndFields
                )
            })
        |])

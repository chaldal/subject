[<AutoOpen>]
module LibUiSubjectAdmin.Components.Audit_Typed

open Fable.React
open System
open LibClient
open LibUiSubjectAdmin.Components
open LibClient.Services.HttpService.ThothEncodedHttpService

type EndpointParams = {
    BaseUrl:     string
    SubjectName: string
    IdString:    string
    PageSize:    PositiveInteger
    Offset:      UnsignedInteger
}

let inline makeEndpoint(): ApiEndpoint<EndpointParams, unit, List<SubjectAuditData<'Action, 'Constructor>>> =
    makeEndpoint
        LibClient.Services.HttpService.Types.HttpAction.Get
        (fun p -> $"{p.BaseUrl}/api/v1/subject/{p.SubjectName}/auditTyped/{Uri.EscapeDataString(p.IdString)}?pageSize={p.PageSize.Value}&offset={p.Offset.Value}")
        id

type UiSubjectAdmin.AuditLog with
    [<Component>]
    static member Typed<'Action, 'Constructor when 'Action :> LifeAction and 'Constructor :> Constructor> (
        endpoint: ApiEndpoint<EndpointParams, unit, List<SubjectAuditData<'Action, 'Constructor>>>,
        baseUrl: string,
        auditSubjectId: LibUiSubjectAdmin.Components.Audit.Types.AuditSubjectId,
        headersAndFields: ReactElement * (SubjectAuditData<'Action, 'Constructor> -> ReactElement),
        ?handheldRow: (SubjectAuditData<'Action, 'Constructor> -> ReactElement),
        ?filter: (SubjectAuditData<'Action, 'Constructor> -> bool)
    ) : ReactElement =
        UiSubjectAdmin.Audit.Generic (
            endpoint             = endpoint,
            pageToEndpointParams = (fun page ->
                {
                    BaseUrl     = baseUrl
                    SubjectName = auditSubjectId.Name
                    IdString    = auditSubjectId.IdString
                    PageSize    = page.Size
                    Offset      = ((page.Number.Value - 1) * page.Size.Value) |> UnsignedInteger.ofInt |> Option.getOrElse UnsignedInteger.Zero
                }
            ),
            auditSubjectId   = auditSubjectId,
            headersAndFields = headersAndFields,
            ?handheldRow     = handheldRow,
            ?filter          = filter
        )


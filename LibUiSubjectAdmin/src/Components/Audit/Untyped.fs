[<AutoOpen>]
module LibUiSubjectAdmin.Components.Audit_Untyped

open Fable.React
open System
open LibClient
open LibClient.Components
open LibClient.Services.DateService
open LibUiSubjectAdmin.Components
open LibClient.Services.HttpService.ThothEncodedHttpService

module dom = Fable.React.Standard

type EndpointParams = {
    BaseUrl:     string
    SubjectName: string
    IdString:    string
    PageSize:    PositiveInteger
    Offset:      UnsignedInteger
}

let private endpoint (maybeEcosystem: Option<string>): ApiEndpoint<EndpointParams, unit, List<UntypedSubjectAuditData>> =
    makeEndpoint
        LibClient.Services.HttpService.Types.HttpAction.Get
        (fun p ->
            $"{p.BaseUrl}/api/v1"
            +
            match maybeEcosystem with
            | None           -> ""
            | Some ecosystem -> $"/ecosystem/{ecosystem}"
            +
            $"/subject/{p.SubjectName}/audit/{Uri.EscapeDataString(p.IdString)}?pageSize={p.PageSize.Value}&offset={p.Offset.Value}"
        )
        id

type UiSubjectAdmin.AuditLog with
    [<Component>]
    static member Untyped (
        baseUrl:                     string,
        auditSubjectId:              LibUiSubjectAdmin.Components.Audit.Types.AuditSubjectId,
        user:                        NonemptyString -> ReactElement,
        ?ecosystem:                  string,
        ?timestamp:                  (System.DateTimeOffset -> ReactElement),
        ?filter:                     (UntypedSubjectAuditData -> bool),
        ?additionalHeadersAndFields: ReactElement * (UntypedSubjectAuditData -> ReactElement)
    ) : ReactElement =
        let headers =
            asFragment [
                dom.td [] [LC.HeaderCell ("When")]
                dom.td [] [LC.HeaderCell ("Who")]
                dom.td [] [LC.HeaderCell ("What")]
                match additionalHeadersAndFields with
                | None -> ()
                | Some (additionalHeaders, _) -> additionalHeaders
            ]

        let fields = fun entry ->
            asFragment [
                dom.td [] [
                    match timestamp with
                    | Some timestamp -> timestamp entry.AsOf
                    | None           -> LC.Timestamp (UniDateTime.Of entry.AsOf)
                ]
                dom.td [] [
                    match NonemptyString.ofString entry.By with
                    | None    -> LC.Text "System"
                    | Some by -> user by
                ]
                dom.td [] [
                    LC.Pre (text = entry.OperationStr)
                ]
                match additionalHeadersAndFields with
                | None -> ()
                | Some (_, additionalFields) -> additionalFields entry
            ]

        UiSubjectAdmin.Audit.Generic (
            endpoint             = endpoint ecosystem,
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
            headersAndFields = (headers, fields),
            ?filter = filter
        )

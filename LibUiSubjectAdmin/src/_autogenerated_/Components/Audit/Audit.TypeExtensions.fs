namespace LibUiSubjectAdmin.Components

open System
open LibClient
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibUiSubjectAdmin.Components.Audit

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module AuditTypeExtensions =
    type LibUiSubjectAdmin.Components.TypeExtensions.TEs with
        static member Audit(auditSubjectId: AuditSubjectId, thothEncodedHttpService: ThothEncodedHttpService, baseUrl: string, user: NonemptyString -> ReactElement, ?headersAndFields: HeadersAndFields, ?timestamp: (System.DateTimeOffset -> ReactElement), ?filter: (UntypedSubjectAuditData -> bool), ?key: string, ?xStyles: List<ReactXP.Styles.RuntimeStyles>, ?xClassName: string) =
            let __currExplicitProps =
                {
                    AuditSubjectId = auditSubjectId
                    ThothEncodedHttpService = thothEncodedHttpService
                    BaseUrl = baseUrl
                    User = user
                    HeadersAndFields = defaultArg headersAndFields (HeadersAndFields.Default)
                    Timestamp = timestamp |> Option.orElse (None)
                    Filter = filter |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            LibUiSubjectAdmin.Components.Audit.Make
                (
                    let __implicitProps = [
                        match xStyles with
                        | Option.None | Option.Some [] -> ()
                        | Option.Some styles -> ("__style", styles :> obj)
                        match xClassName with
                        | Option.None -> ()
                        | Option.Some value -> ("ClassName", value :> obj)
                    ]
                    (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
                )
                []
        static member MakeAuditProps(pAuditSubjectId: AuditSubjectId, pThothEncodedHttpService: ThothEncodedHttpService, pBaseUrl: string, pUser: NonemptyString -> ReactElement, ?pHeadersAndFields: HeadersAndFields, ?pTimestamp: (System.DateTimeOffset -> ReactElement), ?pFilter: (UntypedSubjectAuditData -> bool), ?pkey: string) =
            {
                AuditSubjectId = pAuditSubjectId
                ThothEncodedHttpService = pThothEncodedHttpService
                BaseUrl = pBaseUrl
                User = pUser
                HeadersAndFields = defaultArg pHeadersAndFields (HeadersAndFields.Default)
                Timestamp = pTimestamp |> Option.orElse (None)
                Filter = pFilter |> Option.orElse (None)
                key = pkey |> Option.orElse (JsUndefined)
            }
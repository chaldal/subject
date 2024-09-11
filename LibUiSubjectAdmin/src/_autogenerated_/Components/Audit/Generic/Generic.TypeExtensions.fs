namespace LibUiSubjectAdmin.Components

open LibClient
open System
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibUiSubjectAdmin.Components.Audit.Types
open LibUiSubjectAdmin.Components.Audit.Generic
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Audit_GenericTypeExtensions =
    type LibUiSubjectAdmin.Components.Constructors.UiSubjectAdmin.Audit with
        static member Generic(endpoint: ApiEndpoint<'EndpointParams, unit, List<'Entry>>, pageToEndpointParams: {| Size: PositiveInteger; Number: PositiveInteger |} -> 'EndpointParams, auditSubjectId: AuditSubjectId, headersAndFields: ReactElement * ('Entry -> ReactElement), ?children: ReactChildrenProp, ?handheldRow: ('Entry -> ReactElement), ?filter: ('Entry -> bool), ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Endpoint = endpoint
                    PageToEndpointParams = pageToEndpointParams
                    AuditSubjectId = auditSubjectId
                    HeadersAndFields = headersAndFields
                    HandheldRow = handheldRow |> Option.orElse (None)
                    Filter = filter |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiSubjectAdmin.Components.Audit.Generic.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
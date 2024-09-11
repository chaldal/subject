module LibUiSubjectAdmin.Components.Audit.Generic

open System
open LibClient
open LibClient.Services.HttpService.ThothEncodedHttpService
open LibUiSubjectAdmin.Components.Audit.Types


type Props<'EndpointParams, 'Entry> = (* GenerateMakeFunction *) {
    Endpoint:             ApiEndpoint<'EndpointParams, unit, List<'Entry>>
    PageToEndpointParams: {| Size: PositiveInteger; Number: PositiveInteger |} -> 'EndpointParams
    AuditSubjectId:       AuditSubjectId
    HeadersAndFields:     ReactElement * ('Entry -> ReactElement)
    HandheldRow:          Option<('Entry -> ReactElement)> // defaultWithAutoWrap None
    Filter:               ('Entry -> bool) option       // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'EndpointParams, 'Entry> = {
    CurrentPage: LibUiAdmin.Components.Grid.PaginatedGridData<'Entry>
}



type Generic<'EndpointParams, 'Entry>(_initialProps) =
    inherit EstatefulComponent<Props<'EndpointParams, 'Entry>, Estate<'EndpointParams, 'Entry>, Actions<'EndpointParams, 'Entry>, Generic<'EndpointParams, 'Entry>>("LibUiSubjectAdmin.Components.Audit.Generic", _initialProps, Actions, hasStyles = true)

    override this.GetInitialEstate (_initialProps: Props<'EndpointParams, 'Entry>) : Estate<'EndpointParams, 'Entry> = {
        CurrentPage = {
            PageNumber          = PositiveInteger.One
            PageSize            = PositiveInteger.ofLiteral 10
            MaybePageCount      = None
            Items               = AsyncData.WillStartFetchingSoonHack
            GoToPage            = this.GoToPage
            MaybeTotalItemCount = None
        }
    }

    override this.ComponentDidMount () : unit =
        this.GoToPage this.state.CurrentPage.PageSize this.state.CurrentPage.PageNumber None

    member private this.GoToPage (pageSize: PositiveInteger) (pageNumber: PositiveInteger) (_e: Option<ReactEvent.Action>) : unit =
        async {
            let parameters = this.props.PageToEndpointParams {| Size = pageSize; Number = pageNumber |}

            let! data = LibClient.ServiceInstances.services().ThothEncodedHttp.Request this.props.Endpoint parameters ()
            let itemsAD =
                match data with
                | Ok entries   -> AsyncData.Available (Seq.ofList entries)
                | Error requestError ->
                    match requestError with
                    | RequestError.DecodingError (message, _, _) -> AsyncData.Failed (UnknownFailure message)
                    | RequestError.Non200Code (0,         _)     -> AsyncData.Failed NetworkFailure
                    | RequestError.Non200Code (errorCode, _)     -> AsyncData.Failed (UnknownFailure $"Error {errorCode}")
                |>
                    match this.props.Filter with
                    | None -> identity
                    | Some filter -> AsyncData.map (Seq.filter filter)

            this.SetEstate (fun estate _ ->
                { estate with
                    CurrentPage =
                        { estate.CurrentPage with
                            PageNumber = pageNumber
                            PageSize   = pageSize
                            Items      = itemsAD
                        }
                }
            )
        } |> startSafely

and Actions<'EndpointParams, 'Entry>(_this: Generic<'EndpointParams, 'Entry>) =
    class end

let Make = makeConstructor<Generic<'EndpointParams, 'Entry>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

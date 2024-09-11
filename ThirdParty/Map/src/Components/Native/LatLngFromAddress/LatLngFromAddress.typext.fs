module ThirdParty.Map.Components.Native.LatLngFromAddress

open LibClient
open LibClient.Services.Subscription
open ThirdParty.Map.Types
open Fable.Core
open Fable.Core.JsInterop
open LibClient.Services.HttpService.HttpService
open LibClient.Services.HttpService.Types

type Props = (* GenerateMakeFunction *) {
    Address: string
    With:    AsyncData<LatLng> -> ReactElement
    ApiKey:  string
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    LatLng: AsyncData<LatLng>
}

let subscriptionImplementationValue : AdHocSubscriptionImplementation<AsyncData<LatLng>> = AdHocSubscriptionImplementation<AsyncData<LatLng>>(None, None)

type IResponse =
    abstract candidates: obj [] with get, set
    abstract status: string with get, set

type LatLngFromAddress(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, LatLngFromAddress>("ThirdParty.Map.Components.Native.LatLngFromAddress", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_: Props) : Estate = {
        LatLng = WillStartFetchingSoonHack
    }

    override this.ComponentDidMount () : unit =
        #if EGGSHELL_PLATFORM_IS_WEB
        failwith "Shouldn't be trying to run this on web"
        #else
        async {
            subscriptionImplementationValue.Update WillStartFetchingSoonHack

            let placesEndpoint = {
                Action  = Get
                Url     = fun () -> "https://maps.googleapis.com/maps/api/place/findplacefromtext/json?fields=geometry&input=" + this.props.Address + "&inputtype=textquery&key=" + this.props.ApiKey
                Payload = NoPayload
                Result  = mapHttpResult<IResponse>
            }

            let! response = LibClient.ServiceInstances.services().Http.Request placesEndpoint () () |> Async.TryCatch

            match response with
            | Ok results when results.status = "OK" ->
                let result = results.candidates.[0]
                let latLng = (result?geometry?location?lat, result?geometry?location?lng)
                subscriptionImplementationValue.Update (Available latLng)

            | _ -> subscriptionImplementationValue.Update Unavailable

        } |> startSafely
        #endif

and Actions(_this: LatLngFromAddress) =
    class end

let Make = makeConstructor<LatLngFromAddress, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

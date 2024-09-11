module ThirdParty.Map.Components.Web.LatLngFromAddress

open LibClient
open LibClient.Services.Subscription
open ThirdParty.Map.Types
open Fable.Core.JsInterop

type private LoaderOptions = {
    apiKey:    string
    version:   string
    libraries: array<string>
}

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

type LatLngFromAddress(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, LatLngFromAddress>("ThirdParty.Map.Components.Web.LatLngFromAddress", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_: Props) : Estate = {
        LatLng = WillStartFetchingSoonHack
    }

and Actions(this: LatLngFromAddress) =
    member _.OnMapAnchorLoaded (div: Browser.Types.Element) : unit =
        #if EGGSHELL_PLATFORM_IS_WEB
            promise {
                // TODO Pull out this loader from the component and make it app specific (i.e. load this once in an app)
                let loader =
                    createNew
                        (Fable.Core.JsInterop.import "Loader" "@googlemaps/js-api-loader")
                        {
                            apiKey    = this.props.ApiKey
                            version   = "weekly"
                            // The libraries loaded here must match those loaded within the map component itself, otherwise the component may be
                            // loaded twice with different libraries, which will cause an error.
                            libraries = [| "places"; "drawing" |]
                        }
                let! google = loader?load ()
                let placesService = createNew google?maps?places?PlacesService div

                let placeRequest = createObj [
                    "query"  ==> this.props.Address
                    "fields" ==> [| "geometry" |]
                ]

                subscriptionImplementationValue.Update WillStartFetchingSoonHack

                placesService?findPlaceFromQuery(placeRequest, fun maybeResults ->
                    match isNull maybeResults with
                    | true -> subscriptionImplementationValue.Update Unavailable
                    | false ->
                        let results : array<obj> = maybeResults
                        let result = results.[0]
                        let latLng = (result?geometry?location?lat (), result?geometry?location?lng ())
                        subscriptionImplementationValue.Update (Available latLng)
                )
            } |> ignore
        #else
        (this, div) |> ignore
        failwith "Shouldn't be trying to run this on native"
        #endif


let Make = makeConstructor<LatLngFromAddress, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

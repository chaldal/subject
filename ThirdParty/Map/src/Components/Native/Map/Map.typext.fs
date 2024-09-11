module ThirdParty.Map.Components.Native.Map

open LibClient
open ReactXP.Styles
open ThirdParty.Map.Types
open LibClient.Services.ImageService
open Fable.Core.JsInterop
open LibClient.LocalImages

let defaultLatLng = (23.793932, 90.411814)

let getCoordinate (marker: Marker) : LatLng =
    match marker.Position with
    | MarkerPosition.LatLng latlng -> latlng
    | MarkerPosition.Centered      -> defaultLatLng

let getMarkerImage (marker: Marker) : ImageSource =
    match marker.Image with
    | Some markerImage ->
        match markerImage with
        | MarkerImage.Icon iconMarker     -> localImage iconMarker.Url
        | MarkerImage.Symbol symbolMarker -> localImage symbolMarker.Path
    | None -> localImage "/libs/ThirdParty/Map/images/marker.png"

type Props = (* GenerateMakeFunction *) {
    ApiKey:     string
    Value:      Option<LatLng>
    Zoom:       Option<int>
    OnChange:   Option<LatLng> -> unit
    FullScreen: bool
    Markers:    Option<List<Marker>>
    Shapes:     Option<List<Shape>>
    Ref:        IRefReactNativeMapView -> unit
    key:        string option // defaultWithAutoWrap JsUndefined
}

type Map(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Map>("ThirdParty.Map.Components.Native.Map", _initialProps, Actions, hasStyles = true)

    override this.ComponentWillReceiveProps (nextProps: Props) : unit =
        if nextProps.Value <> this.props.Value then
            nextProps.OnChange nextProps.Value

and Actions(_this: Map) =
    class end

let Make = makeConstructor<Map, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
type Estate = NoEstate
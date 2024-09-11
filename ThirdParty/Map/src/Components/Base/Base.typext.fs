module ThirdParty.Map.Components.Base

open LibClient
open ReactXP.Styles

type Value  = ThirdParty.Map.Types.Value
module Value =
    let Address = ThirdParty.Map.Types.Value.Address
    let LatLng  = ThirdParty.Map.Types.Value.LatLng

type LatLng = ThirdParty.Map.Types.LatLng
let dhakaLatLng = ThirdParty.Map.Types.dhakaLatLng

type MapPosition = ThirdParty.Map.Types.MapPosition

type Marker = ThirdParty.Map.Types.Marker

type MarkerPosition = ThirdParty.Map.Types.MarkerPosition

type MarkerImage = ThirdParty.Map.Types.MarkerImage
let Icon = MarkerImage.Icon
let Symbol = MarkerImage.Symbol

#if EGGSHELL_PLATFORM_IS_WEB
type InfoWindow = ThirdParty.Map.Types.InfoWindow
#endif

type Shape = ThirdParty.Map.Types.Shape
let Polyline = Shape.Polyline
let Polygon = Shape.Polygon
let Circle = Shape.Circle

type Directions = ThirdParty.Map.Types.Directions
type DirectionsRendererOptions = ThirdParty.Map.Types.DirectionsRendererOptions
type MapStyle = ThirdParty.Map.Types.MapStyle

type Place = ThirdParty.Map.Types.Place
let PlaceByQuery = Place.Query
let PlaceById = Place.Id
let PlaceByLatLng = Place.LatLng

type TravelMode = ThirdParty.Map.Types.TravelMode
let Driving = TravelMode.Driving
let Walking = TravelMode.Walking
let Bicycling = TravelMode.Bicycling
let Transit = TravelMode.Transit

type IRefReactNativeMapView = ThirdParty.Map.Types.IRefReactNativeMapView

type IWebMapViewRef = ThirdParty.Map.Types.IWebMapViewRef

type LocateToConfig = ThirdParty.Map.Types.LocateToConfig

type MapTypeId = ThirdParty.Map.Types.MapTypeId

type Props = (* GenerateMakeFunction *) {
    ApiKey:            string
    Position:          MapPosition                    // default MapPosition.Auto
    OnPositionChanged: Option<MapPosition -> unit>    // defaultWithAutoWrap JsUndefined
    Zoom:              Option<int>                    // defaultWithAutoWrap JsUndefined
    Markers:           Option<List<Marker>>           // defaultWithAutoWrap JsUndefined
    Shapes:            Option<List<Shape>>            // defaultWithAutoWrap JsUndefined
    FullScreen:        Option<bool>                   // defaultWithAutoWrap JsUndefined

    // These end up being passed in via MapOptions (see https://developers.google.com/maps/documentation/javascript/reference/map#MapOptions)
    BackgroundColor:   Option<string>                 // defaultWithAutoWrap JsUndefined
    ClickableIcons:    Option<bool>                   // defaultWithAutoWrap JsUndefined
    DisableDefaultUI:  Option<bool>                   // defaultWithAutoWrap JsUndefined
    MinZoom:           Option<float>                  // defaultWithAutoWrap JsUndefined
    MaxZoom:           Option<float>                  // defaultWithAutoWrap JsUndefined
    MapStyles:         Option<List<MapStyle>>         // defaultWithAutoWrap JsUndefined
    MapTypeId:         Option<MapTypeId>              // defaultWithAutoWrap JsUndefined

    // WARNING: Changes to the Directions can result in calls to the directions service, which incurs an API cost.
    //          Typically, you will want to change the value infrequently. As long as the value remains stable,
    //          or an empty list, no additional API call will be made.
    Directions: Option<List<Directions>>       // defaultWithAutoWrap JsUndefined

    OnLocatePress: Option<ReactEvent.Action -> LocateToConfig> // defaultWithAutoWrap None

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Base(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Base>("ThirdParty.Map.Components.Base", _initialProps, Actions, hasStyles = true)

    let mutable _maybeWebRef: Option<IWebMapViewRef> = None

    let mutable _maybeNativeRef: Option<IRefReactNativeMapView> = None

    member _.MaybeWebRef
        with get () = _maybeWebRef
        and set (maybeRef: Option<IWebMapViewRef>) =
            _maybeWebRef <- maybeRef

    member _.MaybeNativeRef
        with get () = _maybeNativeRef
        and set (maybeRef: Option<IRefReactNativeMapView>) =
            _maybeNativeRef <- maybeRef

    member  _.AnimateToCenter (config: LocateToConfig) =
        #if EGGSHELL_PLATFORM_IS_WEB
        _maybeWebRef
        |> Option.sideEffect (fun webRef ->
            webRef.panTo (config.location)
            webRef.setZoom (config.zoom)
        )
        #else
        _maybeNativeRef
        |> Option.sideEffect (fun nativeRef ->
            nativeRef.animateCamera ({
                zoom = config.zoom
                center = (config.location |> ThirdParty.Map.Types.LatLng.asNativeMapViewCoordinates)
            })
        )
        #endif


and Actions(this: Base) =
    member _.NativeRef (nativeRef: IRefReactNativeMapView) =
        this.MaybeNativeRef <- Some nativeRef

    member _.WebRef (webRef: IWebMapViewRef) =
        this.MaybeWebRef <- Some webRef

    member _.OnLocatePress (e: ReactEvent.Action) =
        this.props.OnLocatePress
        |> Option.sideEffect (fun onLocatePress ->
            onLocatePress e
            |> this.AnimateToCenter
        )

let Make = makeConstructor<Base, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
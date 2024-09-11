[<AutoOpen>]
module ThirdParty.Map.Types

open System
open Fable.Core
open Fable.React

// Used to mark types that contain keys, so that lists of that type can be compared by key rather than structurally.
[<Mangle>]
type internal IKeyed =
    abstract member Key: string

// Used to mark certain fields as ignored during comparison/equality checks, since they break the structural equality semantics that are relied upon
// to make deep comparisons between old props and new.
[<CustomEquality; CustomComparison>]
type IgnoredDuringComparison<'T> = IgnoredDuringComparison of 'T
with
    member internal this.Value =
        let (IgnoredDuringComparison value) = this
        value

    interface IComparable<IgnoredDuringComparison<'T>> with
        member _.CompareTo _ =
            // Always the same, since they're the same type.
            0

    interface IComparable with
        member this.CompareTo other =
            match other with
            | :? IgnoredDuringComparison<'T> as other ->
                (this :> IComparable<IgnoredDuringComparison<'T>>).CompareTo(other)
            | _ ->
                -1

    interface IEquatable<IgnoredDuringComparison<'T>> with
        member this.Equals other =
            (this :> IComparable<IgnoredDuringComparison<'T>>).CompareTo(other) = 0

    override this.Equals other =
        match other with
        | :? IgnoredDuringComparison<'T> as other -> (this :> IEquatable<IgnoredDuringComparison<'T>>).Equals(other)
        | _ -> false

    override _.GetHashCode(): int =
        0

type LatLng = float * float (* lat * lng *)
type Point = int * int

// Type declaration of types being used from MapView
// https://github.com/react-native-maps/react-native-maps/blob/master/docs/mapview.md#types
type NativeMapViewCoordinates = {
    latitude: float
    longitude: float
}

type AnimateCameraConfig (* Type in docs -> Camera *) = {
    zoom:   int
    center: NativeMapViewCoordinates
}

// Partial type of MapView Ref, only declaring what is currently being used
// https://github.com/react-native-maps/react-native-maps/blob/master/docs/mapview.md#methods
type IRefReactNativeMapView =
    abstract member animateCamera: AnimateCameraConfig -> unit

[<AbstractClass>]
type IWebMapViewRef (map: obj) =
    member private this.mapObj = map
    abstract member panTo:   LatLng -> unit
    abstract member setZoom: int -> unit

type LocateToConfig = {
    zoom:     int
    location: LatLng
}

module LatLng =
    let asDecimal (ll: LatLng) : decimal * decimal =
        (ll |> fst |> decimal, ll |> snd |> decimal)

    let ofDecimal (ll: decimal * decimal) : LatLng =
        (ll |> fst |> float, ll |> snd |> float)

    let asNativeMapViewCoordinates (ll: LatLng) : NativeMapViewCoordinates =
        {
            latitude =  ll |> fst
            longitude = ll |> snd
        }


// Redo this nonsense — the value that's consumed as Value needs to be
// the same Value as comes through the OnChange handler. What we have with
// this Address business in here is crappy modelling of the idea of "initial value"
[<RequireQualifiedAccess>]
type Value =
| Address of string
| LatLng  of LatLng

// TODO get rid of this thing, it has no bloody business here
let dhakaLatLng : LatLng = (23.793932, 90.411814)

[<RequireQualifiedAccess>]
type MapPosition =
| LatLng of LatLng
// Automatically zoom and pan the map based on initial markers.
| Auto

// https://developers.google.com/maps/documentation/javascript/reference/coordinates#Size
type Size = {
    Width: float
    Height: float
}

[<RequireQualifiedAccess>]
type PixelOrPercentage =
| Pixel of int
| Percentage of float

// https://developers.google.com/maps/documentation/javascript/reference/marker#Icon
type Icon = {
    Url: string
    Anchor: Option<Point>
    LabelOrigin: Option<Point>
    Origin: Option<Point>
    ScaledSize: Option<Size>
    Size: Option<Size>
}

// https://developers.google.com/maps/documentation/javascript/reference/marker#Symbol
type Symbol = {
    Path: string
    Anchor: Option<Point>
    FillColor: Option<string>
    FillOpacity: Option<float>
    LabelOrigin: Option<Point>
    Rotation: Option<float>
    Scale: Option<float>
    StrokeColor: Option<string>
    StrokeOpacity: Option<float>
    StrokeWeight: Option<float>
}

// https://developers.google.com/maps/documentation/javascript/reference/polygon#IconSequence
type IconSequence = {
    Icon: Symbol
    FixedRotation: bool
    Offset: Option<PixelOrPercentage>
    Repeat: Option<PixelOrPercentage>
}

type InfoWindowHandle =
    abstract member Focus : unit -> unit
    abstract member Close : unit -> unit

// https://developers.google.com/maps/documentation/javascript/reference/info-window#InfoWindow
type InfoWindow = {
    Content: IgnoredDuringComparison<InfoWindowHandle -> ReactElement>
    ShouldFocus: bool
    DisableAutoPan: bool
    GetDisplayLocation: Option<IgnoredDuringComparison<Option<LatLng> -> LatLng>>
    IsOpen: bool
    MinWidth: Option<int>
    MaxWidth: Option<int>
    PixelOffset: Option<Size>
    ZIndex: Option<int>
}

[<RequireQualifiedAccess>]
type MarkerImage =
| Icon of Icon
| Symbol of Symbol

[<RequireQualifiedAccess>]
type MarkerPosition =
| LatLng of LatLng
// This is a convenience that automatically centers the marker and keeps it centered if the map is moved.
| Centered

// https://developers.google.com/maps/documentation/javascript/reference/marker#Animation
[<RequireQualifiedAccess>]
type MarkerAnimation =
| Bounce
| Drop

// https://developers.google.com/maps/documentation/javascript/reference/marker#MarkerLabel
type MarkerLabel = {
    Text: string
    ClassName: Option<string>
    Color: Option<string>
    FontFamily: Option<string>
    FontSize: Option<string>
    FontWeight: Option<string>
}

// https://developers.google.com/maps/documentation/javascript/reference/marker#MarkerOptions
type Marker = {
    Key: string
    Position: MarkerPosition
    Draggable: bool
    Label: Option<MarkerLabel>
    Tooltip: Option<string>
    Image: Option<MarkerImage>
    Opacity: Option<float>
    ZIndex: Option<int>
    Animation: Option<MarkerAnimation>
    InfoWindow: Option<InfoWindow>
    OnClick: Option<IgnoredDuringComparison<unit -> unit>>
}
with
    interface IKeyed with
        member this.Key = this.Key

    member this.anchor () =
        this.Image
        |> Option.map (fun img ->
            match img with
            | MarkerImage.Icon icon -> icon.Anchor
            | MarkerImage.Symbol symbol -> symbol.Anchor
        )
        |> Option.flatten

// https://developers.google.com/maps/documentation/javascript/reference/polygon#PolylineOptions
type Polyline = {
    Key: string
    Path: LatLng[]
    Draggable: bool
    Editable: bool
    Geodesic: bool
    Visible: bool
    Icons: Option<IconSequence[]>
    StrokeColor: Option<string>
    StrokeOpacity: Option<float>
    StrokeWeight: Option<float>
    ZIndex: Option<int>
    InfoWindow: Option<InfoWindow>
    OnClick: Option<IgnoredDuringComparison<unit -> unit>>
}
with
    interface IKeyed with
        member this.Key = this.Key

// https://developers.google.com/maps/documentation/javascript/reference/polygon#StrokePosition
[<RequireQualifiedAccess>]
type StrokePosition =
| Center
| Inside
| Outside

// https://developers.google.com/maps/documentation/javascript/reference/polygon#PolygonOptions
type Polygon = {
    Key: string
    Paths: LatLng[][]
    Draggable: bool
    Editable: bool
    Geodesic: bool
    Visible: bool
    FillColor: Option<string>
    FillOpacity: Option<float>
    StrokeColor: Option<string>
    StrokeOpacity: Option<float>
    StrokePosition: Option<StrokePosition>
    StrokeWeight: Option<float>
    ZIndex: Option<int>
    InfoWindow: Option<InfoWindow>
    OnClick: Option<IgnoredDuringComparison<unit -> unit>>
}
with
    interface IKeyed with
        member this.Key = this.Key

// https://developers.google.com/maps/documentation/javascript/reference/polygon#CircleOptions
type Circle = {
    Key: string
    Center: LatLng
    Radius: float
    Draggable: bool
    Editable: bool
    Visible: bool
    FillColor: Option<string>
    FillOpacity: Option<float>
    StrokeColor: Option<string>
    StrokeOpacity: Option<float>
    StrokePosition: Option<StrokePosition>
    StrokeWeight: Option<float>
    ZIndex: Option<int>
    InfoWindow: Option<InfoWindow>
    OnClick: Option<IgnoredDuringComparison<unit -> unit>>
}
with
    interface IKeyed with
        member this.Key = this.Key

[<RequireQualifiedAccess>]
type Shape =
| Polyline of Polyline
| Polygon of Polygon
| Circle of Circle
with
    member this.Key =
        match this with
        | Shape.Polyline polyline -> polyline.Key
        | Shape.Polygon polygon -> polygon.Key
        | Shape.Circle circle -> circle.Key

    member this.InfoWindow =
        match this with
        | Shape.Polyline polyline -> polyline.InfoWindow
        | Shape.Polygon polygon -> polygon.InfoWindow
        | Shape.Circle circle -> circle.InfoWindow

    member this.OnClick =
        match this with
        | Shape.Polyline polyline -> polyline.OnClick
        | Shape.Polygon polygon -> polygon.OnClick
        | Shape.Circle circle -> circle.OnClick

    interface IKeyed with
        member this.Key = this.Key

// https://developers.google.com/maps/documentation/javascript/reference/directions#Place
[<RequireQualifiedAccess>]
type Place =
| Id of string
| LatLng of LatLng
| Query of string

// https://developers.google.com/maps/documentation/javascript/reference/directions#TravelMode
[<RequireQualifiedAccess>]
type TravelMode =
| Driving
| Walking
| Bicycling
| Transit

// https://developers.google.com/maps/documentation/javascript/reference/directions#DirectionsWaypoint
type Waypoint = {
    Place: Place
    IsStopover: bool
}

[<RequireQualifiedAccess>]
type DirectionsPolylineRenderer =
| Polyline of Polyline
| Callback of IgnoredDuringComparison<((* LegIndex *) int -> seq<LatLng> -> Polyline)>

// https://developers.google.com/maps/documentation/javascript/reference/directions#DirectionsRendererOptions
type DirectionsRendererOptions = {
    Draggable: bool
    HideRouteList: bool
    PreserveViewport: bool
    PolylineRenderer: Option<DirectionsPolylineRenderer>
}

// https://developers.google.com/maps/documentation/javascript/reference/directions#DirectionsRequest
type Directions = {
    Origin: Place
    Destination: Place
    TravelMode: TravelMode
    Waypoints: Option<Waypoint[]>

    // Not technically part of the DirectionsRequest, but each Directions object may need custom renderer options.
    RendererOptions: Option<DirectionsRendererOptions>
}

// https://developers.google.com/maps/documentation/javascript/style-reference#style-features
[<RequireQualifiedAccess>]
type MapFeatureType =
| All
| Administrative
| AdministrativeCountry
| AdministrativeLandParcel
| AdministrativeLocality
| AdministrativeNeighborhood
| AdministrativeProvince
| Landscape
| LandscapeManMade
| LandscapeNatural
| LandscapeNaturalLandcover
| LandscapeNaturalTerrain
| PointsOfInterest
| PointsOfInterestAttraction
| PointsOfInterestBusiness
| PointsOfInterestGovernment
| PointsOfInterestMedical
| PointsOfInterestPark
| PointsOfInterestPlaceOfWorship
| PointsOfInterestSchool
| PointsOfInterestSportsComplex
| Road
| RoadArterial
| RoadHighway
| RoadHighwayControlledAccess
| RoadLocal
| Transit
| TransitLine
| TransitStation
| TransitStationAirport
| TransitStationBus
| TransitStationRail
| Water

// https://developers.google.com/maps/documentation/javascript/style-reference#style-elements
[<RequireQualifiedAccess>]
type MapElementType =
| All
| Geometry
| GeometryFill
| GeometryStroke
| Labels
| LabelsIcon
| LabelsText
| LabelsTextFill
| LabelsTextStroke

// https://developers.google.com/maps/documentation/javascript/style-reference#stylers
[<RequireQualifiedAccess>]
type MapStyler =
| Color of string
| Hue of string
| Lightness of float
| Saturation of float
| Gamma of float
| InvertLightness of bool
| Weight of int

// https://developers.google.com/maps/documentation/javascript/style-reference#the-json-object
type MapStyle = {
    FeatureType: MapFeatureType
    ElementType: MapElementType
    Stylers: MapStyler[]
}

// https://developers.google.com/maps/documentation/javascript/maptypes
type MapTypeId =
| OSM
| Roadmap
| Satellite
| Hybrid
| Terrain
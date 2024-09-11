[<AutoOpen>]
module ThirdParty.Map.TypesJs

open LibClient.JsInterop
open Fable.Core.JsInterop

module Size =
    let toJs (size: Size) : obj =
        createObj [
            "width" ==> size.Width
            "height" ==> size.Height
        ]

module PixelOrPercentage =
    let toJs (pixelOrPercentage: PixelOrPercentage) : obj =
        match pixelOrPercentage with
        | PixelOrPercentage.Pixel pixel -> $"{pixel}px"
        | PixelOrPercentage.Percentage percentage -> $"{percentage}%%"

module Point =
    let toJs ((x, y): Point) : obj =
        createObj [
            "x" ==> x
            "y" ==> y
        ]

module LatLng =
    let toJs ((lat, lng): LatLng) : obj =
        createObj [
            "lat" ==> lat
            "lng" ==> lng
        ]

    let fromJs (latLng: obj) : LatLng =
        LatLng(latLng?lat(), latLng?lng())

module Icon =
    let toJs (icon: Icon) : obj =
        createObjWithOptionalValues [
            "url" ==!> icon.Url
            "anchor" ==?> (icon.Anchor |> Option.map Point.toJs)
            "labelOrigin" ==?> (icon.LabelOrigin |> Option.map Point.toJs)
            "origin" ==?> (icon.Origin |> Option.map Point.toJs)
            "scaledSize" ==?> (icon.ScaledSize |> Option.map Size.toJs)
            "size" ==?> (icon.Size |> Option.map Size.toJs)
        ]

module Symbol =
    let toJs (symbol: Symbol) : obj =
        createObjWithOptionalValues [
            "path" ==!> symbol.Path
            "anchor" ==?> (symbol.Anchor |> Option.map Point.toJs)
            "fillColor" ==?> symbol.FillColor
            "fillOpacity" ==?> symbol.FillOpacity
            "labelOrigin" ==?> (symbol.LabelOrigin |> Option.map Point.toJs)
            "rotation" ==?> symbol.Rotation
            "scale" ==?> symbol.Scale
            "strokeColor" ==?> symbol.StrokeColor
            "strokeOpacity" ==?> symbol.StrokeOpacity
            "strokeWeight" ==?> symbol.StrokeWeight
        ]

module IconSequence =
    let toJs (iconSequence: IconSequence) : obj =
        createObjWithOptionalValues [
            "fixedRotation" ==!> iconSequence.FixedRotation
            "icon" ==!> (iconSequence.Icon |> Symbol.toJs)
            "offset" ==?> (iconSequence.Offset |> Option.map PixelOrPercentage.toJs)
            "repeat" ==?> (iconSequence.Repeat |> Option.map PixelOrPercentage.toJs)
        ]

module MarkerImage =
    let toJs (markerImage: MarkerImage) : obj =
        match markerImage with
        | MarkerImage.Icon icon -> icon |> Icon.toJs
        | MarkerImage.Symbol symbol -> symbol |> Symbol.toJs

module MarkerAnimation =
    let toJs (markerAnimation: MarkerAnimation) : obj =
        match markerAnimation with
        | MarkerAnimation.Bounce -> 1
        | MarkerAnimation.Drop -> 2

module MarkerLabel =
    let toJs (markerLabel: MarkerLabel) : obj =
        createObjWithOptionalValues [
            "text" ==!> markerLabel.Text
            "className" ==?> markerLabel.ClassName
            "color" ==?> markerLabel.Color
            "fontFamily" ==?> markerLabel.FontFamily
            "fontSize" ==?> markerLabel.FontSize
            "fontWeight" ==?> markerLabel.FontWeight
        ]

module Marker =
    let toJs (marker: Marker) : obj =
        createObjWithOptionalValues [
            "draggable" ==!> marker.Draggable
            "label" ==?> (marker.Label |> Option.map MarkerLabel.toJs)
            "title" ==?> marker.Tooltip
            "icon" ==?> (marker.Image |> Option.map MarkerImage.toJs)
            "opacity" ==?> marker.Opacity
            "zIndex" ==?> marker.ZIndex
            "animation" ==?> (marker.Animation |> Option.map MarkerAnimation.toJs)
        ]

module InfoWindow =
    let toJs (infoWindow: InfoWindow) : obj =
        createObjWithOptionalValues [
            "disableAutoPan" ==!> infoWindow.DisableAutoPan
            "minWidth" ==?> infoWindow.MinWidth
            "maxWidth" ==?> infoWindow.MaxWidth
            "pixelOffset" ==?> (infoWindow.PixelOffset |> Option.map Size.toJs)
            "zIndex" ==?> infoWindow.ZIndex
        ]

module StrokePosition =
    let toJs (strokePosition: StrokePosition) : obj =
        match strokePosition with
        | StrokePosition.Center -> 0
        | StrokePosition.Inside -> 1
        | StrokePosition.Outside -> 2

module Polyline =
    let toJs (polyline: Polyline) : obj =
        createObjWithOptionalValues [
            "path" ==!> (polyline.Path |> Array.map LatLng.toJs)
            "draggable" ==!> polyline.Draggable
            "editable" ==!> polyline.Editable
            "geodesic" ==!> polyline.Geodesic
            "visible" ==!> polyline.Visible
            "icons" ==?> (polyline.Icons |> Option.map (Array.map IconSequence.toJs))
            "strokeColor" ==!> polyline.StrokeColor
            "strokeOpacity" ==!> polyline.StrokeOpacity
            "strokeWeight" ==!> polyline.StrokeWeight
            "zIndex" ==?> polyline.ZIndex
        ]

module Polygon =
    let toJs (polygon: Polygon) : obj =
        createObjWithOptionalValues [
            "paths" ==!> (polygon.Paths |> Array.map (fun path -> path |> Array.map LatLng.toJs))
            "draggable" ==!> polygon.Draggable
            "editable" ==!> polygon.Editable
            "geodesic" ==!> polygon.Geodesic
            "visible" ==!> polygon.Visible
            "fillColor" ==?> polygon.FillColor
            "fillOpacity" ==?> polygon.FillOpacity
            "strokeColor" ==?> polygon.StrokeColor
            "strokeOpacity" ==?> polygon.StrokeOpacity
            "strokePosition" ==?> (polygon.StrokePosition |> Option.map StrokePosition.toJs)
            "strokeWeight" ==?> polygon.StrokeWeight
            "zIndex" ==?> polygon.ZIndex
        ]

module Circle =
    let toJs (circle: Circle) : obj =
        createObjWithOptionalValues [
            "center" ==!> (circle.Center |> LatLng.toJs)
            "radius" ==!> circle.Radius
            "draggable" ==!> circle.Draggable
            "editable" ==!> circle.Editable
            "visible" ==!> circle.Visible
            "fillColor" ==?> circle.FillColor
            "fillOpacity" ==?> circle.FillOpacity
            "strokeColor" ==?> circle.StrokeColor
            "strokeOpacity" ==?> circle.StrokeOpacity
            "strokePosition" ==?> (circle.StrokePosition |> Option.map StrokePosition.toJs)
            "strokeWeight" ==?> circle.StrokeWeight
            "zIndex" ==?> circle.ZIndex
        ]

module Shape =
    let toJs (shape: Shape) : obj =
        match shape with
        | Shape.Polyline polyline -> polyline |> Polyline.toJs
        | Shape.Polygon polygon -> polygon |> Polygon.toJs
        | Shape.Circle circle -> circle |> Circle.toJs

module Place =
    let toJs (place: Place) : obj =
        createObj [
            match place with
            | Place.Id id -> "placeId" ==> id
            | Place.LatLng latLng -> "location" ==> (latLng |> LatLng.toJs)
            | Place.Query query -> "query" ==> query
        ]

module TravelMode =
    let toJs (travelMode: TravelMode) : obj =
        match travelMode with
        | TravelMode.Driving -> "DRIVING"
        | TravelMode.Walking -> "WALKING"
        | TravelMode.Bicycling -> "BICYCLING"
        | TravelMode.Transit -> "TRANSIT"

module Waypoint =
    let toJs (waypoint: Waypoint) : obj =
        createObj [
            "location" ==> (waypoint.Place |> Place.toJs)
            "stopover" ==> waypoint.IsStopover
        ]

module Directions =
    let toJs (directions: Directions) : obj =
        createObj [
            "origin" ==> (directions.Origin |> Place.toJs)
            "destination" ==> (directions.Destination |> Place.toJs)
            "travelMode" ==> (directions.TravelMode |> TravelMode.toJs)
            "waypoints" ==> (directions.Waypoints |> Option.map (Array.map Waypoint.toJs))
        ]

module DirectionsRendererOptions =
    let toJs (directionsRendererOptions: DirectionsRendererOptions) : obj =
        createObjWithOptionalValues [
            "draggable" ==!> directionsRendererOptions.Draggable
            "hideRouteList" ==!> directionsRendererOptions.HideRouteList
            "preserveViewport" ==!> directionsRendererOptions.PreserveViewport
        ]

module MapFeatureType =
    let toJs (mapFeatureType: MapFeatureType) : obj =
        match mapFeatureType with
        | MapFeatureType.All -> "all"
        | MapFeatureType.Administrative -> "administrative"
        | MapFeatureType.AdministrativeCountry -> "administrative.country"
        | MapFeatureType.AdministrativeLandParcel -> "administrative.land_parcel"
        | MapFeatureType.AdministrativeLocality -> "administrative.locality"
        | MapFeatureType.AdministrativeNeighborhood -> "administrative.neighborhood"
        | MapFeatureType.AdministrativeProvince -> "administrative.province"
        | MapFeatureType.Landscape -> "landscape"
        | MapFeatureType.LandscapeManMade -> "landscape.man_made"
        | MapFeatureType.LandscapeNatural -> "landscape.natural"
        | MapFeatureType.LandscapeNaturalLandcover -> "landscape.natural.landcover"
        | MapFeatureType.LandscapeNaturalTerrain -> "landscape.natural.terrain"
        | MapFeatureType.PointsOfInterest -> "poi"
        | MapFeatureType.PointsOfInterestAttraction -> "poi.attraction"
        | MapFeatureType.PointsOfInterestBusiness -> "poi.business"
        | MapFeatureType.PointsOfInterestGovernment -> "poi.government"
        | MapFeatureType.PointsOfInterestMedical -> "poi.medical"
        | MapFeatureType.PointsOfInterestPark -> "poi.park"
        | MapFeatureType.PointsOfInterestPlaceOfWorship -> "poi.place_of_worship"
        | MapFeatureType.PointsOfInterestSchool -> "poi.school"
        | MapFeatureType.PointsOfInterestSportsComplex -> "poi.sports_complex"
        | MapFeatureType.Road -> "road"
        | MapFeatureType.RoadArterial -> "road.arterial"
        | MapFeatureType.RoadHighway -> "road.highway"
        | MapFeatureType.RoadHighwayControlledAccess -> "road.highway.controlled_access"
        | MapFeatureType.RoadLocal -> "road.local"
        | MapFeatureType.Transit -> "transit"
        | MapFeatureType.TransitLine -> "transit.line"
        | MapFeatureType.TransitStation -> "transit.station"
        | MapFeatureType.TransitStationAirport -> "transit.station.airport"
        | MapFeatureType.TransitStationBus -> "transit.station.bus"
        | MapFeatureType.TransitStationRail -> "transit.station.rail"
        | MapFeatureType.Water -> "water"

module MapElementType =
    let toJs (mapElementType: MapElementType) : obj =
        match mapElementType with
        | MapElementType.All -> "all"
        | MapElementType.Geometry -> "geometry"
        | MapElementType.GeometryFill -> "geometry.fill"
        | MapElementType.GeometryStroke -> "geometry.stroke"
        | MapElementType.Labels -> "labels"
        | MapElementType.LabelsIcon -> "labels.icon"
        | MapElementType.LabelsText -> "labels.text"
        | MapElementType.LabelsTextFill -> "labels.text.fill"
        | MapElementType.LabelsTextStroke -> "labels.text.stroke"

module MapStyler =
    let toJs (mapStyler: MapStyler) : obj =
        createObj [
            match mapStyler with
            | MapStyler.Color v -> "color" ==> v
            | MapStyler.Hue v -> "hue" ==> v
            | MapStyler.Lightness v -> "lightness" ==> v
            | MapStyler.Saturation v -> "saturation" ==> v
            | MapStyler.Gamma v -> "gamma" ==> v
            | MapStyler.InvertLightness v -> "invert_lightness" ==> v
            | MapStyler.Weight v -> "weight" ==> v
        ]

module MapStyle =
    let toJs (mapStyle: MapStyle) : obj =
        createObj [
            "featureType" ==> (mapStyle.FeatureType |> MapFeatureType.toJs)
            "elementType" ==> (mapStyle.ElementType |> MapElementType.toJs)
            "stylers" ==> (mapStyle.Stylers |> Array.map MapStyler.toJs)
        ]

module MapTypeId =
    let mapTypeIdString (maybeMapTypeId: Option<MapTypeId>) : Option<string> =
        maybeMapTypeId
        |> Option.map (fun mapTypeId ->
            match mapTypeId with
            | OSM       -> "OSM"
            | Roadmap   -> "roadmap"
            | Satellite -> "satellite"
            | Hybrid    -> "hybrid"
            | Terrain   -> "terrain"
        )
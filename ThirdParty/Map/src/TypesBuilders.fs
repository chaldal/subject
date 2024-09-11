[<AutoOpen>]
module ThirdParty.Map.TypesBuilders

open System
open Fable.React

[<RequireQualifiedAccess>]
module Size =
    let init (width: float) (height: float) : Size =
        {
            Width = width
            Height = height
        }

    let withWidth (width: float) (size: Size) : Size =
        { size with Width = width }

    let withHeight (height: float) (size: Size) : Size =
        { size with Height = height }

[<RequireQualifiedAccess>]
module Icon =
    let init (url: string) : Icon =
        {
            Url = url
            Anchor = None
            LabelOrigin = None
            Origin = None
            ScaledSize = None
            Size = None
        }

    let withAnchor (anchor: Point) (icon: Icon) : Icon =
        { icon with Anchor = Some anchor }

    let withLabelOrigin (labelOrigin: Point) (icon: Icon) : Icon =
        { icon with LabelOrigin = Some labelOrigin }

    let withOrigin (origin: Point) (icon: Icon) : Icon =
        { icon with Origin = Some origin }

    let withScaledSize (scaledSize: Size) (icon: Icon) : Icon =
        { icon with ScaledSize = Some scaledSize }

    let withSize (size: Size) (icon: Icon) : Icon =
        { icon with Size = Some size }

[<RequireQualifiedAccess>]
module Symbol =
    let init (path: string) : Symbol =
        {
            Path = path
            Anchor = None
            FillColor = None
            FillOpacity = None
            LabelOrigin = None
            Rotation = None
            Scale = None
            StrokeColor = None
            StrokeOpacity = None
            StrokeWeight = None
        }

    let withAnchor (anchor: Point) (symbol: Symbol) : Symbol =
        { symbol with Anchor = Some anchor }

    let withFillColor (fillColor: string) (symbol: Symbol) : Symbol =
        { symbol with FillColor = Some fillColor }

    let withFillOpacity (fillOpacity: float) (symbol: Symbol) : Symbol =
        { symbol with FillOpacity = Some fillOpacity }

    let withLabelOrigin (labelOrigin: Point) (symbol: Symbol) : Symbol =
        { symbol with LabelOrigin = Some labelOrigin }

    let withRotation (rotation: float) (symbol: Symbol) : Symbol =
        { symbol with Rotation = Some rotation}

    let withScale (scale: float) (symbol: Symbol) : Symbol =
        { symbol with Scale = Some scale}

    let withStrokeColor (strokeColor: string) (symbol: Symbol) : Symbol =
        { symbol with StrokeColor = Some strokeColor}

    let withStrokeOpacity (strokeOpacity: float) (symbol: Symbol) : Symbol =
        { symbol with StrokeOpacity = Some strokeOpacity}

    let withStrokeWeight (strokeWeight: float) (symbol: Symbol) : Symbol =
        { symbol with StrokeWeight = Some strokeWeight}

[<RequireQualifiedAccess>]
module IconSequence =
    let init (icon: Symbol) : IconSequence =
        {
            Icon = icon
            FixedRotation = false
            Offset = None
            Repeat = None
        }

    let withFixedRotation (fixedRotation: bool) (iconSequence: IconSequence) : IconSequence =
        { iconSequence with FixedRotation = fixedRotation }

    let withOffset (offset: PixelOrPercentage) (iconSequence: IconSequence) : IconSequence =
        { iconSequence with Offset = Some offset }

    let withRepeat (repeat: PixelOrPercentage) (iconSequence: IconSequence) : IconSequence =
        { iconSequence with Repeat = Some repeat }

[<RequireQualifiedAccess>]
module MarkerLabel =
    let init (text: string) : MarkerLabel =
        {
            Text = text
            ClassName = None
            Color = None
            FontFamily = None
            FontSize = None
            FontWeight = None
        }

    let withClassName (className: string) (markerLabel: MarkerLabel) : MarkerLabel =
        { markerLabel with ClassName = Some className }

    let withColor (color: string) (markerLabel: MarkerLabel) : MarkerLabel =
        { markerLabel with Color = Some color }

    let withFontFamily (fontFamily: string) (markerLabel: MarkerLabel) : MarkerLabel =
        { markerLabel with FontFamily = Some fontFamily }

    let withFontSize (fontSize: string) (markerLabel: MarkerLabel) : MarkerLabel =
        { markerLabel with FontSize = Some fontSize }

    let withFontWeight (fontWeight: string) (markerLabel: MarkerLabel) : MarkerLabel =
        { markerLabel with FontWeight = Some fontWeight }

[<RequireQualifiedAccess>]
module Marker =
    let initWithKey (key: string) (position: MarkerPosition) : Marker =
        {
            Key = key
            Position = position
            Draggable = false
            Label = None
            Tooltip = None
            Image = None
            Opacity = None
            ZIndex = None
            Animation = None
            InfoWindow = None
            OnClick = None
        }

    let init (position: MarkerPosition) : Marker =
        initWithKey (Guid.NewGuid().ToString()) position

    let withDraggable (draggable: bool) (marker: Marker) : Marker =
        { marker with Draggable = draggable }

    let withOpacity (opacity: float) (marker: Marker) : Marker =
        { marker with Opacity = Some opacity }

    let withLabel (label: MarkerLabel) (marker: Marker) : Marker =
        { marker with Label = Some label }

    let withTooltip (tooltip: string) (marker: Marker) : Marker =
        { marker with Tooltip = Some tooltip }

    let withImage (image: MarkerImage) (marker: Marker) : Marker =
        { marker with Image = Some image }

    let withZIndex (zIndex: int) (marker: Marker) : Marker =
        { marker with ZIndex = Some zIndex }

    let withAnimation (animation: MarkerAnimation) (marker: Marker) : Marker =
        { marker with Animation = Some animation }

    let withInfoWindow (infoWindow: InfoWindow) (marker: Marker) : Marker =
        { marker with InfoWindow = Some infoWindow }

    let withOnClick (onClick: unit -> unit) (marker: Marker) : Marker =
        { marker with OnClick = Some (IgnoredDuringComparison onClick) }

[<RequireQualifiedAccess>]
module InfoWindow =
    let init (content: InfoWindowHandle -> ReactElement) : InfoWindow =
        {
            Content = IgnoredDuringComparison content
            ShouldFocus = false
            DisableAutoPan = false
            GetDisplayLocation = None
            IsOpen = false
            MinWidth = None
            MaxWidth = None
            PixelOffset = None
            ZIndex = None
        }

    let withShouldFocus (shouldFocus: bool) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with ShouldFocus = shouldFocus }

    let withDisableAutoPan (disableAutoPan: bool) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with DisableAutoPan = disableAutoPan }

    let withGetDisplayLocation (getDisplayLocation: Option<LatLng> -> LatLng) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with GetDisplayLocation = getDisplayLocation |> IgnoredDuringComparison |> Some }

    let withIsOpen (isOpen: bool) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with IsOpen = isOpen }

    let withMinWidth (minWidth: int) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with MinWidth = Some minWidth }

    let withMaxWidth (maxWidth: int) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with MaxWidth = Some maxWidth }

    let withPixelOffset (pixelOffset: Size) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with PixelOffset = Some pixelOffset }

    let withZIndex (zIndex: int) (infoWindow: InfoWindow) : InfoWindow =
        { infoWindow with ZIndex = Some zIndex }

[<RequireQualifiedAccess>]
module Polyline =
    let initWithKey (key: string) (path: seq<LatLng>) : Polyline =
        {
            Key = key
            Path = path |> Array.ofSeq
            Draggable = false
            Editable = false
            Geodesic = false
            Visible = true
            Icons = None
            StrokeColor = None
            StrokeOpacity = None
            StrokeWeight = None
            ZIndex = None
            InfoWindow = None
            OnClick = None
        }

    let init (path: seq<LatLng>) : Polyline =
        initWithKey (Guid.NewGuid().ToString()) path

    let withPath (path: seq<LatLng>) (polyline: Polyline) : Polyline =
        { polyline with Path = path |> Array.ofSeq }

    let withDraggable (draggable: bool) (polyline: Polyline) : Polyline =
        { polyline with Draggable = draggable }

    let withEditable (editable: bool) (polyline: Polyline) : Polyline =
        { polyline with Editable = editable }

    let withGeodesic (geodesic: bool) (polyline: Polyline) : Polyline =
        { polyline with Geodesic = geodesic }

    let withVisible (visible: bool) (polyline: Polyline) : Polyline =
        { polyline with Visible = visible }

    let withIcons (icons: IconSequence[]) (polyline: Polyline) : Polyline =
        { polyline with Icons = Some icons }

    let withStrokeColor (strokeColor: string) (polyline: Polyline) : Polyline =
        { polyline with StrokeColor = Some strokeColor }

    let withStrokeOpacity (strokeOpacity: float) (polyline: Polyline) : Polyline =
        { polyline with StrokeOpacity = Some strokeOpacity }

    let withStrokeWeight (strokeWeight: float) (polyline: Polyline) : Polyline =
        { polyline with StrokeWeight = Some strokeWeight }

    let withZIndex (zIndex: int) (polyline: Polyline) : Polyline =
        { polyline with ZIndex = Some zIndex }

    let withInfoWindow (infoWindow: InfoWindow) (polyline: Polyline) : Polyline =
        { polyline with InfoWindow = Some infoWindow }

    let withOnClick (onClick: unit -> unit) (polyline: Polyline) : Polyline =
        { polyline with OnClick = Some (IgnoredDuringComparison onClick) }

[<RequireQualifiedAccess>]
module Polygon =
    let initWithKey (key: string) (paths: seq<seq<LatLng>>) : Polygon =
        {
            Key = key
            Paths = paths |> Array.ofSeq |> Array.map Array.ofSeq
            Draggable = false
            Editable = false
            Geodesic = false
            Visible = true
            FillColor = None
            FillOpacity = None
            StrokeColor = None
            StrokeOpacity = None
            StrokePosition = None
            StrokeWeight = None
            ZIndex = None
            InfoWindow = None
            OnClick = None
        }

    let init (paths: seq<seq<LatLng>>) : Polygon =
        initWithKey (Guid.NewGuid().ToString()) paths

    let withDraggable (draggable: bool) (polygon: Polygon) : Polygon =
        { polygon with Draggable = draggable }

    let withEditable (editable: bool) (polygon: Polygon) : Polygon =
        { polygon with Editable = editable }

    let withGeodesic (geodesic: bool) (polygon: Polygon) : Polygon =
        { polygon with Geodesic = geodesic }

    let withVisible (visible: bool) (polygon: Polygon) : Polygon =
        { polygon with Visible = visible }

    let withFillColor (fillColor: string) (polygon: Polygon) : Polygon =
        { polygon with FillColor = Some fillColor }

    let withFillOpacity (fillOpacity: float) (polygon: Polygon) : Polygon =
        { polygon with FillOpacity = Some fillOpacity }

    let withStrokeColor (strokeColor: string) (polygon: Polygon) : Polygon =
        { polygon with StrokeColor = Some strokeColor }

    let withStrokeOpacity (strokeOpacity: float) (polygon: Polygon) : Polygon =
        { polygon with StrokeOpacity = Some strokeOpacity }

    let withStrokePosition (strokePosition: StrokePosition) (polygon: Polygon) : Polygon =
        { polygon with StrokePosition = Some strokePosition }

    let withStrokeWeight (strokeWeight: float) (polygon: Polygon) : Polygon =
        { polygon with StrokeWeight = Some strokeWeight }

    let withZIndex (zIndex: int) (polygon: Polygon) : Polygon =
        { polygon with ZIndex = Some zIndex }

    let withInfoWindow (infoWindow: InfoWindow) (polygon: Polygon) : Polygon =
        { polygon with InfoWindow = Some infoWindow }

    let withOnClick (onClick: unit -> unit) (polygon: Polygon) : Polygon =
        { polygon with OnClick = Some (IgnoredDuringComparison onClick) }

[<RequireQualifiedAccess>]
module Circle =
    let initWithKey (key: string) (center: LatLng) (radius: float) : Circle =
        {
            Key = key
            Center = center
            Radius = radius
            Draggable = false
            Editable = false
            Visible = true
            FillColor = None
            FillOpacity = None
            StrokeColor = None
            StrokeOpacity = None
            StrokePosition = None
            StrokeWeight = None
            ZIndex = None
            InfoWindow = None
            OnClick = None
        }

    let init (center: LatLng) (radius: float) : Circle =
        initWithKey (Guid.NewGuid().ToString()) center radius

    let withDraggable (draggable: bool) (circle: Circle) : Circle =
        { circle with Draggable = draggable }

    let withEditable (editable: bool) (circle: Circle) : Circle =
        { circle with Editable = editable }

    let withVisible (visible: bool) (circle: Circle) : Circle =
        { circle with Visible = visible }

    let withFillColor (fillColor: string) (circle: Circle) : Circle =
        { circle with FillColor = Some fillColor }

    let withFillOpacity (fillOpacity: float) (circle: Circle) : Circle =
        { circle with FillOpacity = Some fillOpacity }

    let withStrokeColor (strokeColor: string) (circle: Circle) : Circle =
        { circle with StrokeColor = Some strokeColor }

    let withStrokeOpacity (strokeOpacity: float) (circle: Circle) : Circle =
        { circle with StrokeOpacity = Some strokeOpacity }

    let withStrokePosition (strokePosition: StrokePosition) (circle: Circle) : Circle =
        { circle with StrokePosition = Some strokePosition }

    let withStrokeWeight (strokeWeight: float) (circle: Circle) : Circle =
        { circle with StrokeWeight = Some strokeWeight }

    let withZIndex (zIndex: int) (circle: Circle) : Circle =
        { circle with ZIndex = Some zIndex }

    let withInfoWindow (infoWindow: InfoWindow) (circle: Circle) : Circle =
        { circle with InfoWindow = Some infoWindow }

    let withOnClick (onClick: unit -> unit) (circle: Circle) : Circle =
        { circle with OnClick = Some (IgnoredDuringComparison onClick) }

[<RequireQualifiedAccess>]
module Waypoint =
    let init (place: Place) : Waypoint =
        {
            Place = place
            IsStopover = false
        }

    let withIsStopover (isStopover: bool) (waypoint: Waypoint) =
        { waypoint with IsStopover = isStopover }

[<RequireQualifiedAccess>]
module DirectionsRendererOptions =
    let init () : DirectionsRendererOptions =
        {
            Draggable = false
            HideRouteList = false
            PreserveViewport = false
            PolylineRenderer = None
        }

    let withDraggable (draggable: bool) (directionsRendererOptions: DirectionsRendererOptions) : DirectionsRendererOptions =
        { directionsRendererOptions with Draggable = draggable }

    let withHideRouteList (hideRouteList: bool) (directionsRendererOptions: DirectionsRendererOptions) : DirectionsRendererOptions =
        { directionsRendererOptions with HideRouteList = hideRouteList }

    let withPreserveViewport (preserveViewport: bool) (directionsRendererOptions: DirectionsRendererOptions) : DirectionsRendererOptions =
        { directionsRendererOptions with PreserveViewport = preserveViewport }

    let withPolyline (polyline: Polyline) (directionsRendererOptions: DirectionsRendererOptions) : DirectionsRendererOptions =
        { directionsRendererOptions with PolylineRenderer = Some (DirectionsPolylineRenderer.Polyline polyline) }

    let withPolylineCallback (polylineCallback: int -> seq<LatLng> -> Polyline) (directionsRendererOptions: DirectionsRendererOptions) : DirectionsRendererOptions =
        { directionsRendererOptions with PolylineRenderer = Some (DirectionsPolylineRenderer.Callback (IgnoredDuringComparison polylineCallback)) }

[<RequireQualifiedAccess>]
module Directions =
    let init (origin: Place) (destination: Place) (travelMode: TravelMode) : Directions =
        {
            Origin = origin
            Destination = destination
            TravelMode = travelMode
            Waypoints = None
            RendererOptions = None
        }

    let withWaypoints (waypoints: seq<Waypoint>) (directions: Directions) : Directions =
        { directions with Waypoints = waypoints |> Array.ofSeq |> Some }

    let withRendererOptions (rendererOptions: DirectionsRendererOptions) (directions: Directions) : Directions =
        { directions with RendererOptions = Some rendererOptions }

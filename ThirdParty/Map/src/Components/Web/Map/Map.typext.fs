module ThirdParty.Map.Components.Web.Map

open LibClient
open LibClient.JsInterop
open ThirdParty.Map

open Fable.Core
open Fable.Core.JsInterop
open Fable.React

type private LoaderOptions = {
    apiKey:    string
    version:   string
    libraries: array<string>
}

type Props = (* GenerateMakeFunction *) {
    ApiKey:            string
    Position:          MapPosition
    OnPositionChanged: Option<MapPosition -> unit>
    Zoom:              Option<int>
    Markers:           Option<List<Marker>>
    Shapes:            Option<List<Shape>>
    Directions:        Option<List<Directions>>
    BackgroundColor:   Option<string>
    ClickableIcons:    Option<bool>
    DisableDefaultUI:  Option<bool>
    MinZoom:           Option<float>
    MaxZoom:           Option<float>
    FullScreen:        Option<bool>
    MapStyles:         Option<List<MapStyle>>
    MapTypeId:         Option<MapTypeId>
    Ref:               IWebMapViewRef -> unit

    key: string option // defaultWithAutoWrap JsUndefined
}

#if EGGSHELL_PLATFORM_IS_WEB

[<AutoOpen>]
module private Helpers =
    let toMapOptionsJs (props: Props) : obj =
        createObjWithOptionalValues [
            "backgroundColor"  ==?> props.BackgroundColor
            "clickableIcons"   ==?> props.ClickableIcons
            "disableDefaultUI" ==?> props.DisableDefaultUI
            "minZoom"          ==?> props.MinZoom
            "maxZoom"          ==?> props.MaxZoom
            "mapTypeId"        ==?> MapTypeId.mapTypeIdString props.MapTypeId
            "styles"           ==?> (props.MapStyles |> Option.map (fun styles -> styles |> List.map (MapStyle.toJs) |> Array.ofList))
        ]

// TODO: only required because we're not on the latest Fable.React, and upgrade attempts have been unsuccessful. Alfonso may be upgrading, at which
// point I can remove this helper module.
[<AutoOpen>]
module private ReactIntegration =
    type IReactRoot =
        abstract render: element: ReactElement -> unit

        abstract unmount: unit -> unit

    [<ImportMember("react-dom/client")>]
    let createRoot (_container: Browser.Types.Element): IReactRoot = jsNative

type private GoogleMaps = {
    Api: obj
    Map: obj
    Div: obj
    DirectionsService: obj
}

type private WebMapViewRef (map: obj) =
    inherit IWebMapViewRef (map)
    override this.panTo (latLng: LatLng) =
        latLng
        |> LatLng.toJs
        |> map?panTo
        |> ignore

    override this.setZoom (zoom: int) =
        zoom
        |> map?setZoom
        |> ignore


[<RequireQualifiedAccess>]
type private ListItemDelta<'T> =
| Added of 'T
| Updated of OldValue: 'T * NewValue: 'T
| Removed of 'T

let inline private listDelta<'T when 'T :> IKeyed> (first: List<'T>) (second: List<'T>): List<ListItemDelta<'T>> =
    let firstMap =
        first
        |> List.map (fun item -> item.Key, item)
        |> Map.ofList
    let secondMap =
        second
        |> List.map (fun item -> item.Key, item)
        |> Map.ofList

    Set.union firstMap.KeySet secondMap.KeySet
    |> Set.toList
    |> List.map (fun key ->
        let maybeFirst = firstMap |> Map.tryFind key
        let maybeSecond = secondMap |> Map.tryFind key

        match maybeFirst, maybeSecond with
        | None, Some newValue -> ListItemDelta.Added newValue
        | Some oldValue, Some newValue -> ListItemDelta.Updated (oldValue, newValue)
        | Some oldValue, None -> ListItemDelta.Removed oldValue
        | None, None -> failwith "Unexpected"
    )

let inline private propDelta<'T> (compare: 'T -> 'T -> bool) (maybeOldProps: Option<Props>) (props: Props) (f: Props -> 'T): Option<'T> =
    let currentValue = f props

    match maybeOldProps with
    | None -> Some currentValue
    | Some oldProps ->
        let oldValue = f oldProps

        match compare oldValue currentValue with
        | true -> None
        | false -> Some currentValue

let inline private propEqualityDelta<'T when 'T: equality> (maybeOldProps: Option<Props>) (props: Props) (f: Props -> 'T): Option<'T> =
    propDelta<'T> (=) maybeOldProps props f

let inline private propReferenceDelta<'T> (maybeOldProps: Option<Props>) (props: Props) (f: Props -> 'T): Option<'T> =
    propDelta<'T> (fun first second -> obj.ReferenceEquals(first, second)) maybeOldProps props f

let inline private propListDelta<'T when 'T :> IKeyed> (maybeOldProps: Option<Props>) (props: Props) (f: Props -> List<'T>): List<ListItemDelta<'T>> =
    let currentValue = f props

    match maybeOldProps with
    | None ->
        // Newly assigned list, so all items have been added.
        currentValue
        |> List.map ListItemDelta.Added
    | Some oldProps ->
        let oldValue = f oldProps
        listDelta oldValue currentValue

let inline private propMaybeListDelta<'T when 'T :> IKeyed> (maybeOldProps: Option<Props>) (props: Props) (f: Props -> Option<List<'T>>): List<ListItemDelta<'T>> =
    let maybeOldValue = maybeOldProps |> Option.bind f
    let maybeCurrentValue = f props

    match maybeOldValue, maybeCurrentValue with
    | None, None -> List.empty
    | Some oldValues, None ->
        oldValues
        |> List.map ListItemDelta.Removed
    | None, Some newValues ->
        newValues
        |> List.map ListItemDelta.Added
    | Some oldValues, Some newValues ->
        listDelta oldValues newValues

type private JsInfoWindowMountInfo = {
    ReactRoot: IReactRoot
    Handle: InfoWindowHandle
}

// The below types are something of a "poor man's interop". Fable is capable of stronger, more type safe, interop, but I found it quite finicky to use
// and not worth the effort. That said, I find working completely without types to be a problem as well. To that end, I have introduced *some* type safety
// by way of simple wrapper types. I did run into an issue when attempting to do the same for the map and API objects, otherwise I'd have used this
// approach more comprehensively. I will likely return to this in the future.
type private JsInfoWindow = | JsInfoWindow of obj
with
    static member Create (googleMaps: GoogleMaps) (infoWindowProvider: IInfoWindowProvider) (infoWindow: InfoWindow): JsInfoWindow =
        let jsInfoWindow =
            createNew
                googleMaps.Api?maps?InfoWindow
                ()
            |> JsInfoWindow

        jsInfoWindow.DivId <- $"info-window-%s{System.Guid.NewGuid().ToString()}"

        jsInfoWindow.Value?addListener(
            "closeclick",
            (fun () ->
                infoWindowProvider.MaybeAssociatedJsInfoWindow <- None
                jsInfoWindow.Close()
            )
        )

        jsInfoWindow.Value?addListener(
            "domready",
            fun () -> jsInfoWindow.UpdateContentFrom googleMaps infoWindowProvider infoWindow
        )

        jsInfoWindow

    member this.Value =
        let (JsInfoWindow value) = this
        value

    member private this.SetContent(content: string) =
        this.Value?setContent(content)

    member private this.DivId
        with get(): string =
            this.Value?divId
        and set(value: string) =
            this.Value?divId <- value
            this.SetContent($"<div id='%s{value}'></div>")

    member private this.MaybeMountInfo
        with get(): Option<JsInfoWindowMountInfo> =
            if this.Value?mountInfo = JsUndefined then
                None
            else
                this.Value?mountInfo
                |> Some
        and set(value: Option<JsInfoWindowMountInfo>) =
            this.Value?mountInfo <-
                value
                |> Option.map box
                |> Option.defaultValue JsUndefined

    // May need to generalize the "jsMarker" parameter to an "anchor" DU if we need info windows for anything other than markers.
    member this.Open (googleMaps: GoogleMaps) (infoWindowProvider: IInfoWindowProvider) (infoWindow: InfoWindow): unit =
        let openOptions =
            createObj [
                "anchor" ==>
                    match infoWindowProvider.Anchor with
                    | InfoWindowAnchor.Marker marker ->
                        marker.Value
                    | InfoWindowAnchor.Position _ ->
                        // We have to position the info window via a call to setPosition
                        null
                "map" ==> googleMaps.Map
                "shouldFocus" ==> infoWindow.ShouldFocus
            ]

        match infoWindowProvider.Anchor with
        | InfoWindowAnchor.Position latLng ->
            this.Value?setPosition(latLng |> LatLng.toJs)
        | InfoWindowAnchor.Marker _ ->
            ()

        this.Value?``open``(openOptions)
        ()

    member this.Focus(): unit =
        this.Value?focus()
        ()

    member this.Close(): unit =
        this.Value?close()

        this.MaybeMountInfo
        |> Option.iter (fun mountInfo ->
            mountInfo.ReactRoot.unmount()
            this.MaybeMountInfo <- None
        )

        ()

    member this.UpdateFrom (googleMaps: GoogleMaps) (infoWindowProvider: IInfoWindowProvider) (infoWindow: InfoWindow): unit =
        this.Value?setOptions(infoWindow |> InfoWindow.toJs)
        this.UpdateContentFrom googleMaps infoWindowProvider infoWindow

    member private this.UpdateContentFrom (googleMaps: GoogleMaps) (infoWindowProvider: IInfoWindowProvider) (infoWindow: InfoWindow): unit=
        let maybeMountInfo =
            match this.MaybeMountInfo with
            | Some mountInfo ->
                Some mountInfo
            | None ->
                let infoWindowDiv: Browser.Types.Element = googleMaps.Div?querySelector($"#{this.DivId}")

                if infoWindowDiv = null then
                    // Might still be unavailable if DOM not ready.
                    None
                else
                    let reactRoot = createRoot infoWindowDiv
                    let infoWindowHandle =
                        { new InfoWindowHandle with
                            member _.Focus() =
                                this.Focus()

                            member _.Close() =
                                infoWindowProvider.MaybeAssociatedJsInfoWindow <- None
                                this.Close()
                        }
                    let mountInfo = {
                        ReactRoot = reactRoot
                        Handle = infoWindowHandle
                    }
                    this.MaybeMountInfo <- Some mountInfo
                    Some mountInfo

        match maybeMountInfo with
        | Some mountInfo ->
            let reactElement = infoWindow.Content.Value mountInfo.Handle
            mountInfo.ReactRoot.render reactElement
        | None ->
            ()

and [<RequireQualifiedAccess>] private InfoWindowAnchor =
    | Marker of JsMarker
    | Position of LatLng

and private IInfoWindowProvider =
    abstract Anchor: InfoWindowAnchor
    abstract MaybeAssociatedJsInfoWindow: Option<JsInfoWindow> with get, set
    abstract HasAssociatedJsInfoWindow: bool

and private JsMarker = | JsMarker of obj
with
    static member Create (googleMaps: GoogleMaps): JsMarker =
        createNew
            googleMaps.Api?maps?Marker
            ()
        |> JsMarker

    member this.Value =
        let (JsMarker value) = this
        value

    member this.HasClickListener
        with get(): bool =
            if this.Value?hasClickListener = JsUndefined then
                false
            else
                this.Value?hasClickListener
        and set(value: bool) =
            this.Value?hasClickListener <- value

    member this.MaybeAssociatedJsInfoWindow
        with get(): Option<JsInfoWindow> =
            if this.Value?associatedJsInfoWindow = JsUndefined then
                None
            else
                this.Value?associatedJsInfoWindow
                |> Some
        and set(value: Option<JsInfoWindow>) =
            this.Value?associatedJsInfoWindow <-
                value
                |> Option.map box
                |> Option.defaultValue JsUndefined

    member this.HasAssociatedJsInfoWindow: bool =
        this.MaybeAssociatedJsInfoWindow
        |> Option.map (fun _ -> true)
        |> Option.defaultValue false

    member this.UpdateFrom (marker: Marker): JsMarker =
        this.Value?setOptions(marker |> Marker.toJs)
        this

    interface IInfoWindowProvider with
        member this.Anchor = InfoWindowAnchor.Marker this
        member this.MaybeAssociatedJsInfoWindow
            with get() = this.MaybeAssociatedJsInfoWindow
            and set(value) = this.MaybeAssociatedJsInfoWindow <- value
        member this.HasAssociatedJsInfoWindow = this.HasAssociatedJsInfoWindow

and private JsShape = | JsShape of obj
with
    member this.Value =
        let (JsShape value) = this
        value

    member this.GetBoundaryPoints(): array<LatLng> =
        if not <| isUndefined(this.Value?getPaths) then
            // Polygon.getPaths returns MVCArray<MVCArray<LatLng>>, so we need to unwrap
            this.Value?getPaths()?getArray()
            |> Array.map (fun mvcPath -> mvcPath?getArray())
            |> Array.concat
        else if not <| isUndefined(this.Value?getPath) then
            // Polyline.getPath returns MVCArray<LatLng>
            this.Value?getPath()?getArray()
        else if not <| isUndefined(this.Value?getBounds) then
            // Circle.getBounds gives us the circle boundaries
            let bounds = this.Value?getBounds()
            [|
                bounds?getNorthEast()
                bounds?getSouthWest()
            |]
        else
            Array.empty

    member this.HasClickListener
        with get(): bool =
            if this.Value?hasClickListener = JsUndefined then
                false
            else
                this.Value?hasClickListener
        and set(value: bool) =
            this.Value?hasClickListener <- value

    member this.MaybeAssociatedJsInfoWindow
        with get(): Option<JsInfoWindow> =
            if this.Value?associatedJsInfoWindow = JsUndefined then
                None
            else
                this.Value?associatedJsInfoWindow
                |> Some
        and set(value: Option<JsInfoWindow>) =
            this.Value?associatedJsInfoWindow <-
                value
                |> Option.map box
                |> Option.defaultValue JsUndefined

    member this.HasAssociatedJsInfoWindow: bool =
        this.MaybeAssociatedJsInfoWindow
        |> Option.map (fun _ -> true)
        |> Option.defaultValue false

    member this.DisplayLatLng
        with get(): Option<LatLng> =
            if this.Value?displayLatLng = JsUndefined then
                None
            else
                this.Value?displayLatLng
                |> Some
        and set(value: Option<LatLng>) =
            this.Value?displayLatLng <-
                value
                |> Option.map box
                |> Option.defaultValue JsUndefined

    interface IInfoWindowProvider with
        member this.Anchor =
            let latLng = defaultArg this.DisplayLatLng (LatLng (0, 0))
            InfoWindowAnchor.Position latLng
        member this.MaybeAssociatedJsInfoWindow
            with get() = this.MaybeAssociatedJsInfoWindow
            and set(value) = this.MaybeAssociatedJsInfoWindow <- value
        member this.HasAssociatedJsInfoWindow = this.HasAssociatedJsInfoWindow

type Map(_initialProps) as this =
    inherit PureStatelessComponent<Props, Actions, Map>("ThirdParty.Map.Components.Web.Map", _initialProps, Actions, hasStyles = true)

    let log =
        Log
            .WithCategory("Map")

    let mutable maybeGoogleMaps = None
    let mutable maybeExistingPositionChangedListener = None

    // Keep track of various JavaScript objects for cleanup purposes. Using mutable collections for performance.
    let existingJsMarkers = System.Collections.Generic.Dictionary<string, JsMarker>()
    let existingJsShapes = System.Collections.Generic.Dictionary<string, JsShape>()
    let existingDirectionsJsShapes = ResizeArray<JsShape>()

    let closeAllJsInfoWindows () : unit =
        let closeJsInfoWindow (infoWindowProvider: IInfoWindowProvider) : unit =
            infoWindowProvider.MaybeAssociatedJsInfoWindow
            |> Option.iter (fun jsInfoWindow ->
                infoWindowProvider.MaybeAssociatedJsInfoWindow <- None
                jsInfoWindow.Close()
            )

        existingJsMarkers.Values
        |> Seq.iter closeJsInfoWindow

        existingJsShapes.Values
        |> Seq.iter closeJsInfoWindow

    let updateJsMarker (googleMaps: GoogleMaps) map (marker: Marker) (jsMarker: JsMarker): JsMarker =
        let showInfoWindow (infoWindow: InfoWindow) =
            if not jsMarker.HasAssociatedJsInfoWindow then
                closeAllJsInfoWindows()

                let jsInfoWindow = JsInfoWindow.Create googleMaps jsMarker infoWindow
                jsInfoWindow.UpdateFrom googleMaps jsMarker infoWindow
                jsInfoWindow.Open googleMaps jsMarker infoWindow

                jsMarker.MaybeAssociatedJsInfoWindow <- Some jsInfoWindow

        jsMarker.UpdateFrom(marker)
        |> ignore

        match marker.Position with
        | MarkerPosition.LatLng latLng ->
            jsMarker.Value?setPosition(latLng |> LatLng.toJs)
        | MarkerPosition.Centered ->
            let centerMarker () =
                let latLng = map?getCenter()

                if latLng = JsUndefined then failwith "Attempting to center marker but map has no center defined"

                jsMarker.Value?setPosition(latLng)

            centerMarker ()

            let listener = googleMaps.Api?maps?event?addDomListener(map, "center_changed", (fun () -> centerMarker ()))
            this.RunOnUnmount(fun () -> googleMaps.Api?maps?event?removeListener(listener))

        match marker.ZIndex with
        | Some zIndex -> jsMarker.Value?setZIndex(zIndex)
        | None -> ()

        match marker.Animation with
        | Some animation -> jsMarker.Value?setAnimation(animation |> MarkerAnimation.toJs)
        | None -> ()

        match marker.InfoWindow with
        | Some infoWindow ->
            if not jsMarker.HasClickListener then
                jsMarker.Value?addListener(
                    "click",
                    fun () ->
                        showInfoWindow infoWindow
                )
                jsMarker.HasClickListener <- true

            match jsMarker.MaybeAssociatedJsInfoWindow with
            | Some jsInfoWindow ->
                jsInfoWindow.UpdateFrom googleMaps jsMarker infoWindow
            | None ->
                ()

            if infoWindow.IsOpen then
                showInfoWindow infoWindow
        | None -> ()

        match marker.OnClick with
        | Some onClick -> jsMarker.Value?addListener("click", onClick)
        | None -> ()

        jsMarker.Value?setOpacity(marker.Opacity)

        jsMarker

    let addJsMarker map (jsMarker: JsMarker): JsMarker =
        jsMarker.Value?setMap(map)
        jsMarker

    let removeJsMarker (jsMarker: JsMarker): JsMarker =
        jsMarker.Value?setMap(null)
        jsMarker

    let createJsShape google shape: JsShape =
        match shape with
        | Shape.Polyline _ ->
            createNew google?maps?Polyline ()
        | Shape.Polygon _ ->
            createNew google?maps?Polygon ()
        | Shape.Circle _ ->
            createNew google?maps?Circle ()
        |> JsShape

    let updateJsShape (googleMaps: GoogleMaps) (shape: Shape) (jsShape: JsShape): JsShape =
        let showInfoWindow (infoWindow: InfoWindow) (maybeClickLocation: Option<LatLng>) =
            let displayLocationFromCallback =
                infoWindow.GetDisplayLocation
                |> Option.map (fun getDisplayLocation -> getDisplayLocation.Value maybeClickLocation)

            let displayLocation =
                match displayLocationFromCallback, maybeClickLocation with
                | Some l, _
                | None, Some l -> l
                | None, None ->
                    failwith "No display location for the InfoWindow could be determined. The InfoWindow has no GetDisplayLocation callback, and there is no click location available"

            jsShape.DisplayLatLng <- Some displayLocation

            if not jsShape.HasAssociatedJsInfoWindow then
                closeAllJsInfoWindows()

                let jsInfoWindow = JsInfoWindow.Create googleMaps jsShape infoWindow
                jsInfoWindow.UpdateFrom googleMaps jsShape infoWindow
                jsInfoWindow.Open googleMaps jsShape infoWindow

                jsShape.MaybeAssociatedJsInfoWindow <- Some jsInfoWindow

        let options =
            match shape with
            | Shape.Polyline polyline ->
                polyline |> Polyline.toJs
            | Shape.Polygon polygon ->
                polygon |> Polygon.toJs
            | Shape.Circle circle ->
                circle |> Circle.toJs
        jsShape.Value?setOptions(options)

        match shape.InfoWindow with
        | Some infoWindow ->
            if not jsShape.HasClickListener then
                jsShape.Value?addListener(
                    "click",
                    fun (mapsMouseEvent: obj) ->
                        let clickLatLng = mapsMouseEvent?latLng |> LatLng.fromJs
                        showInfoWindow infoWindow (Some clickLatLng)
                )
                jsShape.HasClickListener <- true

            match jsShape.MaybeAssociatedJsInfoWindow with
            | Some jsInfoWindow ->
                jsInfoWindow.UpdateFrom googleMaps jsShape infoWindow
            | None ->
                ()

            if infoWindow.IsOpen then
                showInfoWindow infoWindow None
        | None -> ()

        match shape.OnClick with
        | Some onClick -> jsShape.Value?addListener("click", onClick)
        | None -> ()

        jsShape

    let addJsShape map (jsShape: JsShape): JsShape =
        jsShape.Value?setMap(map)
        jsShape

    let removeJsShape (jsShape: JsShape): JsShape =
        jsShape.Value?setMap(null)
        jsShape

    let renderRoute googleMapsApi map (maybeDirectionsRendererOptions: Option<DirectionsRendererOptions>) route: seq<JsShape> =
        let legs = route?legs
        let legCount = legs?length

        seq {
            for l in 0 .. legCount - 1 do
                let leg = legs?(l)
                let steps = leg?steps
                let stepCount = steps?length

                for s in 0 .. stepCount - 1 do
                    let step = steps?(s)
                    let path = step?path
                    let latLngs =
                        seq {
                            for x in 0 .. path?length - 1 do
                                let latLng = path?(x)
                                yield (latLng?lat (), latLng?lng ())
                        }

                    let polyline =
                        maybeDirectionsRendererOptions
                        |> Option.bind (fun directionsRendererOptions -> directionsRendererOptions.PolylineRenderer)
                        |> Option.map (fun polylineRenderer ->
                            match polylineRenderer with
                            | DirectionsPolylineRenderer.Polyline polyline ->
                                polyline
                                |> Polyline.withPath latLngs
                            | DirectionsPolylineRenderer.Callback (IgnoredDuringComparison callback) ->
                                callback l latLngs
                        )
                        |> Option.defaultValue (
                            Polyline.initWithKey $"route-segment-%i{l}-%i{s}" latLngs
                            |> Polyline.withStrokeColor "#66A4E2"
                            |> Polyline.withStrokeWeight 6.0
                            |> Polyline.withStrokeOpacity 0.75
                        )
                        |> Shape.Polyline
                    let jsShape =
                        createJsShape googleMapsApi polyline
                        |> addJsShape map

                    yield jsShape
        }

    let setDirections googleMapsApi map directionsService (directions: List<Directions>): JS.Promise<Result<List<JsShape>, string>> =
        let directionsRendererOptions =
            {|
                // We want to render our own markers and polylines to gain full control over them.
                suppressMarkers = true
                suppressPolylines = true
            |}

        promise {
            let! directionsWithResponses =
                directions
                |> List.map (fun directions ->
                    promise {
                        let! response = directionsService?route(directions |> Directions.toJs)
                        return directions, response
                    }
                )
                |> Promise.all

            let allOk =
                directionsWithResponses
                |> Array.forall (fun (_, response) -> response?status = googleMapsApi?maps?DirectionsStatus?OK)

            if not allOk then
                return Error $"Failed to read response from directions service"
            else
                let allJsShapes =
                    directionsWithResponses
                    |> Array.collect (fun (directions, response) ->
                        let route = response?routes?(0)
                        let shapes = renderRoute googleMapsApi map directions.RendererOptions route

                        let directionsRenderer = createNew googleMapsApi?maps?DirectionsRenderer (directionsRendererOptions)
                        directionsRenderer?setMap(map)
                        directionsRenderer?setDirections(response)

                        directions.RendererOptions
                        |> Option.iter (fun directionsRendererOptions ->
                            directionsRenderer
                                ?setOptions(directionsRendererOptions |> DirectionsRendererOptions.toJs)
                        )

                        shapes
                        |> Array.ofSeq
                    )

                return allJsShapes |> List.ofArray |> Ok
        }

    member this.Load(div) =
        promise {
            // TODO try using Async.AwaitPromise and handle error
            let loader =
                createNew
                    (Fable.Core.JsInterop.import "Loader" "@googlemaps/js-api-loader")
                    {
                        apiKey    = this.props.ApiKey
                        version   = "weekly"
                        // It might seem like we can selectively choose libraries based on the features used, but this causes issues if multiple maps
                        // appear on one page, with each map using different features.
                        libraries = [| "places"; "drawing" |]
                    }

            let! googleMapsApi = loader?load ()
            let map = createNew googleMapsApi?maps?Map (div, this.props |> toMapOptionsJs)
            let directionsService = createNew googleMapsApi?maps?DirectionsService ()

            // This is a default zoom level.
            map?setZoom(14)

            map?mapTypes?set ("OSM",
                createNew googleMapsApi?maps?ImageMapType (
                    createObj [
                        "getTileUrl" ==> fun coord zoom ->
                            if isNull coord || isNull zoom then
                                Fable.Core.JS.console.log("getTileUrl called with invalid parameters. coord: %A, zoom: %A", coord, zoom)
                                ""
                            else
                                let x = coord?x
                                let y = coord?y
                                $"https://{['a'; 'b'; 'c'].[(x + y) % 3]}.tile.openstreetmap.org/{zoom}/{x}/{y}.png"
                        "tileSize" ==> createNew googleMapsApi?maps?Size (256, 256)
                        "name"     ==> "OpenStreetMap"
                        "maxZoom"  ==> 19
                    ]
                )
            )

            let infoWindow =
                createNew googleMapsApi?maps?InfoWindow (
                    createObj [
                        "position"      ==> ("lat" ==> 23.44; "lon" ==> 90.44)
                        "headerContent" ==> "Coordinates"
                    ]
                )

            map?addListener("rightclick",
                fun mapsMouseEvent ->
                    infoWindow?close(map)
                    let latLng = mapsMouseEvent?latLng
                    let coords = sprintf "%f, %f" (latLng?lat()) (latLng?lng())
                    let contentHtml =
                        sprintf
                            """
                                <div>
                                    <span
                                        style="cursor:pointer; transition: text-shadow 0.2s;"
                                        onmouseover="this.style.textShadow='0 0 4px rgba(0, 99, 25, 0.77)'"
                                        onmouseout="this.style.textShadow='none'"
                                        onclick=
                                            "
                                                navigator.clipboard.writeText('%s');
                                                let message=document.getElementById('copyMessage');
                                                message.style.display='block';
                                                setTimeout(function(){ message.style.display='none'; }, 500);
                                            "
                                    >
                                        %s
                                    </span>
                                    <span id="copyMessage" style="display: none; color: rgba(3, 5, 4, 0.77); margin-top:4px; font-size: 12px;">
                                        Copied
                                    </span>
                                </div>
                            """
                            coords
                            coords

                    infoWindow?setPosition(latLng)
                    infoWindow?setContent(contentHtml)
                    infoWindow?``open``(map)
            )

            maybeGoogleMaps <-
                {
                    Api = googleMapsApi
                    Map = map
                    Div = div
                    DirectionsService = directionsService
                }
                |> Some

            this.props.Ref (WebMapViewRef map)

            this.UpdateFromProps(None, this.props)
        }

    member this.UpdateFromProps(maybeOldProps, props) =
        match maybeGoogleMaps with
        | Some googleMaps ->
            let maybeZoomDelta = propEqualityDelta maybeOldProps props (fun p -> p.Zoom)

            match maybeZoomDelta with
            | Some maybeZoom ->
                match maybeZoom with
                | Some zoom ->
                    googleMaps.Map?setZoom(zoom)
                | None -> ()
            | None -> ()

            let maybePositionDelta = propEqualityDelta maybeOldProps props (fun p -> p.Position)
            let maybeOnPositionChangedDelta = propReferenceDelta maybeOldProps props (fun p -> p.OnPositionChanged)

            maybePositionDelta
            |> Option.iter (fun positionDelta ->
                match positionDelta with
                | MapPosition.Auto ->
                    // We handle any auto-centering below, but only if a center is not already set.
                    ()
                | MapPosition.LatLng center ->
                    googleMaps.Map?setCenter(center |> LatLng.toJs)

                match maybeOnPositionChangedDelta with
                | Some (Some onPositionChanged) ->
                    onPositionChanged positionDelta
                | _ -> ()
            )

            maybeOnPositionChangedDelta
            |> Option.iter (fun maybeOnPositionChanged ->
                maybeExistingPositionChangedListener
                |> Option.iter (fun existingPositionChangedListener ->
                    googleMaps.Api?maps?event?removeListener(existingPositionChangedListener)
                    maybeExistingPositionChangedListener <- None
                )

                maybeOnPositionChanged
                |> Option.iter (fun onPositionChanged ->
                    let listener = googleMaps.Api?maps?event?addDomListener(
                        googleMaps.Map,
                        "center_changed",
                        fun () ->
                            let latLng = googleMaps.Map?getCenter()

                            (latLng?lat (), latLng?lng ())
                            |> MapPosition.LatLng
                            |> onPositionChanged
                    )
                    maybeExistingPositionChangedListener <- Some listener
                    this.RunOnUnmount(fun () -> googleMaps.Api?maps?event?removeListener(listener))
                )
            )

            let markersDelta = propMaybeListDelta maybeOldProps props (fun p -> p.Markers)

            markersDelta
            |> List.iter (fun markerDelta ->
                match markerDelta with
                | ListItemDelta.Added marker ->
                    let jsMarker =
                        JsMarker.Create googleMaps
                        |> updateJsMarker googleMaps googleMaps.Map marker
                        |> addJsMarker googleMaps.Map

                    existingJsMarkers.Add(marker.Key, jsMarker)
                | ListItemDelta.Updated (_oldMarker, newMarker) ->
                    match existingJsMarkers.Get(newMarker.Key) with
                    | Some jsMarker ->
                        updateJsMarker googleMaps googleMaps.Map newMarker jsMarker
                        |> ignore
                    | None ->
                        log.Warn("Marker with key {Key} determined to be updated but could not be found in existing markers", newMarker.Key)
                | ListItemDelta.Removed marker ->
                    match existingJsMarkers.Get(marker.Key) with
                    | Some jsMarker ->
                        jsMarker
                        |> removeJsMarker
                        |> ignore

                        existingJsMarkers.Remove(marker.Key)
                        |> ignore
                    | None ->
                        ()
            )

            let shapesDelta = propMaybeListDelta maybeOldProps props (fun p -> p.Shapes)

            shapesDelta
            |> List.iter (fun shapeDelta ->
                match shapeDelta with
                | ListItemDelta.Added shape ->
                    let jsShape =
                        createJsShape googleMaps.Api shape
                        |> updateJsShape googleMaps shape
                        |> addJsShape googleMaps.Map

                    existingJsShapes.Add(shape.Key, jsShape)
                | ListItemDelta.Updated (_oldShape, newShape) ->
                    match existingJsShapes.Get(newShape.Key) with
                    | Some jsShape ->
                        updateJsShape googleMaps newShape jsShape
                        |> ignore
                    | None ->
                        log.Warn("Shape with key {Key} determined to be updated but could not be found in existing shapes", newShape.Key)
                | ListItemDelta.Removed shape ->
                    match existingJsShapes.Get(shape.Key) with
                    | Some jsShape ->
                        jsShape
                        |> removeJsShape
                        |> ignore

                        existingJsShapes.Remove(shape.Key)
                        |> ignore
                    | None ->
                        ()
            )

            let maybeDirectionsDelta = propEqualityDelta maybeOldProps props (fun p -> p.Directions)

            maybeDirectionsDelta
            |> Option.iter (fun maybeDirections ->
                existingDirectionsJsShapes
                |> Seq.iter (removeJsShape >> ignore)

                existingDirectionsJsShapes.Clear()

                maybeDirections
                |> Option.iter (fun directions ->
                    promise {
                        let! shapesResult = setDirections googleMaps.Api googleMaps.Map googleMaps.DirectionsService directions

                        match shapesResult with
                        | Ok shapes ->
                            existingDirectionsJsShapes.AddRange(shapes)
                        | Error e ->
                            failwith (sprintf "Something went wrong: %A" e)
                    }
                    |> ignore
                )
            )

            if googleMaps.Map?getCenter() = JsUndefined && props.Position = MapPosition.Auto then
                // No center set yet and we want to set automatically
                let bounds = createNew googleMaps.Api?maps?LatLngBounds ()

                // Use all added markers to extend the bounds...
                existingJsMarkers.Values
                |> Seq.iter (fun jsMarker -> bounds?extend(jsMarker.Value?getPosition()))

                // ...and all shapes
                existingJsShapes.Values
                |> Seq.iter (fun jsShape -> jsShape.GetBoundaryPoints() |> Array.iter (fun point ->
                    bounds?extend(point))
                )

                googleMaps.Map?fitBounds(bounds)
                googleMaps.Map?panToBounds(bounds)

                // TODO: update OnCenterChanged listener, if any

        | None -> ()

    override this.ComponentWillReceiveProps (nextProps: Props) : unit =
        this.UpdateFromProps(Some this.props, nextProps)

and Actions(map: Map) =
    member _.OnMapAnchorLoaded (div: Browser.Types.Element) : unit =
        #if EGGSHELL_PLATFORM_IS_WEB
        map.Load(div) |> ignore
        #else
        // Keep the compiler happy
        ignore map
        ignore div

        // we need to prevent the import from being processed, fable-splitter (or metro) is apparently
        // dumb and doesn't know how to tree shake this module off since it's not used in native
        // and it also hoists the import to the top of the generated JS file, instead of keeping it
        // deeply nested.
        Noop
        #endif


let Make = makeConstructor<Map, _, _>
#else
type Map(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Map>("ThirdParty.Map.Components.Web.Map", _initialProps, Actions, hasStyles = true)

and Actions(_this: Map) =
    class end

let Make = makeConstructor<Map, _, _>
#endif

// Unfortunately necessary boilerplate
type Pstate = NoPstate
type Estate = NoEstate

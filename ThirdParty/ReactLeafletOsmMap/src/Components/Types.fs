[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.Types

open Fable.Core
open Fable.Core.JsInterop

open LibClient
open LibClient.JsInterop

type GeoLocation with
    member internal this.ToJs () : array<obj> =
        [|
            this.Lat |> float
            this.Lng |> float
        |]

    member this.ToLngLat () : array<obj> =
        [|
            this.Lng |> float
            this.Lat |> float
        |]


type LatLngBounds = {
    SouthWest: GeoLocation
    NorthEast: GeoLocation
}
with
    member internal this.ToJs () : array<obj> =
        [|
            this.SouthWest.ToJs ()
            this.NorthEast.ToJs ()
        |]

type LeafletLatLng =
    {
        lat: float
        lng: float
    }

type LeafletLatLngBounds =
    {
        _southWest: LeafletLatLng
        _northEast: LeafletLatLng
    }

type OsmMapStyle = {
    Width:  NonemptyString
    Height: NonemptyString
    ZIndex: int
}
with
    member internal this.ToJs () : obj =
        createObj [
            "height" ==> this.Height.Value
            "width"  ==> this.Width.Value
            "zIndex" ==> this.ZIndex
        ]

type PaneStyle = {
    ZIndex: int
}
with
    member internal this.ToJs () : obj =
        createObj [
            "zIndex" ==> this.ZIndex
        ]

type ControlPosition =
| TopLeft
| TopRight
| BottomLeft
| BottomRight
with
    member this.ToJs () : string =
        match this with
        | TopLeft     -> "topleft"
        | TopRight    -> "topright"
        | BottomLeft  -> "bottomleft"
        | BottomRight -> "bottomright"

type Point =
| Point of int * int
    with
        member private this.Value : int * int =
            match this with
            | Point (x, y) -> x, y

        member internal this.ToJs () : array<obj> =
            [|
                fst this.Value |> float
                snd this.Value |> float
            |]

type MarkerIcon = {
    IconUrl:       NonemptyString
    IconRetinaUrl: Option<NonemptyString>
    IconSize:      Point
    IconAnchor:    Point
}
with
    static member New (
        iconUrl:        NonemptyString,
        ?iconRetinaUrl: NonemptyString,
        ?iconSize:      Point,
        ?iconAnchor:    Point)
        : MarkerIcon =
        {
            IconUrl       = iconUrl
            IconRetinaUrl = iconRetinaUrl
            IconSize      = (iconSize     |> Option.defaultValue (Point (0, 0)))
            IconAnchor    = (iconAnchor   |> Option.defaultValue (Point (0, 0)))
        }

    member internal this.ToJs () : obj =
        createObjWithOptionalValues [
            "iconUrl"       ==!> this.IconUrl.Value
            "iconRetinaUrl" ==?> (this.IconRetinaUrl |> Option.map (fun x -> x.Value))
            "iconSize"      ==!> this.IconSize.ToJs ()
            "iconAnchor"    ==!> this.IconAnchor.ToJs ()
        ]

type TooltipDirection =
| Top
| Bottom
| Right
| Left
| Center
| Auto
with
    member internal this.ToJs () : string =
        match this with
        | Top    -> "top"
        | Bottom -> "bottom"
        | Right  -> "right"
        | Left   -> "left"
        | Center -> "center"
        | Auto   -> "auto"

type PolygonPositions =
| Polygon      of seq<GeoLocation>
| MultiPolygon of seq<seq<GeoLocation>>
with
    member internal this.ToJs () : array<obj> =
        match this with
        | Polygon      x -> [| x |> Seq.map (fun x -> x.ToJs ()) |> Array.ofSeq |]
        | MultiPolygon x -> [| x |> Seq.map (fun x -> x |> Seq.map (fun x -> x.ToJs ()) |> Array.ofSeq) |> Array.ofSeq |]

    member this.ToJsWithInvertedCoords () : array<obj> =
        match this with
        | Polygon      x -> [| x |> Seq.map (fun x -> x.ToLngLat ()) |> Array.ofSeq |]
        | MultiPolygon x -> [| x |> Seq.map (fun x -> x |> Seq.map (fun x -> x.ToLngLat ()) |> Array.ofSeq) |> Array.ofSeq |]

type ILeafletMapEvent =
    abstract member ``type``:       string
    abstract member popup:          obj
    abstract member target:         obj
    abstract member sourceTarget:   obj
    abstract member propagatedFrom: obj
    abstract member layer:          obj

type ILeafletMouseEvent =
    inherit ILeafletMapEvent
    abstract member latlng:         obj
    abstract member containerPoint: obj
    abstract member layerPoint:     obj
    abstract member originalEvent:  obj

type ILeafletResizeEvent =
    inherit ILeafletMapEvent
    abstract member newSize:        obj
    abstract member oldSize:        obj

type ILeafletKeyboardEvent =
    inherit ILeafletMapEvent
    abstract member originalEvent:  obj

type ILeafletDragEndEvent =
    inherit ILeafletMapEvent
    abstract member distance:       float

type ILeafletTooltipEvent =
    inherit ILeafletMapEvent
    abstract member tooltip:        obj

type ILeafletPopupEvent =
    inherit ILeafletMapEvent
    abstract member popup:          obj

type LeafletMapEventHandlerFn      = ILeafletMapEvent -> unit
type LeafletMouseEventHandlerFn    = ILeafletMouseEvent -> unit
type LeafletResizeEventHandlerFn   = ILeafletResizeEvent -> unit
type LeafletKeyboardEventHandlerFn = ILeafletKeyboardEvent -> unit
type LeafletDragEndEventHandlerFn  = ILeafletDragEndEvent -> unit
type LeafletTooltipEventHandlerFn  = ILeafletTooltipEvent -> unit
type LeafletPopupEventHandlerFn    = ILeafletPopupEvent -> unit

type LeafletEvent =
// Map Events
| ZoomLevelsChange of LeafletMapEventHandlerFn
| ZoomStart        of LeafletMapEventHandlerFn
| Zoom             of LeafletMapEventHandlerFn
| ZoomEnd          of LeafletMapEventHandlerFn
| MoveStart        of LeafletMapEventHandlerFn
| Move             of LeafletMapEventHandlerFn
| MoveEnd          of LeafletMapEventHandlerFn
| PreDrag          of LeafletMapEventHandlerFn
| DragStart        of LeafletMapEventHandlerFn
| Drag             of LeafletMapEventHandlerFn
| AutoPanStart     of LeafletMapEventHandlerFn
| ViewReset        of LeafletMapEventHandlerFn
| Load             of LeafletMapEventHandlerFn
| Unload           of LeafletMapEventHandlerFn
// Mouse Events
| Click            of LeafletMouseEventHandlerFn
| DblClick         of LeafletMouseEventHandlerFn
| MouseDown        of LeafletMouseEventHandlerFn
| MouseUp          of LeafletMouseEventHandlerFn
| MouseOver        of LeafletMouseEventHandlerFn
| MouseOut         of LeafletMouseEventHandlerFn
| MouseMove        of LeafletMouseEventHandlerFn
| ContextMenu      of LeafletMouseEventHandlerFn
| PreClick         of LeafletMouseEventHandlerFn
// Resize Events
| Resize           of LeafletResizeEventHandlerFn
// Keyboard Events
| KeyPress         of LeafletKeyboardEventHandlerFn
| KeyDown          of LeafletKeyboardEventHandlerFn
| KeyUp            of LeafletKeyboardEventHandlerFn
// Drag Events
| DragEnd          of LeafletDragEndEventHandlerFn
// Tooltip Events
| TooltipOpen      of LeafletTooltipEventHandlerFn
| TooltipClose     of LeafletTooltipEventHandlerFn
// Popup Events
| PopupOpen        of LeafletPopupEventHandlerFn
| PopupClose       of LeafletPopupEventHandlerFn
with
    member private this.ToJs () : (string * obj) =
        match this with
        | ZoomLevelsChange fn -> "zoomlevelschange" ==> fn
        | ZoomStart        fn -> "zoomstart"        ==> fn
        | Zoom             fn -> "zoom"             ==> fn
        | ZoomEnd          fn -> "zoomend"          ==> fn
        | MoveStart        fn -> "movestart"        ==> fn
        | Move             fn -> "move"             ==> fn
        | MoveEnd          fn -> "moveend"          ==> fn
        | PreDrag          fn -> "predrag"          ==> fn
        | DragStart        fn -> "dragstart"        ==> fn
        | Drag             fn -> "drag"             ==> fn
        | AutoPanStart     fn -> "autopanstart"     ==> fn
        | ViewReset        fn -> "viewreset"        ==> fn
        | Load             fn -> "load"             ==> fn
        | Unload           fn -> "unload"           ==> fn
        | Click            fn -> "click"            ==> fn
        | DblClick         fn -> "dblclick"         ==> fn
        | MouseDown        fn -> "mousedown"        ==> fn
        | MouseUp          fn -> "mouseup"          ==> fn
        | MouseOver        fn -> "mouseover"        ==> fn
        | MouseOut         fn -> "mouseout"         ==> fn
        | MouseMove        fn -> "mousemove"        ==> fn
        | ContextMenu      fn -> "contextmenu"      ==> fn
        | PreClick         fn -> "preclick"         ==> fn
        | Resize           fn -> "resize"           ==> fn
        | KeyPress         fn -> "keypress"         ==> fn
        | KeyDown          fn -> "keydown"          ==> fn
        | KeyUp            fn -> "keyup"            ==> fn
        | DragEnd          fn -> "dragend"          ==> fn
        | TooltipOpen      fn -> "tooltipopen"      ==> fn
        | TooltipClose     fn -> "tooltipclose"     ==> fn
        | PopupOpen        fn -> "popupopen"        ==> fn
        | PopupClose       fn -> "popupclose"       ==> fn

    static member internal ToJsObj (events: array<LeafletEvent>) : obj =
        events
        |> Array.map (fun x -> x.ToJs ())
        |> Array.ofSeq
        |> createObj

type EditOption =
| Edit
| Remove
with
    static member internal SetToJs(options: Set<EditOption>) : obj =
        createObj [
            "edit"   ==> options.Contains Edit
            "remove" ==> options.Contains Remove
        ]

type DrawOption =
| Polyline
| Polygon
| Rectangle
| Circle
| Marker
| CircleMarker
with
    static member internal SetToJs(options: Set<DrawOption>) : obj =
        createObj [
            "polyline"     ==> options.Contains Polyline
            "polygon"      ==> options.Contains Polygon
            "rectangle"    ==> options.Contains Rectangle
            "circle"       ==> options.Contains Circle
            "marker"       ==> options.Contains Marker
            "circlemarker" ==> options.Contains CircleMarker
        ]

type LeafletDrawEventFn = obj -> unit

type LeafletDrawEvent =
| Mounted   of LeafletDrawEventFn
| Created of LeafletDrawEventFn
| Edited  of LeafletDrawEventFn
| Deleted of LeafletDrawEventFn
    with
    member private this.ToJs () : (string * obj) =
        match this with
        | Mounted fn -> "onMounted" ==> fn
        | Created fn -> "onCreated" ==> fn
        | Edited  fn -> "onEdited"  ==> fn
        | Deleted fn -> "onDeleted" ==> fn

    static member internal ToJsObj (events: array<LeafletDrawEvent>) : array<string * obj> =
        events
        |> Array.map (fun x -> x.ToJs ())






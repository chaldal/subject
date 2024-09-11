[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.TileLayer

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components

#if EGGSHELL_PLATFORM_IS_WEB

let private TileLayerComp: obj -> ReactElement = JsInterop.import "TileLayer" "react-leaflet"
let private useMapEvents: obj -> obj           = JsInterop.import "useMapEvents" "react-leaflet"

type OsmMap with
    [<Component>]
    static member TileLayer (
        url:                string,
        ?attribution:       string,
        ?opacity:           float,
        ?maxNativeZoom:     int,
        ?maxZoom:           int,
        ?minZoom:           int,
        ?updateWhenIdle:    bool,
        ?updateWhenZooming: bool,
        ?eventHandlers:     array<LeafletEvent>
        )
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "url"               ==!> url
                "attribution"       ==!> (attribution |> Option.defaultValue """&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors""")
                "opacity"           ==?> opacity
                "maxNativeZoom"     ==?> maxNativeZoom
                "maxZoom"           ==?> maxZoom
                "minZoom"           ==?> minZoom
                "updateWhenIdle"    ==?> updateWhenIdle
                "updateWhenZooming" ==?> updateWhenZooming
                "eventHandlers"     ==?> (eventHandlers |> Option.map LeafletEvent.ToJsObj)
            ]

        let _ =
            eventHandlers
            |> Option.defaultValue Array.empty
            |> LeafletEvent.ToJsObj
            |> useMapEvents

        Fable.React.ReactBindings.React.createElement (TileLayerComp, wrappedProps, Seq.empty)

#endif
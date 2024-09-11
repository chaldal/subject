[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.CircleMarker

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components
open Fable.Core.JsInterop

#if EGGSHELL_PLATFORM_IS_WEB

let private CircleMarkerComp: obj -> ReactElement = JsInterop.import "CircleMarker" "react-leaflet"

type OsmMap with
    [<Component>]
    static member CircleMarker (
        center:        GeoLocation,
        radius:        int,
        ?key:          NonemptyString,
        ?color:        string,
        ?weight:       int,
        ?opacity:      float,
        ?fill:         bool,
        ?fillColor:    string,
        ?fillOpacity:  float,
        ?eventHandlers: array<LeafletEvent>,
        ?children:     ReactChildrenProp
        )
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "center"        ==!> center.ToJs ()
                "radius"        ==!> radius
                "key"           ==!> (key |> Option.map (fun x -> x.Value) |> Option.defaultValue (System.Guid.NewGuid().ToString()))
                "color"         ==?> color
                "weight"        ==?> weight
                "opacity"       ==?> opacity
                "fill"          ==?> fill
                "fillColor"     ==?> fillColor
                "fillOpacity"   ==?> fillOpacity
                "eventHandlers" ==?> (eventHandlers |> Option.map LeafletEvent.ToJsObj)
            ]

        Fable.React.ReactBindings.React.createElement (CircleMarkerComp, wrappedProps, (children |> Option.defaultValue Array.empty))

#endif




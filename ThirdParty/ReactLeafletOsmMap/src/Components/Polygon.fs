[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.Polygon

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components
open Fable.Core.JsInterop

#if EGGSHELL_PLATFORM_IS_WEB

let private PolygonComp: obj -> ReactElement = JsInterop.import "Polygon" "react-leaflet"

type OsmMap with
    [<Component>]
    static member Polygon (
        positions:      PolygonPositions,
        ?key:           NonemptyString,
        ?strock:        bool,
        ?color:         string,
        ?weight:        int,
        ?opacity:       float,
        ?fill:          bool,
        ?fillColor:     string,
        ?fillOpacity:   float,
        ?children:      ReactChildrenProp,
        ?eventHandlers: array<LeafletEvent>
        )
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "positions"     ==!> positions.ToJs()
                "key"           ==!> (key |> Option.map (fun x -> x.Value) |> Option.defaultValue (System.Guid.NewGuid().ToString()))
                "strock"        ==?> strock
                "color"         ==?> color
                "weight"        ==?> weight
                "opacity"       ==?> opacity
                "fill"          ==?> fill
                "fillColor"     ==?> fillColor
                "fillOpacity"   ==?> fillOpacity
                "eventHandlers" ==?> (eventHandlers |> Option.map LeafletEvent.ToJsObj)
            ]

        Fable.React.ReactBindings.React.createElement (PolygonComp, wrappedProps, (children |> Option.defaultValue Array.empty))

#endif
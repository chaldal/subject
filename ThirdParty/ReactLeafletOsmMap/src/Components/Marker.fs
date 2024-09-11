[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.Marker

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components
open Fable.Core.JsInterop

#if EGGSHELL_PLATFORM_IS_WEB

let private MarkerComp: obj -> ReactElement = JsInterop.import "Marker" "react-leaflet"
let private leaflet: obj                    = JsInterop.import "*" "leaflet"
let private useMapEvents: obj -> obj        = JsInterop.import "useMapEvents" "react-leaflet"

type OsmMap with
    [<Component>]
    static member Marker (
        position:       GeoLocation,
        icon:           MarkerIcon,
        ?key:           NonemptyString,
        ?eventHandlers: array<LeafletEvent>,
        ?children:      ReactChildrenProp
        )
        : ReactElement =
        let wrappedProps =
            let icon =
                createNew
                    leaflet?Icon
                    (icon.ToJs())

            createObjWithOptionalValues [
                "position"      ==!> position.ToJs()
                "icon"          ==!> icon
                "key"           ==!> (key |> Option.map (fun x -> x.Value) |> Option.defaultValue (System.Guid.NewGuid().ToString()))
                "eventHandlers" ==?> (eventHandlers |> Option.map LeafletEvent.ToJsObj)
            ]

        let _ =
            eventHandlers
            |> Option.defaultValue Array.empty
            |> LeafletEvent.ToJsObj
            |> useMapEvents

        Fable.React.ReactBindings.React.createElement (MarkerComp, wrappedProps, (children |> Option.defaultValue Array.empty))

#endif
[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.Tooltip

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components

#if EGGSHELL_PLATFORM_IS_WEB

let private TooltipComp: obj -> ReactElement = JsInterop.import "Tooltip" "react-leaflet"

type OsmMap with
    [<Component>]
    static member Tooltip (
        ?key:       string,
        ?position:  GeoLocation,
        ?direction: TooltipDirection,
        ?permanent: bool,
        ?sticky:    bool,
        ?opacity:   float,
        ?offset:    Point,
        ?children:  ReactChildrenProp)
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "key"       ==?> key
                "direction" ==!> (direction |> Option.defaultValue TooltipDirection.Auto |> fun x -> x.ToJs())
                "permanent" ==!> (permanent |> Option.defaultValue false)
                "sticky"    ==?> sticky
                "opacity"   ==?> opacity
                "offset"    ==?> (offset |> Option.map (fun x -> x.ToJs()))
                "position"  ==?> (position |> Option.map (fun x -> x.ToJs()))
            ]

        Fable.React.ReactBindings.React.createElement (TooltipComp, wrappedProps, (children |> Option.defaultValue Array.empty))

#endif
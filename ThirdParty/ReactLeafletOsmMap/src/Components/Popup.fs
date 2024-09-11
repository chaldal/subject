[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.Popup

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components

#if EGGSHELL_PLATFORM_IS_WEB

let private PopupComp: obj -> ReactElement = JsInterop.import "Popup" "react-leaflet"

type OsmMap with
    [<Component>]
    static member Popup (
        key:               string,
        ?position:         GeoLocation,
        ?offset:           Point,
        ?maxWidth:         int,
        ?minWidth:         int,
        ?maxHeight:        int,
        ?autoPan:          bool,
        ?autoPanPadding:   Point,
        ?keepInView:       bool,
        ?closeButton:      bool,
        ?autoClose:        bool,
        ?closeOnClick:     bool,
        ?closeOnEscapeKey: bool,
        ?children:         ReactChildrenProp)
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "key"              ==!> key
                "offset"           ==!> (offset   |> Option.defaultValue (Point (0, 7)) |> fun x -> x.ToJs())
                "position"         ==?> (position |> Option.map (fun x -> x.ToJs()))
                "maxWidth"         ==!> (maxWidth |> Option.defaultValue 300)
                "minWidth"         ==!> (minWidth |> Option.defaultValue 50)
                "maxHeight"        ==?> maxHeight
                "autoPan"          ==!> (autoPan          |> Option.defaultValue false)
                "autoPanPadding"   ==?> (autoPanPadding   |> Option.map (fun x -> x.ToJs()))
                "keepInView"       ==!> (keepInView       |> Option.defaultValue false)
                "closeButton"      ==!> (closeButton      |> Option.defaultValue true)
                "autoClose"        ==!> (autoClose        |> Option.defaultValue true)
                "closeOnClick"     ==!> (closeOnClick     |> Option.defaultValue true)
                "closeOnEscapeKey" ==!> (closeOnEscapeKey |> Option.defaultValue true)
            ]

        Fable.React.ReactBindings.React.createElement (PopupComp, wrappedProps, (children |> Option.defaultValue Array.empty))

#endif
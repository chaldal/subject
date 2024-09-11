[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.FullscreenControl

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components

#if EGGSHELL_PLATFORM_IS_WEB

let private FullscreenControlComp: obj -> ReactElement = JsInterop.import "FullscreenControl" "react-leaflet-fullscreen"

type OsmMap with
    [<Component>]
    static member FullscreenControl (
        ?position:              ControlPosition,
        ?title:                 string,
        ?titleCancel:           string,
        ?content:               string,
        ?forceSeparateButton:   bool,
        ?forcePseudoFullscreen: bool)
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "position"              ==?> (position |> Option.map (fun x -> x.ToJs()))
                "title"                 ==?> title
                "titleCancel"           ==?> titleCancel
                "content"               ==?> content
                "forceSeparateButton"   ==?> forceSeparateButton
                "forcePseudoFullscreen" ==?> forcePseudoFullscreen
            ]

        Fable.React.ReactBindings.React.createElement (FullscreenControlComp, wrappedProps, Seq.empty)

#endif
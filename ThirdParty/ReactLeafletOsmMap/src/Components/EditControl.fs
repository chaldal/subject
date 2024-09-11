[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.EditControl

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components
open Fable.Core.JsInterop

#if EGGSHELL_PLATFORM_IS_WEB

let private EditControlComp: obj -> ReactElement = JsInterop.import "EditControl" "react-leaflet-draw"

type OsmMap with
    [<Component>]
    static member EditControl (
        controlPosition:    ControlPosition,
        drawOptions:        Set<DrawOption>,
        ?editOptions:       Set<EditOption>,
        ?drawEventHandlers: array<LeafletDrawEvent>)
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "position" ==!> controlPosition.ToJs()
                "draw"     ==!> (drawOptions |> DrawOption.SetToJs)
                "edit"     ==!> (editOptions |> Option.defaultValue Set.empty |> EditOption.SetToJs)

                yield!
                    drawEventHandlers
                    |> Option.map
                        (fun events ->
                            events
                            |> LeafletDrawEvent.ToJsObj
                        )
                    |> Option.defaultValue (Array.empty)
                    |> Array.map Some
                    |> Array.toList
            ]

        Fable.React.ReactBindings.React.createElement (EditControlComp, wrappedProps, Array.empty)

#endif
[<AutoOpen>]
module ThirdParty.ReactLeafletOsmMap.Components.FeatureGroup

open Fable.Core
open Fable.React
open LibClient
open LibClient.JsInterop

open ThirdParty.ReactLeafletOsmMap.Components

#if EGGSHELL_PLATFORM_IS_WEB

let private FeatureGroupComp: obj -> ReactElement = JsInterop.import "FeatureGroup" "react-leaflet"

type OsmMap with
    [<Component>]
    static member FeatureGroup (
        ?key:                 string,
        ?onfeatureGroupReady: obj -> unit,
        ?children:            ReactChildrenProp)
        : ReactElement =
        let wrappedProps =
            createObjWithOptionalValues [
                "key"              ==?> key
                "ref"              ==?> onfeatureGroupReady
            ]

        Fable.React.ReactBindings.React.createElement (FeatureGroupComp, wrappedProps, (children |> Option.defaultValue Array.empty))

#endif
module ThirdParty.Map.Components.Native.ReactNativeMaps

// NOTE there are some project-specific tweaks here. Ideally they would sit in the
// project that uses this component, instead of the third party library, but I simply
// wanted to pull out the component as a sample third party wrapper, and didn't want
// to do a full cleanup refactoring.

open System
open LibClient
open Fable.Core.JsInterop
open ThirdParty.Map.Types

type Props = (* GenerateMakeFunction *) {
    Size:     Option<(* width *) int * (* height *) int>
    Zoom:     Option<int>
    Value:    LatLng
    OnChange: Option<LatLng> -> unit
    Ref:      IRefReactNativeMapView -> unit
}

#if EGGSHELL_PLATFORM_IS_WEB
let Make (_props: Props) (_children: array<Fable.React.ReactElement>) : Fable.React.ReactElement =
    failwith "Shouldn't be trying to run this on web"
#else
let Make =
    let mapView:    obj = importDefault "react-native-maps"
    let styleSheet: obj = import "StyleSheet" "react-native"

    let style: obj =
        styleSheet?create (
            createObj [
                "map" ==> createObj [
                   "position" ==> "absolute"
                   "left"     ==> 0
                   "right"    ==> 0
                   "top"      ==> 0
                   "bottom"   ==> 0
                ]
            ]
        )

    ThirdParty.wrapComponentTransformingProps<Props>
        mapView
        (fun (props: Props) ->
            let width, height = props.Size |> Option.getOrElse (1, 1)
            let ratio = float width / float height
            let zoom = props.Zoom |> Option.getOrElse 11 |> float
            let latitudeDelta = ratio / (Math.Pow(2., (zoom - 1.)) / 360.)
            let longitudeDelta = latitudeDelta * ratio

            let latitude, longitude = props.Value
            createObj [
                "children"      ==> props?children
                "initialRegion" ==> createObj [
                    "latitude"       ==> latitude
                    "longitude"      ==> longitude
                    "latitudeDelta"  ==> latitudeDelta
                    "longitudeDelta" ==> longitudeDelta
                ]
                "showsUserLocation" ==> true
                "style"          ==> style?map
                "provider"       ==> "google"
                "showsTraffic"   ==> true
                "onRegionChange" ==>
                    fun e ->
                        props.OnChange ((e?latitude, e?longitude) |> Some)
                "ref" ==>
                    fun (jsRef: JsInterop.JsNullable<IRefReactNativeMapView>) ->
                        jsRef.ToOption
                        |> Option.sideEffect (fun ref ->
                            props.Ref ref
                        )
            ]
        )
#endif
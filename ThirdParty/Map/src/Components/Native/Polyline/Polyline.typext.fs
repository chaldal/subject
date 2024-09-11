module ThirdParty.Map.Components.Native.Polyline

// NOTE there are some project-specific tweaks here. Ideally they would sit in the
// project that uses this component, instead of the third party library, but I simply
// wanted to pull out the component as a sample third party wrapper, and didn't want
// to do a full cleanup refactoring.

open ThirdParty.Map.Types
open LibClient
open Fable.Core.JsInterop
open LibClient.Services.ImageService

type Props = (* GenerateMakeFunction *) {
    Value: Polyline
}

#if EGGSHELL_PLATFORM_IS_WEB
let Make (_props: Props) (_children: array<Fable.React.ReactElement>) : Fable.React.ReactElement =
    failwith "Shouldn't be trying to run this on web"
#else
let Make =
    let polyline: obj = import "Polyline" "react-native-maps"

    ThirdParty.wrapComponentTransformingProps<Props>
        polyline
        (fun (props: Props) ->
            let coordinatesArray =
                props.Value.Path
                |> Array.map (
                        fun coordinate ->
                            createObj [
                                "latitude"  ==> fst coordinate
                                "longitude" ==> snd coordinate
                            ]
                )

            createObj [
                "coordinates"     ==> coordinatesArray
                "strokeWidth"     ==> props.Value.StrokeWeight
                "strokeColor"     ==> props.Value.StrokeColor
            ]
        )
#endif

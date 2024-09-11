module ThirdParty.Map.Components.Native.Circle

// NOTE there are some project-specific tweaks here. Ideally they would sit in the
// project that uses this component, instead of the third party library, but I simply
// wanted to pull out the component as a sample third party wrapper, and didn't want
// to do a full cleanup refactoring.

open ThirdParty.Map.Types
open LibClient
open Fable.Core.JsInterop
open LibClient.Services.ImageService

type Props = (* GenerateMakeFunction *) {
    Circle: Circle
}

let hexToRgba (maybeHex: Option<string>) (maybeAlpha: Option<float>) =
    let hex      = maybeHex   |> Option.defaultValue "#000000"
    let alpha    = maybeAlpha |> Option.defaultValue 1.0
    let cleanHex = if hex.StartsWith("#") then hex.Substring(1) else hex

    let r = System.Convert.ToInt32(cleanHex.Substring(0, 2), 16)
    let g = System.Convert.ToInt32(cleanHex.Substring(2, 2), 16)
    let b = System.Convert.ToInt32(cleanHex.Substring(4, 2), 16)

    "rgba(" + r.ToString() + ", " + g.ToString() + ", " + b.ToString() + ", " + alpha.ToString() + ")"

let strokeWidthFromStrokeWeight (maybeStrokeWeight: Option<float>) : int =
    maybeStrokeWeight
    |> Option.defaultValue 1.0
    |> int

#if EGGSHELL_PLATFORM_IS_WEB
let Make (_props: Props) (_children: array<Fable.React.ReactElement>) : Fable.React.ReactElement =
    failwith "Shouldn't be trying to run this on web"
#else
let Make =
    let circle: obj = import "Circle" "react-native-maps"

    ThirdParty.wrapComponentTransformingProps<Props>
        circle
        (fun (props: Props) ->
            createObj [
                "center" ==> createObj [
                    "latitude"  ==> fst props.Circle.Center
                    "longitude" ==> snd props.Circle.Center
                ]
                "radius"      ==> props.Circle.Radius
                "fillColor"   ==>
                    // The `Shape.Circle` type is used for both web and native libraries.
                    // `FillColor` is a hex string for web and an RGBA string for native.
                    // Additionally, using RGBA allows us to control the opacity directly in native.
                    hexToRgba props.Circle.FillColor props.Circle.FillOpacity
                "strokeColor" ==> props.Circle.StrokeColor
                "strokeWidth" ==> strokeWidthFromStrokeWeight props.Circle.StrokeWeight
            ]
        )
#endif
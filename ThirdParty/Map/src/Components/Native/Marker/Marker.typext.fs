module ThirdParty.Map.Components.Native.Marker

// NOTE there are some project-specific tweaks here. Ideally they would sit in the
// project that uses this component, instead of the third party library, but I simply
// wanted to pull out the component as a sample third party wrapper, and didn't want
// to do a full cleanup refactoring.

open ThirdParty.Map.Types
open LibClient
open Fable.Core.JsInterop
open LibClient.Services.ImageService

type Props = (* GenerateMakeFunction *) {
    Coordinate: LatLng
    Draggable:  bool
    Image:      ImageSource
}

#if EGGSHELL_PLATFORM_IS_WEB
let Make (_props: Props) (_children: array<Fable.React.ReactElement>) : Fable.React.ReactElement =
    failwith "Shouldn't be trying to run this on web"
#else
let Make =
    let marker: obj = import "Marker" "react-native-maps"

    ThirdParty.wrapComponentTransformingProps<Props>
        marker
        (fun (props: Props) ->
            createObj [
                "coordinate" ==> createObj [
                    "latitude"  ==> fst props.Coordinate
                    "longitude" ==> snd props.Coordinate
                ]
                "image" ==> props.Image.Url
                // TODO
                // Anchor is hard coded now
                // Take it from props and use that value

                "anchor" ==> createObj [
                    "x" ==> 0.5
                    "y" ==> 0.5
                ]
            ]
        )
#endif

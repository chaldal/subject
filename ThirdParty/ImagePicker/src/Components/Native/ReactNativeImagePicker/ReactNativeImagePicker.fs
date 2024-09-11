[<AutoOpen>]
module ThirdParty.ImagePicker.Components.ReactNativeImagePicker

open Fable.React
open ReactXP.Components
open ReactXP.Styles
open LibClient.Components
open LibClient.LocalImages
open Fable.Core.JsInterop
open LibClient.EggShellReact
open LibLifeCycleTypes.File

type Asset = {
    Base64:   string
    Uri:      string
    Width:    uint16
    Height:   uint16
    FileSize: int<B>
    Type:     string
    FileName: string
}

#if EGGSHELL_PLATFORM_IS_WEB
    type ImagePicker.Native with
        [<Component>]
        static member ReactNativeImagePicker (onImageSelect: Option<list<Asset>> -> Unit) =
            ignore onImageSelect
            nothing
#else
module private Styles =

    let photoSource = makeViewStyles {
        FlexDirection.Row
        JustifyContent.SpaceBetween
        AlignItems.Center
        AlignContent.Center
        width        200
        padding      10
        borderRadius 5
        borderWidth  1
        borderColor  (Color.Hex "#f16049")
    }

    let photoSourceText = makeTextStyles {
        FontWeight.Bold
        fontSize 14
        color    (Color.Hex "#f16049")
    }

    let photoSourceIcon = makeViewStyles {
        size 25 25
    }

    let view = makeViewStyles {
        AlignItems.Center
        JustifyContent.Center
        gap 5
    }

[<RequireQualifiedAccess>]
type ImagePickingSource =
| Gallary
| Camera

let private launchImageLibrary (_options: obj) (_callback: obj -> unit) : unit = import "launchImageLibrary" "react-native-image-picker"
let private launchCamera       (_options: obj) (_callback: obj -> unit) : unit = import "launchCamera" "react-native-image-picker"

let private onImageSelectTransformer (onImageSelect: Option<list<Asset>> -> Unit) (originalAssets: obj) : unit =
    let response =
        match originalAssets?didCancel with
        | true ->
            None
        | false ->
            originalAssets?assets
            |> Array.map (fun asset ->
                {
                    Base64   = asset?base64
                    Uri      = asset?uri
                    Width    = asset?width
                    Height   = asset?height
                    FileSize = asset?fileSize
                    Type     = asset?``type``
                    FileName = asset?fileName
                })
            |> List.ofArray
            |> Some

    onImageSelect response


type ImagePicker.Native with
    [<Component>]
    static member ReactNativeImagePicker (onImageSelect: Option<list<Asset>> -> Unit) =

        let selectImage  (source: ImagePickingSource) (_: LibClient.Input.ReactEvent.Action) : unit =
            let options : obj =
                !!{|
                    mediaType      = "photo"
                    includeBase64  = true
                    selectionLimit = 0
                |}

            match source with
            | ImagePickingSource.Gallary -> launchImageLibrary options (onImageSelectTransformer onImageSelect)
            | ImagePickingSource.Camera  -> launchCamera       options (onImageSelectTransformer onImageSelect)


        RX.View (styles = [| Styles.view |], children = [|
            RX.View(styles = [|Styles.photoSource|], children = [|
                LC.Text("Take Photo", styles = [|Styles.photoSourceText|])
                RX.Image (
                    styles = [|Styles.photoSourceIcon|],
                    size   = Image.Size.FromStyles,
                    source = localImage "/libs/ThirdParty/ImagePicker/images/camera.png"
                )
                LC.TapCapture (onPress = (fun e ->
                    selectImage ImagePickingSource.Camera e
                ))
            |])

            RX.View(styles = [|Styles.photoSource|], children = [|
                LC.Text("Photo Gallery", styles = [|Styles.photoSourceText|])
                RX.Image (
                    styles = [|Styles.photoSourceIcon|],
                    size   = Image.Size.FromStyles,
                    source = localImage "/libs/ThirdParty/ImagePicker/images/gallery.png"
                )
                LC.TapCapture (onPress = (fun e ->
                    selectImage ImagePickingSource.Gallary e
                ))
            |])
        |])
#endif
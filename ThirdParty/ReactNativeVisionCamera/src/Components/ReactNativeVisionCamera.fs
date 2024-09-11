module ThirdParty.ReactNativeVisionCamera.ReactNativeVisionCamera

open Fable.React
open Fable.Core
open Fable.Core.JsInterop
open ReactXP
open LibLifeCycleTypes.File


type CameraMode =
| Front
| Back
with
    member this.toString: string =
        match this with
        | Front -> "front"
        | Back  -> "back"
        
type CameraPermissionStatus =
| Granted
| Denied
with
    member this.toString: string =
        match this with
        | Granted -> "granted"
        | Denied  -> "denied"
    
    static member ofString (status: string): CameraPermissionStatus =
        match status with
        | "granted" -> Granted
        | _         -> Denied

type CameraError =
| CameraNotAvailable
| PermissionDenied
| FileNotFound
| UnknownError of NonemptyString

#if !EGGSHELL_PLATFORM_IS_WEB
let private StyleSheet:                      obj = import "StyleSheet"      "react-native"
let private Camera:                          obj = import "Camera"          "react-native-vision-camera"
let private useCameraDevice (_options: obj): obj = import "useCameraDevice" "react-native-vision-camera"
let private useCodeScanner (_callback: obj): obj = import "useCodeScanner"  "react-native-vision-camera"
let public useCameraFormat (_device: obj) (_filter: obj): obj = import "useCameraFormat" "react-native-vision-camera"

let RNFetchBlob: obj = import "default" "rn-fetch-blob"

let requestCameraPermission (): Async<CameraPermissionStatus> =
    async {
        let! status = Camera?requestCameraPermission() |> Async.AwaitPromise
        let statusString = status :> obj :?> string
        return CameraPermissionStatus.ofString statusString
    }

let getCameraDevice (mode: CameraMode) : obj =
    useCameraDevice mode.toString
    
let getAbsoluteFillStyleSheet () : obj =
    StyleSheet?absoluteFill


let createCamera (props: obj) : ReactElement =
    ReactBindings.React.createElement(Camera, props, [])

let capturePhoto (cameraRef: obj) (callback: File -> unit) (onError: CameraError -> unit) : unit =
    promise {
        try
            if isNull cameraRef then
                onError CameraNotAvailable
            else
                let! photoFile = cameraRef?takePhoto()
                if isNull photoFile then
                    onError CameraNotAvailable
                else
                    let filePath: string =
                        match Runtime.platform with
                        | Native NativePlatform.IOS -> (string photoFile?path).Replace ("file://", "")
                        | Native NativePlatform.Android -> photoFile?path
                        | _ -> failwith "Unsupported platform"

                    match NonemptyString.ofString filePath with
                    | None ->
                        onError FileNotFound
                    | Some _ ->
                        let! (fileData: string) =
                            RNFetchBlob?fs?readFile(filePath, "base64")

                        let byteSize = asB fileData.Length
                        let file: File = {
                            MimeType = MimeType.ofString "image/png" |> Option.get
                            Data     = FileData.Base64 (fileData, byteSize)
                        }

                        callback file
        with
        | ex ->
            $"Failed to capture photo. Error: {ex.Message}" |> NonemptyString.ofLiteral |> UnknownError |> onError
    }
    |> Promise.start

let qrCodeScanner (callback: string -> unit) : ReactElement =
    let codeScanner: obj =
        useCodeScanner
            {|
                codeTypes     = [| "qr" |]
                onCodeScanned =
                    fun (codes : obj) ->
                        let codesArray = codes :?> obj[]
                        if codesArray?length > 0 then
                            let scannedData: string = codesArray[0]?value
                            callback scannedData
            |}

    let props : obj =
        {|
            device      = getCameraDevice CameraMode.Back
            isActive    = true
            codeScanner = codeScanner
            style       = StyleSheet?absoluteFill
            resizeMode  = "cover"
        |}

    createCamera props

#endif
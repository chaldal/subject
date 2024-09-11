module ThirdParty.ImagePicker.Components.Native.ImagePicker

open LibClient
open LibLifeCycleTypes.File
open Fable.Core
open ThirdParty.ImagePicker.Components.ReactNativeImagePicker
open ReactXP.Styles

type SelectionMode = LibClient.Components.Input.File.SelectionMode
let ReplacedExisting = SelectionMode.ReplacedExisting
let AppendToExisting = SelectionMode.AppendToExisting

type Props = (* GenerateMakeFunction *) {
    Value:         list<File>
    Validity:      InputValidity
    OnChange:      Result<list<File>, string> -> unit
    MaxFileCount:  Positive.PositiveInteger option // defaultWithAutoWrap None
    MaxFileSize:   int<KB> option                  // defaultWithAutoWrap None
    ShowPreview:   bool                            // default true
    SelectionMode: SelectionMode                   // default ReplacedExisting
    styles:        array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    InternalValidity: InputValidity
}

[<Global>]
let private atob (_encodedString: string): string = jsNative

[<Global>]
let private Uint8Array (_length: int): obj = jsNative

type ImagePicker(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, ImagePicker>("ThirdParty.ImagePicker.Components.Native.ImagePicker", _initialProps, Actions, hasStyles = true)

    override this.GetInitialEstate (_: Props) : Estate = {
        InternalValidity = InputValidity.Valid
    }

    override this.ComponentWillReceiveProps (_: Props) : unit =
        this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Valid })

    member this.TryAssetToFile (maybeMaxFileSize: Option<int<KB>>) (asset: Asset) : Result<File, string> =
        match (MimeType.ofString asset.Type, maybeMaxFileSize) with
        | (None, _) -> Error (sprintf "Unknown file type: %s" asset.Type)
        | (_, Some maxFileSize) when asset.FileSize > kBToB maxFileSize -> Error "File is too large"
        | (Some mimeType, _) ->
            {
                MimeType = mimeType
                Data     = FileData.Base64 (asset.Base64, asset.FileSize)
            }
            |> Ok

    member this.LoadFiles (assets: list<Asset>) : unit =
        async {
            this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Valid })

            let mappedResults =
                assets
                |> List.map (this.TryAssetToFile this.props.MaxFileSize)
                |> Result.liftList
                |> Result.mapError (fun errors ->
                    let invalidReasons = String.concat "\n" errors
                    this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid invalidReasons })
                    invalidReasons
                )

            let allFiles =
                match (this.props.SelectionMode, mappedResults) with
                | (SelectionMode.AppendToExisting, Ok result) -> Ok (this.props.Value @ result)
                | _                                           -> mappedResults

            match (this.state.InternalValidity, allFiles, this.props.MaxFileCount) with
            | (InputValidity.Valid, Ok files, Some maxFileCount) when files.Length > maxFileCount.Value ->
                this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid "Too many files" })
            | (InputValidity.Valid, Ok _, _) ->
                this.props.OnChange allFiles
            | _ ->
                Noop

        } |> startSafely

and Actions(this: ImagePicker) =
    member _.SelectImage (maybeAssets: Option<list<Asset>>) : unit =
        match maybeAssets with
        | Some assets ->
            this.LoadFiles assets
        | None -> ()

let Make = makeConstructor<ImagePicker, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
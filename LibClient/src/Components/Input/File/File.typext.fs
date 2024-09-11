module LibClient.Components.Input.File

open LibClient
open Fable.Core
open Fable.Core.JsInterop

open LibLifeCycleTypes.File
type LibLifeCycleFile     = LibLifeCycleTypes.File.File
type LibLifeCycleFileData = LibLifeCycleTypes.File.FileData

type AcceptedType =
| FileNameExtension of string
| MimeType of MimeType
| AnyAudioFile
| AnyVideoFile
| AnyImageFile
with
    member this.Value : string =
        match this with
        | FileNameExtension value -> value
        | MimeType mimeType       -> mimeType.Value
        | AnyAudioFile            -> "audio/*"
        | AnyVideoFile            -> "video/*"
        | AnyImageFile            -> "image/*"

type SelectionMode =
| ReplacedExisting
| AppendToExisting

type Props = (* GenerateMakeFunction *) {
    Value:         list<LibLifeCycleFile>
    Validity:      InputValidity
    OnChange:      Result<list<LibLifeCycleFile>, string> -> unit
    MaxFileCount:  Positive.PositiveInteger option // defaultWithAutoWrap None
    MaxFileSize:   int<KB> option                  // defaultWithAutoWrap None
    AcceptedTypes: Set<AcceptedType>               // default Set.empty
    SelectionMode: SelectionMode                   // default ReplacedExisting
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    InternalValidity: InputValidity
}

// NOTE newer Fable has Uint8Array support, but our version doesn't, so we hack
[<Emit("Array.from(new Uint8Array($0))")>]
let private arrayBufferToBytesArray (_arrayBuffer: Fable.Core.JS.ArrayBuffer) : byte[] = jsNative

type File(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, File>("LibClient.Components.Input.File", _initialProps, Actions, hasStyles = true)

    override this.GetInitialEstate (_: Props) : Estate = {
        InternalValidity = InputValidity.Valid
    }

    override this.ComponentWillReceiveProps (_: Props) : unit =
        this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Valid })

    member this.LoadFile (file: Browser.Types.File) : Async<Result<LibLifeCycleFile, string>> =
        let deferred = LibLangFsharp.Deferred<Result<LibLifeCycleFile, string>>()

        match (MimeType.ofString file.``type``, this.props.MaxFileSize) with
        | (None, _) -> deferred.Resolve (Error $"Unknown file type: %s{file.``type``}")
        | (_, Some maxFileSize) when (asB file.size) > (kBToB maxFileSize) -> deferred.Resolve (Error "File is too large")
        | (Some mimeType, _) ->
            let reader = Browser.Dom.FileReader.Create()
            reader.onload <- (fun event ->
                let arrayBuffer: JS.ArrayBuffer = event.target?result
                let bytes = arrayBufferToBytesArray arrayBuffer
                let result: LibLifeCycleFile = {
                    MimeType = mimeType
                    Data     = LibLifeCycleFileData.Bytes bytes
                }
                deferred.Resolve (Ok result)
            )
            reader.readAsArrayBuffer file

        deferred.Value

    member this.LoadFiles (files: seq<Browser.Types.File>) : unit =
        async {
            this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Valid })

            let! results =
                files
                |> Seq.map this.LoadFile
                |> Async.Parallel

            let mappedResults =
                Result.liftList (List.ofArray results)
                |> Result.mapError (fun errors ->
                    let invalidReasons = String.concat "\n" errors
                    this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid invalidReasons })
                    invalidReasons
                )

            let allFiles =
                match (this.props.SelectionMode, mappedResults) with
                | (AppendToExisting, Ok result) -> Ok (this.props.Value @ result)
                | _                          -> mappedResults

            match (this.state.InternalValidity, allFiles, this.props.MaxFileCount) with
            | (InputValidity.Valid, Ok files, Some maxFileCount) when files.Length > maxFileCount.Value ->
                this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid "Too many files" })
            | (InputValidity.Valid, Ok _, _) ->
                this.props.OnChange allFiles
            | _ ->
                Noop

        } |> startSafely

and Actions(this: File) =
    member _.OnSelectPress (maybeInput: Option<Browser.Types.Element>) (_e: ReactEvent.Action) : unit =
        maybeInput |> Option.sideEffect (fun input -> input?click())

    member _.OnInputInitialize (input: Browser.Types.Element) : unit =
        let fileInput = input :> obj :?> Browser.Types.HTMLInputElement
        input.addEventListener("change", fun _ ->
            seq {0 .. fileInput.files.length - 1} |> Seq.map (fun i ->
                fileInput.files.[i]
            )
            |> this.LoadFiles
        )

    member _.OnDropZoneInitialize (div: Browser.Types.Element) : unit =
        div.addEventListener("dragover", fun e ->
            // preventing browser default behaviour, which is to open the file
            e.preventDefault()
        )

        div.addEventListener("drop", fun e ->
            e.preventDefault()
            let maybeDataTransfer: Option<Browser.Types.DataTransfer> = e?dataTransfer
            maybeDataTransfer |> Option.sideEffect (fun dataTransfer ->
                let files =
                    if not (isNullOrUndefined dataTransfer.items) then
                        seq {0 .. dataTransfer.items.length - 1} |> Seq.filterMap (fun i ->
                            let candidate = dataTransfer.items.[i]
                            match candidate.kind with
                            | "file" -> Some (candidate.getAsFile())
                            | _      -> None
                        )
                    elif not (isNullOrUndefined dataTransfer.files) then
                        seq {0 .. dataTransfer.files.length - 1} |> Seq.map (fun i ->
                            dataTransfer.files.[i]
                        )
                    else
                        Seq.empty

                files |> this.LoadFiles
            )
        )

let Make = makeConstructor<File, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

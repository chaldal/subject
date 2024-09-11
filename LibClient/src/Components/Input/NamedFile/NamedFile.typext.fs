module LibClient.Components.Input.NamedFile

open LibClient
open Fable.Core
open Fable.Core.JsInterop

open LibLifeCycleTypes.File
type LibLifeCycleNamedFile = LibLifeCycleTypes.File.NamedFile
type LibLifeCycleFileData  = LibLifeCycleTypes.File.FileData

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
    Value:            list<LibLifeCycleNamedFile>
    Validity:         InputValidity
    OnChange:         Result<list<LibLifeCycleNamedFile>, string> -> unit
    MaxFileCount:     Positive.PositiveInteger option // defaultWithAutoWrap None
    MaxFileSize:      int<KB> option                  // defaultWithAutoWrap None
    MaxTotalFileSize: int<KB> option                  // defaultWithAutoWrap None
    AcceptedTypes:    Set<AcceptedType>               // default Set.empty
    SelectionMode:    SelectionMode                   // default ReplacedExisting
    key:              string option                   // defaultWithAutoWrap JsUndefined
}

type Estate = {
    InternalValidity: InputValidity
}

// NOTE newer Fable has Uint8Array support, but our version doesn't, so we hack
[<Emit("Array.from(new Uint8Array($0))")>]
let private arrayBufferToBytesArray (_arrayBuffer: Fable.Core.JS.ArrayBuffer) : byte[] = jsNative

let constrainMessage (maxFileCount: Option<Positive.PositiveInteger>) (maxFileSize: Option<int<KB>>) (maxTotalFileSize: Option<int<KB>>) =
    [
        maxFileCount
        |> Option.map (fun maxFileCount -> $"Up to {maxFileCount.Value} files.")

        maxFileSize
        |> Option.map (fun maxFileSize -> $"{kBToMB maxFileSize} MB per file.")

        maxTotalFileSize
        |> Option.map (fun maxTotalFileSize -> $"{kBToMB maxTotalFileSize} MB total.")
    ]
    |> Seq.choose id
    |> String.concat " "
    |> NonemptyString.ofString

let getPrintableAsciiChars (input: string) : string =
    input
    |> Seq.filter (fun c -> c >= ' ' && c <= '~') // printable ascii range is 32 to 126
    |> System.String.Concat

type File(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, File>("LibClient.Components.Input.NamedFile", _initialProps, Actions, hasStyles = true)

    override this.GetInitialEstate (_: Props) : Estate = {
        InternalValidity = InputValidity.Valid
    }

    override this.ComponentWillReceiveProps (_: Props) : unit =
        this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Valid })

    member this.LoadFile (file: Browser.Types.File) : Async<Result<LibLifeCycleNamedFile, string>> =
        let deferred = LibLangFsharp.Deferred<Result<LibLifeCycleNamedFile, string>>()

        match (MimeType.ofString file.``type``, this.props.MaxFileSize) with
        | (None, _) -> deferred.Resolve (Error (sprintf "Unknown file type: %s" file.``type``))
        | (_, Some maxFileSize) when (asB file.size) > (kBToB maxFileSize) -> deferred.Resolve (Error $"{file.name} is too large")
        | (Some mimeType, _) ->
            let reader = Browser.Dom.FileReader.Create()
            reader.onload <- (fun event ->
                let arrayBuffer: JS.ArrayBuffer = event.target?result
                let bytes = arrayBufferToBytesArray arrayBuffer
                let result: LibLifeCycleNamedFile = {
                    Name = file.name |> getPrintableAsciiChars |> NonemptyString.ofStringWithDefault "Untitled"
                    File = {
                        MimeType = mimeType
                        Data     = LibLifeCycleFileData.Bytes bytes
                    }
                }
                deferred.Resolve (Ok result)
            )
            reader.readAsArrayBuffer file

        deferred.Value

    member this.LoadFiles (files: seq<Browser.Types.File>) : unit =
        let totalExistingFileSize =
            this.props.Value
            |> Seq.map (fun file -> file.File.Bytes.Length)
            |> Seq.fold (fun acc size -> acc + size) 0

        let totalFileSize =
            files
            |> Seq.map (fun file -> file.size)
            |> Seq.fold (fun a b -> a + b) 0

        match this.props.MaxFileCount, this.props.MaxTotalFileSize with
        | Some maxFileCount, _ when files |> Seq.length > maxFileCount.Value ->
            this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid "Too many files" })
        | _,  Some maxTotalFileSize when (asB totalFileSize) > (kBToB maxTotalFileSize) ->
            this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid "Total file size limit exceeded" })
        | _,  Some maxTotalFileSize when this.props.SelectionMode = AppendToExisting && ((asB totalFileSize) + (asB totalExistingFileSize)) > (kBToB maxTotalFileSize) ->
            this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid "Total file size limit exceeded" })
        | _ ->
            async {
                let! results =
                    files
                    |> Seq.map this.LoadFile
                    |> Async.Parallel

                let mappedResults =
                    Result.liftList (List.ofArray results)
                    |> Result.mapError (fun errors ->
                        let invalidReasons = String.concat "\n" errors
                        invalidReasons
                    )

                let allFiles =
                    match (this.props.SelectionMode, mappedResults) with
                    | AppendToExisting, Ok result -> Ok (this.props.Value @ result)
                    | _                           -> mappedResults

                match allFiles with
                | Ok files ->
                    this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Valid })
                    this.props.OnChange (Ok files)
                | Error reason ->
                    this.SetEstate(fun estate _ -> { estate with InternalValidity = InputValidity.Invalid reason })

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

        div.addEventListener("paste", fun e ->
            e.preventDefault()

            let clipboardItems = (e :?> Browser.Types.ClipboardEvent).clipboardData.items

            if not (isNullOrUndefined clipboardItems) then
                [ 0 .. clipboardItems.length - 1 ]
                |> Seq.filterMap (fun i ->
                    let item = clipboardItems.[i]
                    if item.kind = "file" then
                        item.getAsFile() |> Some
                    else
                        None
                )
                |> this.LoadFiles
        )

let Make = makeConstructor<File, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

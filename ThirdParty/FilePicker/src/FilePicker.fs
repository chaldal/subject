[<AutoOpen>]
module ThirdParty.FilePicker.Components.Constructors

open Fable.React
open Fable.Core
open Fable.Core.JsInterop
open LibClient
open LibClient.Components
open LibClient.Components.Input.NamedFile
open LibLifeCycleTypes.File
open ReactXP.Components
open ReactXP.Styles

#if !EGGSHELL_PLATFORM_IS_WEB
module private Styles =
    let view =
        makeViewStyles {
            padding   5
            marginTop 10
            AlignItems.Center
        }

    let textCenter = makeTextStyles {
        TextAlign.Center
    }

    let dragAndDropMessage = makeViewStyles { marginTop 20 }

    let invalidReason =
        makeTextStyles {
            TextAlign.Center
            color Color.DevRed
        }

    let infoMessage = makeTextStyles { color Color.DevOrange }

    let messageContainer = makeViewStyles { marginTop 10 }

    let selectFileButton = makeViewStyles { maxWidth 236 }

type DocumentPickerResult =
    { ``type``: string
      fileCopyUri: string
      name: string
      size: int }

[<Import("readFile", "react-native-fs")>]
let private readFile (_filePath: string, _enCoding: string) : JS.Promise<string> = jsNative
#endif


type FilePicker =
    [<Component>]
    static member Base
        (
            value:             List<NamedFile>,
            validity:          InputValidity,
            onChange:          Result<List<NamedFile>, string> -> unit,
            ?maxFileCount:     Positive.PositiveInteger,
            ?maxFileSize:      int<KB>,
            ?maxTotalFileSize: int<KB>,
            ?selectionMode:    SelectionMode,
            ?acceptedTypes:    Set<AcceptedType>,
            ?key:              string
        ) =
        #if EGGSHELL_PLATFORM_IS_WEB
        FilePicker.Web (
            value                  = value,
            validity               = validity,
            onChange               = onChange,
            ?maybeMaxFileCount     = maxFileCount,
            ?maybeMaxFileSize      = maxFileSize,
            ?maybeMaxTotalFileSize = maxTotalFileSize,
            ?maybeSelectionMode    = selectionMode,
            ?maybeAcceptedTypes    = acceptedTypes,
            ?key                   = key
        )
        #else
        FilePicker.Native (
            value                  = value,
            validity               = validity,
            onChange               = onChange,
            ?maybeMaxFileCount     = maxFileCount,
            ?maybeMaxFileSize      = maxFileSize,
            ?maybeMaxTotalFileSize = maxTotalFileSize,
            ?maybeSelectionMode    = selectionMode,
            ?maybeAcceptedTypes    = acceptedTypes,
            ?key                   = key
        )
        #endif

    #if EGGSHELL_PLATFORM_IS_WEB
    [<Component>]
    static member Web
        (
            value:                  List<NamedFile>,
            validity:               InputValidity,
            onChange:               Result<List<NamedFile>, string> -> unit,
            ?maybeMaxFileCount:     Positive.PositiveInteger,
            ?maybeMaxFileSize:      int<KB>,
            ?maybeMaxTotalFileSize: int<KB>,
            ?maybeSelectionMode:    SelectionMode,
            ?maybeAcceptedTypes:    Set<AcceptedType>,
            ?key:                   string
        ) =
        LC.Input.NamedFile (
            value             = value,
            validity          = validity,
            onChange          = onChange,
            ?maxFileCount     = maybeMaxFileCount,
            ?maxFileSize      = maybeMaxFileSize,
            ?maxTotalFileSize = maybeMaxTotalFileSize,
            ?selectionMode    = maybeSelectionMode,
            ?acceptedTypes    = maybeAcceptedTypes,
            ?key              = key
        )

    #else
    [<Component>]
    static member Native
        (
            value:                  List<NamedFile>,
            validity:               InputValidity,
            onChange:               Result<List<NamedFile>, string> -> unit,
            ?maybeMaxFileCount:     Positive.PositiveInteger,
            ?maybeMaxFileSize:      int<KB>,
            ?maybeMaxTotalFileSize: int<KB>,
            ?maybeSelectionMode:    SelectionMode,
            ?maybeAcceptedTypes:    Set<AcceptedType>,
            ?key:                   string
        ) =
        let selectionMode = defaultArg maybeSelectionMode ReplacedExisting
        let internalValidity = Hooks.useState<InputValidity> InputValidity.Valid

        let pickDocument: obj -> JS.Promise<DocumentPickerResult[]> =
            import "pick" "react-native-document-picker"

        let fileTypes: obj = import "types" "react-native-document-picker"

        let loadFileFromPickerResult result =
            promise {
                let! base64string = readFile (result.fileCopyUri.Substring(7), "base64")

                match maybeMaxFileSize, MimeType.ofString result.``type`` with
                | Some maxFileSize, _ when (asB result.size) > (kBToB maxFileSize) ->
                    return Error $"{result.name} is too large"
                | _, None ->
                    return Error $"Unknown file type: {result.``type``}"
                | _, Some mimeType ->
                    return
                        Ok
                            { Name = result.name |> getPrintableAsciiChars |> NonemptyString.ofStringWithDefault "Untitled"
                              File =
                                { MimeType = mimeType
                                  Data = FileData.Base64(base64string, asB result.size) } }
            }

        let fileTypeFromAcceptedType acceptedType =
            match acceptedType with
            | FileNameExtension _ -> failwith "FileNameExtension is not supported by react native document picker"
            | MimeType mimeType   -> mimeType.Value
            | AnyVideoFile        -> fileTypes?video
            | AnyAudioFile        -> fileTypes?audio
            | AnyImageFile        -> fileTypes?images

        let acceptedFileTypes =
            match maybeAcceptedTypes with
            | None ->
                [| fileTypes?allFiles |]
            | Some acceptedTypes ->
                acceptedTypes
                |> Set.toArray
                |> Array.map fileTypeFromAcceptedType

        let pickFiles () =
            promise {
                let documentPickerProps: obj =
                    {| copyTo = "cachesDirectory"
                       allowMultiSelection = maybeMaxFileCount <> Some PositiveInteger.One
                       ``type`` = acceptedFileTypes |}

                let! results = pickDocument documentPickerProps

                let totalExistingFileSize =
                    value
                    |> Seq.map (fun file -> file.File.Bytes.Length)
                    |> Seq.fold (fun acc size -> acc + size) 0

                let totalFileSize =
                    results
                    |> Seq.map (fun file -> file.size)
                    |> Seq.fold (fun a b -> a + b) 0

                match maybeMaxFileCount, maybeMaxTotalFileSize with
                | Some maxFileCount, _ when results.Length > maxFileCount.Value ->
                    internalValidity.update (InputValidity.Invalid "Too many files")
                | _, Some maxTotalFileSize when (asB totalFileSize) > (kBToB maxTotalFileSize) ->
                    internalValidity.update (InputValidity.Invalid "Total file size limit exceeded")
                | _, Some maxTotalFileSize when selectionMode = AppendToExisting && ((asB totalFileSize) + (asB totalExistingFileSize)) > (kBToB maxTotalFileSize) ->
                    internalValidity.update (InputValidity.Invalid "Total file size limit exceeded")
                | _ ->
                    let! results =
                        results
                        |> Array.map loadFileFromPickerResult
                        |> Promise.Parallel

                    let mappedResults =
                        Result.liftList (List.ofArray results)
                        |> Result.mapError (String.concat "\n")

                    let allFiles =
                        match selectionMode, mappedResults with
                        | AppendToExisting, Ok result -> Ok (value @ result)
                        | _                           -> mappedResults

                    match allFiles with
                    | Ok files ->
                        internalValidity.update InputValidity.Valid
                        onChange (Ok files)
                    | Error reason ->
                        internalValidity.update (InputValidity.Invalid reason)
            }

        let pickFiles _e =
            try
                pickFiles ()
                |> Async.AwaitPromise
            with ex ->
                JS.console.error ex.Message
                Async.Of()
            |> startSafely

        element {
            RX.View (
                ?key = key,
                styles = [| Styles.view |],
                children =
                    [| LC.Button (
                           styles = [| Styles.selectFileButton |],
                           label = "Select File",
                           state = ButtonHighLevelStateFactory.MakeLowLevel(ButtonLowLevelState.Actionable pickFiles)
                       )

                       RX.View (
                           children =
                               [| LC.Text (
                                      styles = [| Styles.textCenter |],
                                      children =
                                          [|
                                             match constrainMessage maybeMaxFileCount maybeMaxFileSize maybeMaxTotalFileSize with
                                             | Some message ->
                                                 LC.Text message.Value
                                             | _ ->
                                                 noElement
                                          |]
                                  ) |]
                       )

                       RX.View (
                           styles = [| Styles.messageContainer |],
                           children =
                               [| RX.View
                                      [| match value.Length with
                                         | 1 ->
                                             LC.Text (
                                                 styles = [| Styles.textCenter; Styles.infoMessage |],
                                                 value = $"{value.Length} file selected"
                                             )
                                         | length when length > 1 ->
                                             LC.Text (
                                                 styles = [| Styles.textCenter; Styles.infoMessage |],
                                                 value = $"{value.Length} files selected"
                                             )
                                         | _ -> noElement |] |]
                       )

                       match internalValidity.current.InvalidReason |> Option.orElse validity.InvalidReason with
                       | None ->
                           noElement
                       | Some reason ->
                           RX.View [| LC.Text (styles = [| Styles.invalidReason |], value = reason) |]

                       if validity = InputValidity.Missing then
                           RX.View [| LC.Text (styles = [| Styles.invalidReason |], value = "This field is required") |] |]
            )
        }
    #endif
module LibRouter.RoutesSpec

open System.Text.RegularExpressions

open LibClient
open LibClient.Json
open LibClient.Dialogs
open LibClient.SystemDialogs

type NavigationRoute            = interface end
type NavigationResultlessDialog = interface end
type NavigationResultfulDialog  = interface end

type Location = {
    Path:  string
    Query: string
} with
    member this.ToString : string =
        match this.Query with
        | ""                              -> this.Path
        | query when query.StartsWith "?" -> this.Path + this.Query
        | _                               -> this.Path + "?" + this.Query

module Location =
    let ofPath (path: string) : Location = {
        Path  = path
        Query = ""
    }

type OpenDialogToken = OpenDialogToken of int

[<RequireQualifiedAccess>]
type NavigationDialog<'ResultlessDialog> =
| Resultful  of OpenDialogToken
| Resultless of OpenDialogToken * 'ResultlessDialog
| AdHoc      of OpenDialogToken
| System     of OpenDialogToken
with
    member this.Token : OpenDialogToken =
        match this with
        | Resultful  token
        | Resultless (token, _)
        | AdHoc      token
        | System     token -> token

type NavigationFrame<'Route, 'ResultlessDialog> = {
    Route:   'Route
    Dialogs: List<NavigationDialog<'ResultlessDialog>>
}

module NavigationFrame =
    let ofRoute (route: 'Route) = {
        Route   = route
        Dialogs = []
    }

    let route (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : 'Route =
        navigationFrame.Route

    let dialogs (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : List<NavigationDialog<'ResultlessDialog>> =
        navigationFrame.Dialogs

type [<RequireQualifiedAccess>] NavigationDirection =
| Forward
| Back
| Replace

type PreviousNavigationFrame<'Route, 'ResultlessDialog> = {
    Frame:     NavigationFrame<'Route, 'ResultlessDialog>
    Direction: NavigationDirection
}

type Conversions<'Route, 'ResultlessDialog> = {
    FromLocation: Location -> Option<NavigationFrame<'Route, 'ResultlessDialog>>
    ToLocation:   NavigationFrame<'Route, 'ResultlessDialog> -> Location
    AppBaseUrl:   string
}

type PartType =
| Integer
| Guid
| Json
| JsonBase64
| String
| NonemptyString

type ProcessedSpec<'Route, 'ResultlessDialog> = {
    PartTypes:          List<PartType>
    PartsToUrl:         List<string> -> string
    Regex:              Regex
    ParsedPartsToRoute: ParsedParts -> 'Route
    RouteToParts:       'Route -> Option<List<string>>
}

and ParsedParts (partTypes: List<PartType>, groups: GroupCollection, query: string) =
    // want: immutable O(1) indexed access DS
    let parts: array<obj> =
        partTypes
        |> Seq.mapi
            (fun i partType ->
                // first group is always the full match
                let rawValue = groups.Item(i + 1).Value
                match partType with
                | Integer ->
                    match System.Int32.ParseOption rawValue with
                    | Some value -> value :> obj
                    | None -> failwith (sprintf "We're supposed to be guaranteed an integer here given how parsing works below, so must be an implementation error: %s" rawValue)
                | Guid ->
                    match System.Guid.ParseOption rawValue with
                    | Some value -> value :> obj
                    | None -> failwith (sprintf "Parsing group %i, raw value was %s. Failed to parse Guid." i rawValue)
                | Json | String ->
                    (LibClient.JsInterop.decodeURIComponent rawValue) :> obj
                | JsonBase64 ->
                    rawValue :> obj
                | NonemptyString ->
                    (LibClient.JsInterop.decodeURIComponent rawValue |> NonemptyString.ofStringUnsafe (* safe because regex matches only nonempty *)) :> obj
            )
        |> Seq.toArray

    // This is to allow for the inline-ness of the GetFromJson function
    member _.Parts = parts

    member _.Get<'T>(index: int) : 'T =
        // we're fine letting it throw an exception, as this is only
        // likely to happen while a dev is adding a new route and is
        // in the middle of getting the conversion functions right
        parts.[index] :?> 'T

    // NOTE we're letting the user read the query string (and thus possibly
    // construct typed routes that rely on value from it) but not write it back,
    // since it would require some significant changes to the library. Read-only
    // access to it should be sufficient for the use case of taking in return-url
    // parameters, which is the use case we're exposing it for.
    member _.Query : string =
        query

    member inline this.GetFromJson<'T>(index: int) : 'T =
        let valueRaw = this.Parts.[index] :?> string
        let valueUrlDecoded = LibClient.JsInterop.decodeURIComponent valueRaw
        match Json.FromString<'T> valueUrlDecoded with
        | Ok value -> value
        | Error e ->
            failwith (sprintf "Failed to parse supposedly JSON-encoded URL part Raw(`%s`) UrlDecoded(`%s`) at index %i, error was %s" valueRaw valueUrlDecoded index e) // TODO do error handling in router properly

    member inline this.GetFromJson64<'T>(index: int) : 'T =
        let valueRaw = this.Parts.[index] :?> string
        let valueBase64Decoded = LibClient.Base64.decodeUrlSafe valueRaw
        match Json.FromString<'T> valueBase64Decoded with
        | Ok value -> value
        | Error e ->
            failwith (sprintf "Failed to parse supposedly JSON64-encoded URL part Raw(`%s`) Base64Decoded(`%s`) at index %i, error was %s" valueRaw valueBase64Decoded index e) // TODO do error handling in router properly

type Spec<'Route> = string * (ParsedParts -> 'Route) * ('Route -> Option<List<string>>)

[<RequireQualifiedAccess>]
type private UnencodableDialog<'ResultfulDialog> =
| Resultful of 'ResultfulDialog
| System    of SystemDialog
| AdHoc     of ((DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement)

type DialogsState<'ResultfulDialog>() =
    let mutable nextToken: int = 0
    let mutable unencodableDialogs: Map<int, UnencodableDialog<'ResultfulDialog>> = Map.empty

    member _.AddResultful (dialog: 'ResultfulDialog) : OpenDialogToken =
        let currToken = nextToken
        nextToken <- nextToken + 1
        unencodableDialogs <- unencodableDialogs.Add (currToken, UnencodableDialog.Resultful dialog)
        OpenDialogToken currToken

    member _.AddSystem (dialog: SystemDialog) : OpenDialogToken =
        let currToken = nextToken
        nextToken <- nextToken + 1
        unencodableDialogs <- unencodableDialogs.Add (currToken, UnencodableDialog.System dialog)
        OpenDialogToken currToken

    member _.AddResultless () : OpenDialogToken =
        let currToken = nextToken
        nextToken <- nextToken + 1
        OpenDialogToken currToken

    member _.AddAdHoc (closeToDialog: (DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement) : OpenDialogToken =
        let currToken = nextToken
        nextToken <- nextToken + 1
        unencodableDialogs <- unencodableDialogs.Add (currToken, UnencodableDialog.AdHoc closeToDialog)
        OpenDialogToken currToken

    member _.TryGetResultful (OpenDialogToken token: OpenDialogToken) : Option<'ResultfulDialog> =
        unencodableDialogs.TryFind token
        |> Option.bind (function
            | UnencodableDialog.Resultful value -> Some value
            | _ -> None
        )

    member _.TryGetSystem (OpenDialogToken token: OpenDialogToken) : Option<SystemDialog> =
        unencodableDialogs.TryFind token
        |> Option.bind (function
            | UnencodableDialog.System value -> Some value
            | _ -> None
        )

    member _.TryGetAdHoc (OpenDialogToken token: OpenDialogToken) : Option<(DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement> =
        unencodableDialogs.TryFind token
        |> Option.bind (function
            | UnencodableDialog.AdHoc value -> Some value
            | _ -> None
        )

    member _.RemoveStateFor (OpenDialogToken token: OpenDialogToken) : unit =
        unencodableDialogs <- unencodableDialogs.Remove token

    member _.Contains (OpenDialogToken token: OpenDialogToken) : bool =
        unencodableDialogs.ContainsKey token

type NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>() =
    let dialogsState: DialogsState<'ResultfulDialog> = DialogsState<'ResultfulDialog>()
    let mutable maybePreviousNavigationFrame: Option<PreviousNavigationFrame<'Route, 'ResultlessDialog>> = None

    member _.DialogsState = dialogsState

    member _.MaybePreviousNavigationFrame : Option<PreviousNavigationFrame<'Route, 'ResultlessDialog>> =
        maybePreviousNavigationFrame

    member _.Navigate (direction: NavigationDirection) (maybePreviousFrame: Option<NavigationFrame<'Route, 'ResultlessDialog>>) : unit =
        maybePreviousNavigationFrame <- maybePreviousFrame |> Option.map (fun frame -> { Frame = frame; Direction = direction })


// Need a real regex here eventually, lest route matching will be buggy
let private urlEncodedFragmentHackRegexSrc = "([^/]*)"
let private nonemptyUrlEncodedFragmentHackRegexSrc = "([^/]+)"
let private parameterRegex = Regex("\{[a-zA-Z0-9]+\}")
let private buildRegexAndPartTypes (pattern: string) : Regex * List<PartType> =
    // we could do away with only `accRegexSource` and no `remainingPattern`, but it felt a bit cleaner that way,
    // since it's only incidental that parameter placeholders and the regex that replaces them have no overlap textually.
    let rec helper = fun (accRegexSource: string) (accPartTypesRev: List<PartType>) (remainingPattern: string) ->
        let theMatch = parameterRegex.Match remainingPattern
        match theMatch.Success with
        | false -> (accRegexSource, accPartTypesRev)
        | true ->
            let (partType, partTypeRegexSource) =
                match theMatch.Value with
                | "{int}"            -> (Integer,        "([0-9]+)")
                | "{guid}"           -> (Guid,           "([0-9a-f\\-]+)")
                | "{json}"           -> (Json,           urlEncodedFragmentHackRegexSrc)
                | "{json64}"         -> (JsonBase64,     urlEncodedFragmentHackRegexSrc)
                | "{string}"         -> (String,         urlEncodedFragmentHackRegexSrc)
                | "{nonemptystring}" -> (NonemptyString, nonemptyUrlEncodedFragmentHackRegexSrc)
                | _ -> failwith (sprintf "Unknown URL parameter capture — %s" theMatch.Value)

            helper
                (parameterRegex.Replace(accRegexSource, partTypeRegexSource, 1))
                (partType :: accPartTypesRev)
                (parameterRegex.Replace(remainingPattern, "", 1))

    let (regexSource, partTypesRev) = helper pattern [] pattern

    let updatedRegexSource = "^/?" + regexSource + "$"
    (Regex updatedRegexSource, partTypesRev |> List.rev)

let private decodeDialogs<'ResultlessDialog, 'ResultfulDialog> (decoder: string -> Result<List<NavigationDialog<'ResultlessDialog>>, string>) (dialogsState: DialogsState<'ResultfulDialog>) (query: string) : List<NavigationDialog<'ResultlessDialog>> =
    let sanitizedQuery = if query.StartsWith "?" then query.Substring 1 else query

    sanitizedQuery.Split('&')
    |> Seq.tryFind (fun pair -> pair.StartsWith "dialogs=")
    |> Option.flatMap (fun pair ->
        pair.Substring("dialogs=".Length)
        |> LibClient.Base64.decodeUrlSafe
        |> decoder
        |> Result.toOption
    )
    |> Option.map (fun dialogs ->
        dialogs |> List.takeWhile (function
            | NavigationDialog.Resultless _    -> true
            | NavigationDialog.Resultful token
            | NavigationDialog.AdHoc token
            | NavigationDialog.System token -> dialogsState.Contains token
        )
    )
    |> Option.getOrElse []

let private encodeDialogs<'ResultlessDialog> (encoder: List<NavigationDialog<'ResultlessDialog>> -> string) (dialogs: List<NavigationDialog<'ResultlessDialog>>) : string =
    match dialogs with
    | [] -> ""
    | _  -> sprintf "dialogs=%s" (dialogs |> encoder |> LibClient.Base64.encodeUrlSafe)

let (* private but called from inline *) makeConversionsHelper<'Route, 'ResultlessDialog, 'ResultfulDialog>
    (dialogsEncoder: List<NavigationDialog<'ResultlessDialog>> -> string)
    (dialogsDecoder: string -> Result<List<NavigationDialog<'ResultlessDialog>>, string>)
    (appBaseUrl: string)
    (specs: List<Spec<'Route>>)
    (navigationState: NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>)
    : Conversions<'Route, 'ResultlessDialog> =
    let processedSpecs: List<ProcessedSpec<'Route, 'ResultlessDialog>> =
        specs
        |> List.map (fun (pattern, parsedPartsToRoute, routeToParts) ->
            let (regex, partTypes) = buildRegexAndPartTypes pattern

            let partsToUrl = fun (parts: List<string>) ->
                List.fold
                    (fun (acc) ((part, partType): string * PartType) ->
                        let encodedPart: string =
                            match partType with
                            | JsonBase64 -> LibClient.Base64.encodeUrlSafe part
                            | _          -> LibClient.JsInterop.encodeURIComponent part
                        parameterRegex.Replace(acc, encodedPart, 1)
                    )
                    pattern
                    (List.zip parts partTypes)

            {
                PartTypes          = partTypes
                PartsToUrl         = partsToUrl
                Regex              = regex
                ParsedPartsToRoute = parsedPartsToRoute
                RouteToParts       = routeToParts
            }
        )

    let fromLocation: Location -> Option<NavigationFrame<'Route, 'ResultlessDialog>> = fun (location: Location) ->
        let maybeRoute =
            match processedSpecs |> List.tryFind (fun currSpec -> currSpec.Regex.IsMatch location.Path) with
            | None ->
                Log.Warn $"Failed to parse route out of {location.Path}"
                None
            | Some currSpec ->
                let groups = currSpec.Regex.Match(location.Path).Groups
                Some (currSpec.ParsedPartsToRoute (ParsedParts(currSpec.PartTypes, groups, location.Query)))

        maybeRoute |> Option.map (fun route ->
            {
                Route   = route
                Dialogs = decodeDialogs dialogsDecoder navigationState.DialogsState location.Query
            }
        )

    let toLocation: NavigationFrame<'Route, 'ResultlessDialog> -> Location = fun (target: NavigationFrame<'Route, 'ResultlessDialog>) ->
        let maybeMatch =
            processedSpecs
            |> List.tryPick
                (fun currSpec ->
                    currSpec.RouteToParts target.Route
                    |> Option.map (fun parts -> (currSpec, parts))
                )

        let path =
            match maybeMatch with
            | None -> failwith (sprintf "No match url making function for target %O" target)
            | Some (spec, parts) -> spec.PartsToUrl parts

        {
            Path  = path
            Query = encodeDialogs dialogsEncoder target.Dialogs
        }

    {
        FromLocation = fromLocation
        ToLocation   = toLocation
        AppBaseUrl   = appBaseUrl
    }

let inline makeConversions<'Route, 'ResultlessDialog, 'ResultfulDialog>
    (appBaseUrl: string)
    (specs: List<Spec<'Route>>)
    (navigationState: NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>)
    : Conversions<'Route, 'ResultlessDialog> =

    let dialogsEncoder = Json.ToString<List<NavigationDialog<'ResultlessDialog>>>
    let dialogsDecoder = Json.FromString<List<NavigationDialog<'ResultlessDialog>>>
    makeConversionsHelper dialogsEncoder dialogsDecoder appBaseUrl specs navigationState

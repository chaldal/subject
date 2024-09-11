[<AutoOpen>]
module LibClient.Components.Input_Guid

open System
open Fable.React
open LibClient
open ReactXP.Styles

module private Helpers =
    let parseGuid (maybeNonemptyString: Option<NonemptyString>) : Result<Option<Guid>, string> =
        maybeNonemptyString
        |> Option.mapOrElse (Ok None)
            (fun maybeGuidString ->
                match maybeGuidString.Value |> Guid.ParseOption with
                | None -> Error "Invalid Guid"
                | Some guid -> Ok (Some guid)
            )

module LC =
    module Input =
        module GuidTypes =
            type Value = LibClient.Components.Input.ParsedText.Value<Guid>

            let parse (raw: Option<NonemptyString>) : Value =
                Value.OfRaw Helpers.parseGuid raw

            let wrap (value: Guid) : Value =
                Value.Wrap (
                    value,
                    value.ToString()
                )

            let empty = parse None

open LC.Input.GuidTypes

type LibClient.Components.Constructors.LC.Input with
    [<Component>]
    static member Guid (
        value:                Value,
        validity:             InputValidity,
        onChange:             Value -> unit,
        ?label:               string,
        ?styles:              array<ViewStyles>,
        ?placeholder:         string,
        ?prefix:              string,
        ?suffix:              InputSuffix,
        ?requestFocusOnMount: bool,
        ?tabIndex:            int,
        ?onKeyPress:          Browser.Types.KeyboardEvent -> unit,
        ?onEnterKeyPress:     ReactEvent.Keyboard -> unit,
        ?key:                 string
    ) : ReactElement =

        LC.Input.ParsedText (
            parse               = Helpers.parseGuid,
            ?styles             = styles,
            ?label              = label,
            value               = value,
            validity            = validity,
            onChange            = onChange,
            requestFocusOnMount = defaultArg requestFocusOnMount true,
            tabIndex            = defaultArg tabIndex 1,
            onEnterKeyPress     = defaultArg onEnterKeyPress (fun _ -> ()),
            ?placeholder        = placeholder,
            ?prefix             = prefix,
            ?suffix             = suffix,
            ?onKeyPress         = onKeyPress,
            ?key                = key
        )
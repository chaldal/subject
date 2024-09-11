module EvolutionCheckerLib

open CodecLib
open FSharpPlus
open FSharpPlus.Data

[<RequireQualifiedAccess>]
type JsonNodeShortForm = | JsonNodeShortForm of string

module JsonNode =
    let private veryShortForm (node: JsonNode) : string =
        match node with
        | JsonNode.Terminal terminalNode ->
            $"{terminalNode}"
        | JsonNode.Choice nodes ->
            $"Choice{nodes |> NonEmptyList.length}"
        | JsonNode.Result _ ->
            "Result"
        | JsonNode.Option _ ->
            "Option"
        | JsonNode.Array _ ->
            "Array"
        | JsonNode.Record container ->
            let summary = container.Keys |> String.concat ""
            $"Record{summary}"
        | JsonNode.Tuple nodes ->
            $"Tuple{nodes |> NonEmptyList.length}"
        | JsonNode.AnyOneOf _ ->
            "UnionCase"
        | JsonNode.Any _ ->
            "Any"
        | JsonNode.OptWithOption _ ->
            "OptWithOption"

    let ShortForm (node: JsonNode) : JsonNodeShortForm =
        let summaryVeryShortForm (nodes: NonEmptyList<JsonNode>) : string =
            nodes |> NonEmptyList.map veryShortForm |> NonEmptyList.toArray |> String.concat "|"
        match node with
        | JsonNode.Terminal terminalNode ->
            $"{terminalNode}"
        | JsonNode.Choice nodes ->
            $"Choice[{summaryVeryShortForm nodes}]"
        | JsonNode.Result (ok, error) ->
            $"Result[{veryShortForm ok}|{veryShortForm error}]"
        | JsonNode.Option node ->
            $"Option[{veryShortForm node}]"
        | JsonNode.Array node ->
            $"Array[{veryShortForm node}]"
        | JsonNode.Record container ->
            let summary = container |> Map.toList |> List.map (fun (key, node) -> $"{key}={veryShortForm node}") |> String.concat ";"
            $"Record[{summary}]"
        | JsonNode.Tuple nodes ->
            $"Tuple[{summaryVeryShortForm nodes}]"
        | JsonNode.AnyOneOf nodes ->
            $"UnionCase[{summaryVeryShortForm nodes}]"
        | JsonNode.Any _ ->
            "Any"
        | JsonNode.OptWithOption node ->
            $"OptWithOption[{veryShortForm node}]"
        |> JsonNodeShortForm.JsonNodeShortForm

[<RequireQualifiedAccess>]
type EvolutionError =
    | NoDecoderFound of JsonNodeShortForm: JsonNodeShortForm
    | NoDecoderForEncodedNullValue
    | TerminalNodeMismatch of {| Expected: TerminalJsonNode; Found : TerminalJsonNode |}
    | TerminalNodeParsingError of Found: TerminalJsonNode
    | KeyNotFoundInEncodedRecord of {| Expected: string; Found: List<string> |}
    | DecodeErrorForKey of key: string
    | CannotReplaceOptWithByReqWith of Key: string
    | TupleSizeMismatch of {| Expected: int; Found: int |}
    | ChoiceSizeMismatch of {| Expected: int; Found: int |}
    | CannotRemoveAnyDecoder

[<RequireQualifiedAccess>]
type EvolutionOk =
    | ExactMatch
    | Evolved

// check if all existing data can be decoded by any of the new decoder
let rec checkEvolutionCorrectness (callLevel: uint) (oldJsonNode: JsonNode) (newJsonNode: JsonNode) : Result<EvolutionOk, EvolutionError> =

    let checkAnyNewNodeCanEvolveOldNode (newNodes: NonEmptyList<JsonNode>) (oldNode: JsonNode) : Result<EvolutionOk, EvolutionError>=
        newNodes.Tail
        |> Seq.fold (fun res newNode ->
            res
            |> Result.bindError (fun _oldError ->
                checkEvolutionCorrectness (callLevel + 1u) oldNode newNode
            )
        ) (checkEvolutionCorrectness (callLevel + 1u) oldNode newNodes.Head)

    let checkAllOldNodeCanBeEvolved (oldNodes: NonEmptyList<JsonNode>) (oldNodeEvolutionChecker: JsonNode -> Result<EvolutionOk, EvolutionError>) =
        oldNodes.Tail
        |> Seq.fold (fun res oldNode ->
            res
            |> Result.bind (fun _ ->
                oldNodeEvolutionChecker oldNode
                |> Result.mapError (fun _ -> EvolutionError.NoDecoderFound (JsonNode.ShortForm oldNode))
            )
        ) (oldNodeEvolutionChecker oldNodes.Head
           |> Result.mapError (fun _ -> EvolutionError.NoDecoderFound (JsonNode.ShortForm oldNodes.Head)))

    let mapOkToEvolved = Result.map (fun _ -> EvolutionOk.Evolved)

    match oldJsonNode, newJsonNode with
    | oldJsonNode, newJsonNode when oldJsonNode = newJsonNode ->
        Ok EvolutionOk.ExactMatch
    // if new node is AnyOneOf type, check existing data can be decoded by any of the new decoder
    | JsonNode.AnyOneOf oldNodes, JsonNode.AnyOneOf newNodes ->
        // all old variant (union cases) are decode able by at least one new variant
        checkAllOldNodeCanBeEvolved oldNodes (checkAnyNewNodeCanEvolveOldNode newNodes)
        |> mapOkToEvolved

    | (JsonNode.Terminal _ as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.Choice _ as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.Result _ as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.Option _ as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.Array _ as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.Record _ as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.Tuple _ as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.Any as oldNode), JsonNode.AnyOneOf newNodes
    | (JsonNode.OptWithOption _ as oldNode), JsonNode.AnyOneOf newNodes ->
        checkAnyNewNodeCanEvolveOldNode newNodes oldNode
        |> mapOkToEvolved

    // if encoded node is AnyOneOf type, check if all encoded data can be decoded by new decoder
    | JsonNode.AnyOneOf oldNodes, (JsonNode.Terminal _ as newNode)
    | JsonNode.AnyOneOf oldNodes, (JsonNode.Choice _ as newNode)
    | JsonNode.AnyOneOf oldNodes, (JsonNode.Result _ as newNode)
    | JsonNode.AnyOneOf oldNodes, (JsonNode.Option _ as newNode)
    | JsonNode.AnyOneOf oldNodes, (JsonNode.Array _ as newNode)
    | JsonNode.AnyOneOf oldNodes, (JsonNode.Record _ as newNode)
    | JsonNode.AnyOneOf oldNodes, (JsonNode.Tuple _ as newNode)
    | JsonNode.AnyOneOf oldNodes, (JsonNode.OptWithOption _ as newNode) ->
        // all old variant (union cases) are decode able by at least one new variant
        checkAllOldNodeCanBeEvolved oldNodes (fun oldNode -> checkEvolutionCorrectness (callLevel + 1u) oldNode newNode)
        |> mapOkToEvolved

    //  if new node is Any type, that means any existing data can be decoded by new decoder
    | JsonNode.Terminal _, JsonNode.Any
    | JsonNode.Choice _, JsonNode.Any
    | JsonNode.Result _, JsonNode.Any
    | JsonNode.Option _, JsonNode.Any
    | JsonNode.Array _, JsonNode.Any
    | JsonNode.Record _, JsonNode.Any
    | JsonNode.Tuple _, JsonNode.Any
    | JsonNode.AnyOneOf _, JsonNode.Any
    | JsonNode.Any _, JsonNode.Any
    | JsonNode.OptWithOption _, JsonNode.Any -> Ok EvolutionOk.Evolved

    // if encoded node is Option type, check if new decoder can can decode both null and non-null data
    | JsonNode.Option oldNode, JsonNode.Option newNode
    | JsonNode.OptWithOption oldNode, JsonNode.Option newNode
    | JsonNode.Option oldNode, JsonNode.OptWithOption newNode
    | JsonNode.OptWithOption oldNode, JsonNode.OptWithOption newNode ->
        // new node support null value
        checkEvolutionCorrectness (callLevel + 1u) oldNode newNode
        |> mapOkToEvolved

    | JsonNode.Option _, JsonNode.Terminal _
    | JsonNode.Option _, JsonNode.Choice _
    | JsonNode.Option _, JsonNode.Result _
    | JsonNode.Option _, JsonNode.Array _
    | JsonNode.Option _, JsonNode.Record _
    | JsonNode.Option _, JsonNode.Tuple _
    | JsonNode.OptWithOption _, JsonNode.Terminal _
    | JsonNode.OptWithOption _, JsonNode.Choice _
    | JsonNode.OptWithOption _, JsonNode.Result _
    | JsonNode.OptWithOption _, JsonNode.Array _
    | JsonNode.OptWithOption _, JsonNode.Record _
    | JsonNode.OptWithOption _, JsonNode.Tuple _ ->
        // new node does not support null value
        Error EvolutionError.NoDecoderForEncodedNullValue

    // if new decoder is Optional, considering Some value in decoder, check if existing data can be decoded
    | (JsonNode.Choice _ as oldNode), JsonNode.Option newNode
    | (JsonNode.Record _ as oldNode), JsonNode.Option newNode
    | (JsonNode.Tuple _ as oldNode), JsonNode.Option newNode
    | (JsonNode.Array _ as oldNode), JsonNode.Option newNode
    | (JsonNode.Result _ as oldNode), JsonNode.Option newNode
    | (JsonNode.Terminal _ as oldNode), JsonNode.Option newNode
    | (JsonNode.Choice _ as oldNode), JsonNode.OptWithOption newNode
    | (JsonNode.Record _ as oldNode), JsonNode.OptWithOption newNode
    | (JsonNode.Tuple _ as oldNode), JsonNode.OptWithOption newNode
    | (JsonNode.Array _ as oldNode), JsonNode.OptWithOption newNode
    | (JsonNode.Result _ as oldNode), JsonNode.OptWithOption newNode
    | (JsonNode.Terminal _ as oldNode), JsonNode.OptWithOption newNode ->
        checkEvolutionCorrectness (callLevel + 1u) oldNode newNode
        |> mapOkToEvolved

    // existing terminal can be matched only with same terminal
    | JsonNode.Terminal oldTerminal, JsonNode.Terminal newTerminal ->
        if oldTerminal = newTerminal then
            Ok EvolutionOk.Evolved
        else
            Error (EvolutionError.TerminalNodeMismatch {| Expected = newTerminal; Found = oldTerminal |})
    | JsonNode.Terminal oldTerminal, JsonNode.Choice _
    | JsonNode.Terminal oldTerminal, JsonNode.Result _
    | JsonNode.Terminal oldTerminal, JsonNode.Array _
    | JsonNode.Terminal oldTerminal, JsonNode.Record _
    | JsonNode.Terminal oldTerminal, JsonNode.Tuple _ ->
        Error (EvolutionError.TerminalNodeParsingError oldTerminal)

    // existing choice can be decoded only by similar Choice
    | JsonNode.Choice oldNodes, JsonNode.Choice newNodes ->
        if oldNodes.Length <> newNodes.Length then
            Error (EvolutionError.ChoiceSizeMismatch {| Expected = newNodes.Length; Found = oldNodes.Length |})
        else
            Seq.zip oldNodes newNodes
            |> Seq.fold (fun res (oldNode, newNode) ->
                res
                |> Result.bind (fun _ -> checkEvolutionCorrectness (callLevel + 1u) oldNode newNode)
            ) (Ok EvolutionOk.Evolved)
            |> mapOkToEvolved

    | (JsonNode.Choice _ as oldJsonNode), JsonNode.Terminal _
    | (JsonNode.Choice _ as oldJsonNode), JsonNode.Result _
    | (JsonNode.Choice _ as oldJsonNode), JsonNode.Array _
    | (JsonNode.Choice _ as oldJsonNode), JsonNode.Record _
    | (JsonNode.Choice _ as oldJsonNode), JsonNode.Tuple _ ->
        Error (EvolutionError.NoDecoderFound (JsonNode.ShortForm oldJsonNode))

    // existing result can be decoded only by similar result
    | JsonNode.Result (ok, error), JsonNode.Result (newOk, newError) ->
        checkEvolutionCorrectness (callLevel + 1u) ok newOk
        |> Result.bind (fun _ -> checkEvolutionCorrectness (callLevel + 1u) error newError)
        |> mapOkToEvolved

    | (JsonNode.Result _ as oldJsonNode), JsonNode.Terminal _
    | (JsonNode.Result _ as oldJsonNode), JsonNode.Choice _
    | (JsonNode.Result _ as oldJsonNode), JsonNode.Array _
    | (JsonNode.Result _ as oldJsonNode), JsonNode.Record _
    | (JsonNode.Result _ as oldJsonNode), JsonNode.Tuple _ ->
        Error (EvolutionError.NoDecoderFound (JsonNode.ShortForm oldJsonNode))

    // Array can be decoded only by Array of similar type
    | JsonNode.Array oldNode, JsonNode.Array newNode ->
        checkEvolutionCorrectness (callLevel + 1u) oldNode newNode
        |> mapOkToEvolved

    | (JsonNode.Array _ as oldJsonNode), JsonNode.Terminal _
    | (JsonNode.Array _ as oldJsonNode), JsonNode.Choice _
    | (JsonNode.Array _ as oldJsonNode), JsonNode.Result _
    | (JsonNode.Array _ as oldJsonNode), JsonNode.Record _
    | (JsonNode.Array _ as oldJsonNode), JsonNode.Tuple _ ->
        Error (EvolutionError.NoDecoderFound (JsonNode.ShortForm oldJsonNode))

    // Record can be decoded only by Record
    | (JsonNode.Record _ as oldJsonNode), JsonNode.Terminal _
    | (JsonNode.Record _ as oldJsonNode), JsonNode.Choice _
    | (JsonNode.Record _ as oldJsonNode), JsonNode.Result _
    | (JsonNode.Record _ as oldJsonNode), JsonNode.Array _
    | (JsonNode.Record _ as oldJsonNode), JsonNode.Tuple _ ->
        Error (EvolutionError.NoDecoderFound (JsonNode.ShortForm oldJsonNode))

    | JsonNode.Record oldRecord, JsonNode.Record newRecord ->
        // all required keys in newRecord is also present in oldRecord
        // all optional keys in newRecord might not be present in oldRecord,
        //  but if they are present, they should be decode-able by the new Type
        newRecord
        |> Map.toSeq
        |> Seq.fold (fun res (newKey, newNode) ->
            res
            |> Result.bind (fun _ ->
                if newNode.IsRequiredKey then
                    oldRecord.TryFind newKey
                    |> Option.map (fun oldNode ->
                        if oldNode.IsRequiredKey then
                            checkEvolutionCorrectness (callLevel + 1u) oldNode newNode
                            |> Result.mapError (fun _ -> EvolutionError.DecodeErrorForKey newKey)
                        else
                            Error (EvolutionError.CannotReplaceOptWithByReqWith newKey)
                    )
                    |> Option.defaultValue (Error (EvolutionError.KeyNotFoundInEncodedRecord {| Expected = newKey; Found = oldRecord.Keys |> List.ofSeq |}))
                else
                    oldRecord.TryFind newKey
                    |> Option.map (fun oldNode -> checkEvolutionCorrectness (callLevel + 1u) oldNode newNode |> Result.mapError (fun _ -> EvolutionError.DecodeErrorForKey newKey))
                    |> Option.defaultValue (Ok EvolutionOk.Evolved)
            )
            ) (Ok EvolutionOk.Evolved)
        |> mapOkToEvolved

    // Tuple can be decoded by Tuple of same size or compatible Array
    | (JsonNode.Tuple _ as oldJsonNode), JsonNode.Terminal _
    | (JsonNode.Tuple _ as oldJsonNode), JsonNode.Choice _
    | (JsonNode.Tuple _ as oldJsonNode), JsonNode.Result _
    | (JsonNode.Tuple _ as oldJsonNode), JsonNode.Record _ ->
        Error (EvolutionError.NoDecoderFound (JsonNode.ShortForm oldJsonNode))

    | JsonNode.Tuple oldNodes, JsonNode.Tuple newNodes ->
        if oldNodes.Length <> newNodes.Length then
            Error (EvolutionError.TupleSizeMismatch {| Expected = newNodes.Length; Found = oldNodes.Length |})
        else
            Seq.zip oldNodes newNodes
            |> Seq.fold (fun res (oldNode, newNode) ->
                res
                |> Result.bind (fun _ -> checkEvolutionCorrectness (callLevel + 1u) oldNode newNode)
            ) (Ok EvolutionOk.Evolved)
            |> mapOkToEvolved

    // Tuple can be replaced with Array of super decoder
    | JsonNode.Tuple oldNodes, JsonNode.Array newNode ->
        oldNodes
        |> Seq.fold (fun res oldNode ->
            res
            |> Result.bind (fun _ -> checkEvolutionCorrectness (callLevel + 1u) oldNode newNode)
        ) (Ok EvolutionOk.Evolved)
        |> mapOkToEvolved

    | JsonNode.Any, JsonNode.Terminal _
    | JsonNode.Any, JsonNode.Choice _
    | JsonNode.Any, JsonNode.Result _
    | JsonNode.Any, JsonNode.Option _
    | JsonNode.Any, JsonNode.Array _
    | JsonNode.Any, JsonNode.Record _
    | JsonNode.Any, JsonNode.Tuple _
    | JsonNode.Any, JsonNode.OptWithOption _ ->
        // Any is only Used In Decoding, that means encoded value can be anything
        // so no other type can decode this node
        Error EvolutionError.CannotRemoveAnyDecoder

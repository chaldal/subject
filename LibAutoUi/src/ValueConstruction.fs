module LibAutoUi.ValueConstruction

open System
open FSharp.Reflection
open LibAutoUi.Types
open LibAutoUi.TypeExtensions


let private mergeValidationResults (input: seq<InputValidationResult<obj>>) : InputValidationResult<obj[]> =
    Seq.foldBack
        (fun curr (failures, values) ->
            match curr with
            | Ok value      -> (failures, value :: values)
            | Error failure -> (failure :: failures, values)
        )
        input
        ([], [])
    |> fun (failures, values) ->
        match failures with
        | []              -> values |> List.toArray |> Ok
        | [singleFailure] -> singleFailure |> Error
        | _               -> MultipleErrors failures |> Error

let private hasDecimals (v: decimal) : bool =
    Math.Floor(v) <> v

let updateAccumulatorWithDecodedRawValue (acc: Accumulator) (path: Path) (fn: Accumulator -> InputValue -> InputValidationResult<obj> * Accumulator) : InputValidationResult<obj> * Accumulator =
    match acc.GetCurrentValue path with
       | Some inputValue -> fn acc inputValue
       | None            -> acc.UpdateDerivedValue (path, (MissingValue path))


let updateAccumulatorWithInteger<'T> (acc: Accumulator) (path: Path) (min: decimal, max: decimal) (valueMaker: decimal -> 'T) : InputValidationResult<obj> * Accumulator =
    updateAccumulatorWithDecodedRawValue acc path (fun acc -> function
       | NumericValue(v) when v <  min || v >  max -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Value is outside valid range")))
       | NumericValue(v) when hasDecimals v        -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Value has decimals")))
       | NumericValue(v)                           -> acc.UpdateDerivedValue (path, valueMaker v |> box)
       | _                                         -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Expecting a valid number")))
    )

let updateAccumulatorWithNonIntegerNumber<'T> (acc: Accumulator) (path: Path) (min: decimal, max: decimal) (valueMaker: decimal -> 'T) : InputValidationResult<obj> * Accumulator =
    updateAccumulatorWithDecodedRawValue acc path (fun acc -> function
       | NumericValue(v) when v <  min || v >  max -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Value is outside valid range")))
       | NumericValue(v)                           -> acc.UpdateDerivedValue (path, valueMaker v |> box)
       | _                                         -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Expecting a valid number")))
    )

let (* want private but called by inline *) maybePropagateInternalNodeSettingsToLeaves (settingValue: SettingValue<obj>) (path: Path) (theType: Type) (initialAcc: Accumulator) : Accumulator =
    let acc =
        match settingValue with
        | Hidden _ -> { initialAcc with HiddenPaths = initialAcc.HiddenPaths.Add path }
        | _        -> initialAcc

    let maybeValue =
        match settingValue with
        | Hidden value                -> Some value
        | Visible visibleSettingValue -> visibleSettingValue.Initial

    let accWithInitialValues =
        match maybeValue with
        | Some value ->
            let pathToValue: Map<Path, InputValue> = generateLeafValues path theType value Map.empty
            match Set.intersect acc.UserInputValues.Keys pathToValue.Keys |> Set.isEmpty with
            | false -> acc
            | true ->
                pathToValue
                |> Map.fold
                    (fun currAcc path value ->
                        currAcc.UpdatePrefilledValue path value
                    )
                    acc

        | None -> acc

    let accWithInitialValuesAndRanges =
        match settingValue with
        | Visible { Range = Some range } ->
            acc.UpdateValueRange path range

        | _ -> accWithInitialValues

    accWithInitialValuesAndRanges

let (* want private but called by inline *) propagateSettings (settings: ResolvedSettings) (acc: Accumulator) : Accumulator =
    settings
    |> Map.fold
        (fun currAcc path setting ->
            let theType =
                acc.PathToType.TryFind path
                |> Option.getOrElseRaise (sprintf "Path %O not found in PathToType, even though this should have been checked earlier in the runtime" path |> exn)

            maybePropagateInternalNodeSettingsToLeaves setting path theType currAcc
        )
        acc


let rec private getTupleFromInput (acc: Accumulator) (path: Path) (tupleType: Type) : InputValidationResult<obj> * Accumulator =
    let (accWithElements, elementResults) =
        FSharpType.GetTupleElements(tupleType)
        |> Seq.indexed
        |> Seq.fold
            (fun (currAcc, elementResults) (index, elementType) ->
                let elementPath = path.Append (PathSegment.TupleField index)
                let (elementResult, updatedAcc) = getObjectAndPutInAccumulator elementPath elementType currAcc
                (updatedAcc, elementResult :: elementResults)
            )
            (acc, [])

    match mergeValidationResults (List.rev elementResults) with
    | Ok elementValues          -> accWithElements.UpdateDerivedValue (path, FSharpValue.MakeTuple(elementValues, tupleType))
    | Error verificationFailure -> accWithElements.UpdateDerivedValue (path, verificationFailure)

and private getRecordFromInput (acc: Accumulator) (path: Path) (recordType: Type) : InputValidationResult<obj> * Accumulator =
    let (accWithFields, fieldResults) =
        FSharpType.GetRecordFields(recordType)
        |> Seq.fold
            (fun (currAcc, fieldResults) field ->
                let fieldPath = path.Append (PathSegment.RecordField field.Name)
                let (fieldResult, updatedAcc) = getObjectAndPutInAccumulator fieldPath field.PropertyType currAcc
                (updatedAcc, fieldResult :: fieldResults)
            )
            (acc, [])

    match mergeValidationResults (List.rev fieldResults) with
    | Ok fieldValues            -> accWithFields.UpdateDerivedValue (path, FSharpValue.MakeRecord(recordType, fieldValues))
    | Error verificationFailure -> accWithFields.UpdateDerivedValue (path, verificationFailure)

and private getUnionFromInput (acc: Accumulator) (path: Path) (unionType: Type) : InputValidationResult<obj> * Accumulator =
    match acc.GetCurrentValue path with
    // NOTE this is how the implementing UI is meant to handle union cases — provide
    // a NumericValue with the Tag, which is provided on the CaseInputForm
    | Some (NumericValue tag) ->
        FSharpType.GetUnionCases unionType
        |> Seq.tryFind (fun unionCase -> decimal unionCase.Tag = tag)
        |> function
            | Some unionCase ->
                let currCasePath = path.Append (PathSegment.Case unionCase.Name)
                let (accWithFields, fieldResults) =
                    unionCase.GetFields()
                    |> Seq.indexed
                    |> Seq.fold
                        (fun (currAcc, fieldResults) (index, field) ->
                            let currFieldPath = currCasePath.Append (PathSegment.MakeCaseField (unionCaseFieldName field) index)
                            let (fieldResult, updatedAcc) = getObjectAndPutInAccumulator currFieldPath field.PropertyType currAcc
                            (updatedAcc, fieldResult :: fieldResults)
                        )
                        (acc, [])

                match mergeValidationResults (List.rev fieldResults) with
                | Ok fieldValues            -> accWithFields.UpdateDerivedValue (path, FSharpValue.MakeUnion(unionCase, fieldValues))
                | Error verificationFailure -> accWithFields.UpdateDerivedValue (path, verificationFailure)

            | None -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Invalid case selected")))

    | Some _ -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Invalid case selected")))
    | None   -> acc.UpdateDerivedValue (path, (MissingValue(path)))

and private getOptionFromInput (acc: Accumulator) (path: Path) (optionType: Type) : InputValidationResult<obj> * Accumulator =
    let optionPath = path.Append PathSegment.Option

    match acc.GetCurrentValue optionPath with
    | Some (BooleanValue false) ->
        acc.UpdateDerivedValue (path, None)

    | Some (BooleanValue true) ->
        let somePath = optionPath.Append PathSegment.OptionSome
        let (someResult, updatedAcc) = getObjectAndPutInAccumulator somePath optionType.GenericTypeArguments.[0] acc
        match someResult with
        | Ok someValue              -> updatedAcc.UpdateDerivedValue (path, Some someValue)
        | Error verificationFailure -> updatedAcc.UpdateDerivedValue (path, verificationFailure)

    | Some _ -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Invalid option case selected")))
    | None   -> acc.UpdateDerivedValue (path, (MissingValue(path)))


and getObjectAndPutInAccumulator (path: Path) (theType: Type) (acc: Accumulator) : InputValidationResult<obj> * Accumulator =
    match theType with
    | t when t = typeof<DateTimeOffset> ->
        updateAccumulatorWithDecodedRawValue acc path (fun acc -> function
            | DateTimeValue dt -> acc.UpdateDerivedValue (path, dt |> box)
            | _                -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Expecting a valid DateTimeOffset")))
        )

    | t when t = typeof<string> ->
        updateAccumulatorWithDecodedRawValue acc path (fun acc -> function
            | StringValue null
            | StringValue ""   -> acc.UpdateDerivedValue (path, (MissingValue path))
            | StringValue v    -> acc.UpdateDerivedValue (path, v)
            | invalidValue     ->
                acc.UpdateDerivedValue (path, (InvalidValue (path, "Expecting a valid string")))
        )

    | t when t = typeof<Int16>  -> updateAccumulatorWithInteger acc path (decimal Int16.MinValue,  decimal Int16.MaxValue)  Convert.ToInt16
    | t when t = typeof<Int32>  -> updateAccumulatorWithInteger acc path (decimal Int32.MinValue,  decimal Int32.MaxValue)  Convert.ToInt32
    | t when t = typeof<Int64>  -> updateAccumulatorWithInteger acc path (decimal Int64.MinValue,  decimal Int64.MaxValue)  Convert.ToInt64
    | t when t = typeof<Byte>   -> updateAccumulatorWithInteger acc path (decimal Byte.MinValue,   decimal Byte.MaxValue)   Convert.ToByte
    | t when t = typeof<SByte>  -> updateAccumulatorWithInteger acc path (decimal SByte.MinValue,  decimal SByte.MaxValue)  Convert.ToSByte
    | t when t = typeof<UInt16> -> updateAccumulatorWithInteger acc path (decimal UInt16.MinValue, decimal UInt16.MaxValue) Convert.ToUInt16
    | t when t = typeof<UInt32> -> updateAccumulatorWithInteger acc path (decimal UInt32.MinValue, decimal UInt32.MaxValue) Convert.ToUInt32
    | t when t = typeof<UInt64> -> updateAccumulatorWithInteger acc path (decimal UInt64.MinValue, decimal UInt64.MaxValue) Convert.ToUInt64

    | t when t = typeof<Single> -> updateAccumulatorWithNonIntegerNumber acc path (decimal Single.MinValue, decimal Single.MaxValue) Convert.ToSingle
    | t when t = typeof<Double> -> updateAccumulatorWithNonIntegerNumber acc path (decimal Double.MinValue, decimal Double.MaxValue) Convert.ToDouble

    | t when t = typeof<Decimal> ->
        updateAccumulatorWithDecodedRawValue acc path (fun acc -> function
           | NumericValue(v) -> acc.UpdateDerivedValue (path, v |> box)
           | _              -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Expecting a valid number")))
       )

    | t when t = typeof<Boolean> ->
        updateAccumulatorWithDecodedRawValue acc path (fun acc -> function
           | BooleanValue(v) -> acc.UpdateDerivedValue (path, v |> box)
           | _              -> acc.UpdateDerivedValue (path, (InvalidValue (path, "Expecting a boolean value (true/false)")))
       )

    | t when t = typeof<Unit> ->
        // Unit is always available, no need to have the user "input" it
        acc.UpdateDerivedValue (path, () |> box)

    | t when FSharpType.IsTuple  t -> getTupleFromInput  acc path t
    | t when FSharpType.IsRecord t -> getRecordFromInput acc path t
    | t when FSharpType.IsUnion  t -> getUnionFromInput  acc path t

    | t when t.Name = typeof<Option<_>>.Name -> getOptionFromInput acc path t

    | _ -> acc.UpdateDerivedValue (path, (UnsupportedInputType path))


let reprocessAccumulator<'T> (settings: Settings) (acc: Accumulator) : Async<Accumulator> = async {
    // 1. propagate user input into derived values
    // 2. resolve async settings
    // 3. propagate settings into prefilled values
    // 4. propagate everything into derived values

    let updatedOnceAcc              = getObjectAndPutInAccumulator Path.Empty acc.Type acc |> snd
    let! resolvedSettings           = resolveSettings settings updatedOnceAcc
    let updatedOnceAccWithPrefilled = propagateSettings resolvedSettings updatedOnceAcc
    let fullyUpdatedAcc             = getObjectAndPutInAccumulator Path.Empty acc.Type updatedOnceAccWithPrefilled |> snd

    return fullyUpdatedAcc
}

let inline getResult<'T> (accumulator: Accumulator) : InputValidationResult<'T> =
    match accumulator.DerivedValues.TryFind Path.Empty with
    | None          -> MissingValue Path.Empty |> Error
    | Some nodeData -> nodeData :> obj :?> InputValidationResult<'T>

module LibAutoUi.Test

open System
open System.Reflection
open FSharp.Reflection

type InputValidationResult<'T> =
| Validated        of 'T
| ValidationFailed of InputValidationFailure

and InputValidationFailure =
| MissingValue         of Path: string
| InvalidValue         of Path: string * Message: string
| UnsupportedInputType of Path: string
| MultipleErrors       of List<InputValidationFailure>

type InputType =
| StringInput
| NumericInput of AllowDecimals: bool * MinValue: decimal * MaxValue: decimal
| BooleanInput
| DateTimeInput
| OptionInput of Map<string, string>
| UnsupportedInput
| FileInput of MaxFileSizeInBytes: int64

type InputValue =
| StringValue   of string
| NumericValue  of decimal
| BooleanValue  of bool
| DateTimeValue of DateTimeOffset
| FileValue     of byte[]

type InputForm =
| Value  of DisplayName: string * InputName: string * InputType
| Record of DisplayName: string * InputForm list
| Union  of DisplayName: string * CaseTagPath: string * CaseInputForm list

and CaseInputForm = {
    DisplayName: string
    Tag:         int
    FieldForms:  InputForm list
}

let private concatPath (partA: string) (partB: string) : string =
    sprintf "%s_%s" partA partB

// TODO move to LibLangFsharp
type Tuple =
    static member Curry (ctor: ('P1 * 'P2 -> 'T)) : 'P1 -> 'P2 -> 'T =
        fun p1 -> fun p2 -> ctor (p1, p2)

    static member Curry (ctor: ('P1 * 'P2 * 'P3 -> 'T)) : 'P1 -> 'P2 -> 'P3 -> 'T =
        fun p1 -> fun p2 -> fun p3 -> ctor (p1, p2, p3)

let private buildFormForTuple root (path: string) (tupleName: string) (forType: Type) : InputForm =
    FSharpType.GetTupleElements(forType)
    |> Seq.groupBy (fun t -> t.Name)
    |> Seq.collect (fun (typeName, typesInGroup) ->
        typesInGroup
        |> Seq.mapi (fun index currType ->
            (sprintf "%s_%i" typeName index, currType)
        )
    )
    |> Seq.map (fun (name, t) -> root (concatPath path name) t.Name t)
    |> Seq.toList
    |> (Tuple.Curry Record) tupleName

let private buildFormForRecord root (path: string) (recordName: string) (forType: Type) : InputForm =
    FSharpType.GetRecordFields(forType)
    |> Seq.map (fun t -> root (concatPath path t.Name) t.Name t.PropertyType)
    |> Seq.toList
    |> (Tuple.Curry Record) recordName

let private unionPropertyItemRegex = new System.Text.RegularExpressions.Regex("^Item(\d)?$")
let private getFriendlyUnionPropertyName (p: PropertyInfo) : string =
    match unionPropertyItemRegex.IsMatch p.Name with
    | true  -> p.PropertyType.Name
    | false -> p.Name

let private buildFormForUnion root (path: string) (unionName: string) (forType: Type) : InputForm =
    FSharpType.GetUnionCases(forType)
    |> Seq.map (fun unionCaseInfo ->
        let fieldForms =
            unionCaseInfo.GetFields()
            |> Seq.map (fun propertyInfo ->
                let currPath = concatPath (concatPath path (unionCaseInfo.Tag.ToString())) propertyInfo.Name
                let currName = getFriendlyUnionPropertyName propertyInfo
                root currPath currName propertyInfo.PropertyType
            )
            |> Seq.toList
        {
            DisplayName = unionCaseInfo.Name
            Tag         = unionCaseInfo.Tag
            FieldForms  = fieldForms
        }
    )
    |> Seq.toList
    |> (Tuple.Curry Union) unionName (concatPath path "_Case")

type DefaultInputRenderer = string -> string -> Type -> InputForm
type InputRendererPlugin = DefaultInputRenderer -> string -> string -> Type -> Option<InputForm>
type DefaultInputParser = Map<string, InputValue> -> string -> Type -> InputValidationResult<obj>
type InputParserPlugin = DefaultInputParser -> System.Collections.Generic.IDictionary<string, InputValue> -> string -> Type -> Option<InputValidationResult<obj>>

let buildInputForm (plugin: InputRendererPlugin) (name: string) (forType: Type) : InputForm =
    let defaults (root) (path: string) (name: string) = function
        | t when FSharpType.IsTuple  t      -> buildFormForTuple  root path name t
        | t when FSharpType.IsUnion  t      -> buildFormForUnion  root path name t
        | t when FSharpType.IsRecord t      -> buildFormForRecord root path name t
        | t when t = typeof<DateTime>       -> Value (name, path, DateTimeInput)
        | t when t = typeof<DateTimeOffset> -> Value (name, path, DateTimeInput)
        | t when t = typeof<String>         -> Value (name, path, StringInput  )
        | t when t = typeof<Boolean>        -> Value (name, path, BooleanInput )
        | t when t = typeof<Int16>          -> Value (name, path, NumericInput (false, decimal Int16.MinValue,  decimal Int16.MaxValue ))
        | t when t = typeof<UInt16>         -> Value (name, path, NumericInput (false, decimal UInt16.MinValue, decimal UInt16.MaxValue))
        | t when t = typeof<Int32>          -> Value (name, path, NumericInput (false, decimal Int32.MinValue,  decimal Int32.MaxValue ))
        | t when t = typeof<UInt32>         -> Value (name, path, NumericInput (false, decimal UInt32.MinValue, decimal UInt32.MaxValue))
        | t when t = typeof<Int64>          -> Value (name, path, NumericInput (false, decimal Int64.MinValue,  decimal Int64.MaxValue ))
        | t when t = typeof<UInt64>         -> Value (name, path, NumericInput (false, decimal UInt64.MinValue, decimal UInt64.MaxValue))
        | t when t = typeof<Byte>           -> Value (name, path, NumericInput (false, decimal Byte.MinValue,   decimal Byte.MaxValue  ))
        | t when t = typeof<SByte>          -> Value (name, path, NumericInput (false, decimal SByte.MinValue,  decimal SByte.MaxValue ))
        | t when t = typeof<Single>         -> Value (name, path, NumericInput (true,  decimal Single.MinValue, decimal Single.MaxValue))
        | t when t = typeof<Double>         -> Value (name, path, NumericInput (true,  decimal Double.MinValue, decimal Double.MaxValue))
        | t when t = typeof<SByte>          -> Value (name, path, NumericInput (true,  Decimal.MinValue,        Decimal.MaxValue       ))
        // | t when t = typeof<Unsigned<_>>    -> Simple(name,path,NumericInput(true,Decimal.Zero,Decimal.MaxValue))
        | _                                 -> Value(name,path,UnsupportedInput)

    let rec getInput (path: string) (name: string) (forType: Type) =
        match plugin (defaults getInput) path name forType with
        | Some v -> v
        | None   -> defaults getInput path name forType
    getInput "formui" name forType

let private mergeValidationResults (input: seq<InputValidationResult<obj>>) : InputValidationResult<obj[]> =
    Seq.foldBack
        (fun curr (failures, values) ->
            match curr with
            | Validated value          -> (failures, value::values)
            | ValidationFailed failure -> (failure::failures, values)
        )
        input
        ([], [])
    |> fun (failures, values) ->
        match failures with
        | []              -> values |> List.toArray |> Validated
        | [singleFailure] -> singleFailure |> ValidationFailed
        | _               -> MultipleErrors failures |> ValidationFailed

let private getTupleFromInput root path (keyValuePairs:System.Collections.Generic.IDictionary<string,InputValue>) tupleType :InputValidationResult<obj> =
    FSharpType.GetTupleElements(tupleType)
    |> Seq.groupBy (fun t -> t.Name)
    |> Seq.collect(fun (n,g) -> g |> Seq.mapi (fun i t -> (sprintf "%s_%i" n i),t))
    |> Seq.map (fun (n,t) -> root (concatPath path n) keyValuePairs t)
    |> mergeValidationResults
    |> function
       | Validated arr -> FSharpValue.MakeTuple(arr,tupleType) |> Validated
       | ValidationFailed vf -> ValidationFailed vf

let private getRecordFromInput root path (keyValuePairs:System.Collections.Generic.IDictionary<string,InputValue>) recordType :InputValidationResult<obj> =
    FSharpType.GetRecordFields(recordType)
    |> Seq.map (fun t -> root (concatPath path t.Name) keyValuePairs t.PropertyType)
    |> mergeValidationResults
    |> function
       | Validated arr -> FSharpValue.MakeRecord(recordType,arr) |> Validated
       | ValidationFailed vf -> ValidationFailed vf

let private getUnionFromInput root path (keyValuePairs:System.Collections.Generic.IDictionary<string,InputValue>) unionType :InputValidationResult<obj> =
    match keyValuePairs.TryGetValue (concatPath path "_Case") with
    | true,NumericValue v ->
        FSharpType.GetUnionCases(unionType)
        |> Seq.tryFind (fun uc -> decimal uc.Tag = v)
        |> function
           | Some uc -> uc.GetFields()
                        |> Seq.map (fun p -> root (concatPath (concatPath path (uc.Tag.ToString())) p.Name) keyValuePairs p.PropertyType)
                        |> mergeValidationResults
                        |> function
                           | Validated arr -> FSharpValue.MakeUnion(uc,arr) |> Validated
                           | ValidationFailed vf -> ValidationFailed vf
           | None -> InvalidValue(concatPath path "_Case", "Invalid case selected") |> ValidationFailed
    | true,_ -> InvalidValue(concatPath path "_Case", "Invalid case selected") |> ValidationFailed
    | false,_ -> MissingValue(concatPath path "_Case") |> ValidationFailed

let getObjectFromInput<'T> (plugin:InputParserPlugin) (pathToValues:Map<string,InputValue>) :InputValidationResult<'T> =
    let rec getObject path (keyValuePairs:System.Collections.Generic.IDictionary<string,InputValue>) objType :InputValidationResult<obj> =
        let defaults (keyValuePairs:System.Collections.Generic.IDictionary<string,InputValue>) path objType :InputValidationResult<obj> =
            match objType with
            | t when t = typeof<DateTime>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,DateTimeValue(dt) -> dt.DateTime |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid Date/Time") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<DateTime>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,DateTimeValue(dt) -> dt |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid Date/Time") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<string>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,StringValue(null) -> MissingValue path |> ValidationFailed
                   | true,StringValue("") -> MissingValue path |> ValidationFailed
                   | true,StringValue(v) -> v |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid string") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Int16>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal Int16.MinValue || v > decimal Int16.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToInt16(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Int32>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal Int32.MinValue || v > decimal Int32.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToInt32(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Int64>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal Int64.MinValue || v > decimal Int64.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToInt64(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Byte>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal Byte.MinValue || v > decimal Byte.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToByte(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<SByte>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal SByte.MinValue || v > decimal SByte.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToSByte(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<UInt16>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal UInt16.MinValue || v > decimal UInt16.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToUInt16(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<UInt32>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal UInt32.MinValue || v > decimal UInt32.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToUInt32(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<UInt64>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal UInt64.MinValue || v > decimal UInt64.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) when Math.Floor(v) <> v -> InvalidValue(path,"Value has decimals") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToUInt64(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Single>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal Single.MinValue || v > decimal Single.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToSingle(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Double>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) when v < decimal Double.MinValue || v > decimal Double.MaxValue -> InvalidValue(path,"Value is outside valid range") |> ValidationFailed
                   | true,NumericValue(v) -> Convert.ToDouble(v) |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Decimal>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,NumericValue(v) -> v |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
            | t when t = typeof<Boolean>
                -> match keyValuePairs.TryGetValue(path) with
                   | true,BooleanValue(v) -> v |> box |> Validated
                   | true,_ -> InvalidValue(path,"Expecting a boolean value (true/false)") |> ValidationFailed
                   | false,_ -> MissingValue path |> ValidationFailed
//            | t when t = typeof<Unsigned<_>>
//                -> match keyValuePairs.TryGetValue(path) with
//                   | true,NumericValue(v) -> match createUnsigned v with
//                                             | UnsignedResult.Success v -> v |> box |> Validated
//                                             | UnsignedResult.ErrNegative _ -> InvalidValue(path,"Expecting a positive number") |> ValidationFailed
//                   | true,_ -> InvalidValue(path,"Expecting a valid number") |> ValidationFailed
//                   | false,_ -> MissingValue path |> ValidationFailed
            | t when FSharpType.IsTuple t -> getTupleFromInput getObject path keyValuePairs t
            | t when FSharpType.IsRecord t -> getRecordFromInput getObject path keyValuePairs t
            | t when FSharpType.IsUnion t -> getUnionFromInput getObject path keyValuePairs t
            | _ -> UnsupportedInputType path |> ValidationFailed
        match plugin defaults keyValuePairs path objType with
        | Some v -> v
        | None -> defaults keyValuePairs path objType

    match getObject "formui" (pathToValues |> Map.toSeq |> dict) typeof<'T> with
    | Validated v -> v :?> 'T |> Validated
    | ValidationFailed vf -> ValidationFailed vf

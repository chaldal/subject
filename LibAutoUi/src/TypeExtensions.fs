module LibAutoUi.TypeExtensions

open System
open FSharp.Reflection
open LibAutoUi.Types

type PathSegment with
    static member MakeCaseField (maybeName: Option<string>) (index: int) : PathSegment =
        match maybeName with
        | Some name -> PathSegment.CaseField (Named name)
        | None      -> PathSegment.CaseField (Positional index)

// This method is a good idea only in theory. In practice, I tried fairly hard, and writing a clean
// fold abstraction over all the stuff we want to do "while walking the type tree" has been intractable.
// It's just far less confusing to write the traversal by hand each time than to understand the abstraction
// layer that results from trying to accommodate all the use cases.
let rec foldType<'State> (folder: 'State -> Path -> Type -> 'State) (initialState: 'State) (initialPath: Path) (initialType: Type) : 'State =
    let state = folder initialState initialPath initialType

    match initialType with
    | t when FSharpType.IsTuple t  ->
        FSharpType.GetTupleElements(t)
        |> Seq.mapi (fun index elementType -> (index, elementType))
        |> Seq.fold
            (fun currState (index, elementType) ->
                let elementPath = initialPath.Append (PathSegment.TupleField index)
                foldType folder currState elementPath elementType
            )
            state

    | t when FSharpType.IsRecord t ->
        FSharpType.GetRecordFields(t)
        |> Seq.fold
            (fun currState field ->
                let fieldPath = initialPath.Append (PathSegment.RecordField field.Name)
                foldType folder currState fieldPath field.PropertyType
            )
            state

    | t when FSharpType.IsUnion t ->
        FSharpType.GetUnionCases t
        |> Seq.fold
            (fun currCaseState caseInfo ->
                let currCasePath = initialPath.Append (PathSegment.Case caseInfo.Name)

                caseInfo.GetFields()
                |> Seq.indexed
                |> Seq.fold
                    (fun currFieldState (index, field) ->
                        let currFieldPath = currCasePath.Append (PathSegment.MakeCaseField (unionCaseFieldName field) index)
                        foldType folder currFieldState currFieldPath field.PropertyType
                    )
                    currCaseState
            )
            state

    | t when t.Name = typeof<Option<_>>.Name ->
        let pathWithSome  = (initialPath.Append PathSegment.Option).Append PathSegment.OptionSome
        let stateWithSome = foldType folder state pathWithSome t.GenericTypeArguments.[0]
        stateWithSome

    | t when t = typeof<DateTime>       -> state
    | t when t = typeof<DateTimeOffset> -> state
    | t when t = typeof<string>         -> state
    | t when t = typeof<Int16>          -> state
    | t when t = typeof<Int32>          -> state
    | t when t = typeof<Int64>          -> state
    | t when t = typeof<Byte>           -> state
    | t when t = typeof<SByte>          -> state
    | t when t = typeof<UInt16>         -> state
    | t when t = typeof<UInt32>         -> state
    | t when t = typeof<UInt64>         -> state
    | t when t = typeof<Single>         -> state
    | t when t = typeof<Double>         -> state
    | t when t = typeof<Decimal>        -> state
    | t when t = typeof<Boolean>        -> state
    | t when t = typeof<Guid>           -> state
    | t when t = typeof<Unit>           -> state

    | t -> failwith (sprintf "unsupported type %s" t.Name)


let computePathToType (theType: Type) : Map<Path, Type> =
    foldType
        (fun acc path currType ->
            acc.Add (path, currType)
        )
        Map.empty
        Path.Empty
        theType


let rec generateLeafValues (path: Path) (theType: Type) (value: obj) (acc: Map<Path, InputValue>) : Map<Path, InputValue> =
    match theType with
    | t when t = typeof<DateTimeOffset> -> acc.Add (path, (value :?> DateTimeOffset) |> DateTimeValue)
    | t when t = typeof<string>         -> acc.Add (path, (value :?> string) |> StringValue)
    | t when t = typeof<Int16>          -> acc.Add (path, (value :?> Int16 ) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<Int32>          -> acc.Add (path, (value :?> Int32 ) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<Int64>          -> acc.Add (path, (value :?> Int64 ) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<Byte>           -> acc.Add (path, (value :?> Byte  ) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<SByte>          -> acc.Add (path, (value :?> SByte ) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<UInt16>         -> acc.Add (path, (value :?> UInt16) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<UInt32>         -> acc.Add (path, (value :?> UInt32) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<UInt64>         -> acc.Add (path, (value :?> UInt64) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<Single>         -> acc.Add (path, (value :?> Single) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<Double>         -> acc.Add (path, (value :?> Double) |> Convert.ToDecimal |> NumericValue)
    | t when t = typeof<Decimal>        -> acc.Add (path, (value :?> decimal) |> NumericValue)
    | t when t = typeof<Boolean>        -> acc.Add (path, (value :?> bool)    |> BooleanValue)
    | t when t = typeof<Guid>           -> acc.Add (path, (value :?> Guid)    |> GuidValue)
    | t when t = typeof<Unit>           -> acc.Add (path, UnitValue)
    | t when FSharpType.IsTuple t  ->
        FSharpType.GetTupleElements(t)
        |> Seq.mapi (fun index elementType -> (index, elementType))
        |> Seq.fold
            (fun currAcc (index, elementType) ->
                let elementPath = path.Append (PathSegment.TupleField index)
                QQQ
                //let elementValue = LibClient.JsInterop.Reflection.getTupleFieldValue index value
                //generateLeafValues elementPath elementType elementValue currAcc
            )
            acc

    | t when FSharpType.IsRecord t ->
        FSharpType.GetRecordFields(t)
        |> Seq.fold
            (fun currAcc field ->
                let fieldPath = path.Append (PathSegment.RecordField field.Name)
                let fieldValue = LibClient.JsInterop.Reflection.getRecordFieldValue field value
                generateLeafValues fieldPath field.PropertyType fieldValue currAcc
            )
            acc

    | t when FSharpType.IsUnion t ->
        let (ourCase, _) = FSharpValue.GetUnionFields(value, theType)

        FSharpType.GetUnionCases t
        |> Seq.fold
            (fun currAcc caseInfo ->
                // Because for some fracked up reason, in Fable runtime normal equality comparison does not work
                if ourCase.Tag = caseInfo.Tag then
                    let currAccWithCaseChoice = currAcc.Add (path, ourCase.Tag |> decimal |> NumericValue)

                    let currCasePath = path.Append (PathSegment.Case caseInfo.Name)

                    caseInfo.GetFields()
                    |> Seq.indexed
                    |> Seq.fold
                        (fun currInnerAcc (index, field) ->
                            let currFieldPath = currCasePath.Append (PathSegment.MakeCaseField (unionCaseFieldName field) index)
                            let fieldValue = LibClient.JsInterop.Reflection.getUnionCaseFieldValue index value
                            generateLeafValues currFieldPath field.PropertyType fieldValue currInnerAcc
                        )
                        currAccWithCaseChoice
                else
                    currAcc
            )
            acc

    | t when t.Name = typeof<Option<_>>.Name ->
        let optionPath = path.Append PathSegment.Option
        match value with
        | null ->
            acc.Add (optionPath, BooleanValue false)
        | _ ->
            let accWithOption = acc.Add (optionPath, BooleanValue true)
            // NOTE in F# we would use value.GetType().GetProperty("Value").GetGetMethod().Invoke(value, Array.empty)
            // but in the Fable runtime, Somes are _usually_ just straight up values.
            // TODO Option<Option<...>> are, I think, an exception to that, so we'll need to deal with this eventually
            let someValue = value
            let somePath = optionPath.Append PathSegment.OptionSome
            generateLeafValues somePath t.GenericTypeArguments.[0] someValue accWithOption

    | _ -> failwith (sprintf "Unsupported type %O at path %O" theType path)


type Accumulator with
    static member Empty (theType: Type) (pathToType: Map<Path, Type>) : Accumulator = {
        Type            = theType
        PathToType      = pathToType
        UserInputValues = Map.empty
        PrefilledValues = Map.empty
        DerivedValues   = Map.empty
        ValueRanges     = Map.empty
        HiddenPaths     = Set.empty
    }

    member this.UpdateUserInput (path: Path) (value: InputValue) : Accumulator =
        { this with UserInputValues = this.UserInputValues.AddOrUpdate (path, value) }

    member this.UpdatePrefilledValue (path: Path) (value: InputValue) : Accumulator =
        { this with PrefilledValues = this.PrefilledValues.AddOrUpdate (path, value) }

    member this.UpdateUserInputFromRange (path: Path) (value: obj) (theType: Type) : Accumulator =
        let pathToValue: Map<Path, InputValue> = generateLeafValues path theType value Map.empty
        pathToValue
        |> Map.fold
            (fun currAcc path value ->
                currAcc.UpdateUserInput path value
            )
            this

    member this.UpdateDerivedValue (path: Path, value: obj) : InputValidationResult<obj> * Accumulator =
        let updatedDerivedValue = Ok value
        let updatedThis = { this with DerivedValues = this.DerivedValues.AddOrUpdate (path, updatedDerivedValue) }
        (updatedDerivedValue, updatedThis)

    member this.UpdateDerivedValue (path: Path, validationFailure: InputValidationFailure) : InputValidationResult<obj> * Accumulator =
        let updatedDerivedValue = Error validationFailure
        let updatedThis = { this with DerivedValues = this.DerivedValues.AddOrUpdate (path, updatedDerivedValue) }
        (updatedDerivedValue, updatedThis)

    member this.UpdateValueRange (path: Path) (range: NonemptyList<obj>) : Accumulator =
        // TODO wipe all user input in path and all its "children" if the choice is no longer valid
        { this with
            ValueRanges = this.ValueRanges.AddOrUpdate (path, range)
        }

    member this.GetCurrentValue (path: Path) : Option<InputValue> =
        this.UserInputValues.TryFind path
        |> Option.orElse (this.PrefilledValues.TryFind path)

    member this.GetCurrentDerivedValue (path: Path) : Option<obj> =
        this.DerivedValues.TryFind path
        |> Option.flatMap Result.toOption

    member this.GetCurrentDerivedResult (path: Path) : Option<InputValidationResult<obj>> =
        this.DerivedValues.TryFind path

type Setting<'T> with
    member this.Resolve (acc: Accumulator) : Async<Option<SettingValue<'T>>> = async {
        match this with
        | StaticSetting value            -> return Some value
        | DynamicSetting accToMaybeValue -> return! accToMaybeValue acc
    }

let resolveSettings (settings: Settings) (acc: Accumulator) : Async<ResolvedSettings> =
    settings
    |> List.iter (fun (path, settingType, _) ->
        match acc.PathToType.TryFind path with
        | Some expectedType ->
            if settingType <> expectedType then
                sprintf "Setting type %O does not match expected type %O for path %O" settingType expectedType path |> failwith
        | None ->
            sprintf "Setting of type %O provided for an unknown path %O" settingType path |> failwith
    )

    settings
    |> Seq.map (fun (path, _, setting) ->
        setting.Resolve acc
        |> Async.Map (fun resolvedSetting -> (path, resolvedSetting))
    )
    |> Async.Parallel
    |> Async.Map (fun pairs ->
        pairs
        |> Seq.ofArray
        |> Seq.filterMap (fun (path, maybeSettingValue) ->
            maybeSettingValue
            |> Option.map (fun settingValue -> (path, settingValue))
        )
        |> Map.ofSeq
    )

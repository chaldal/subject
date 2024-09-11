module LibAutoUi.FormConstruction

open System
open FSharp.Reflection

open System.Reflection
open LibAutoUi.Types
open LibAutoUi.TypeExtensions

type InputForm with
    static member Make (path: Path) (displayName: string) (theType: Type) (formType: InputFormType) : InputForm = {
        Path        = path
        DisplayName = displayName
        FormType    = formType
        Type        = theType
    }

let private buildFormForTuple root (path: Path) (tupleName: string) (forType: Type) : InputForm =
    FSharpType.GetTupleElements(forType)
    |> Seq.mapi (fun index elementType ->
        let elementPath = path.Append (PathSegment.TupleField index)
        root elementPath elementType.Name elementType
    )
    |> Seq.toList
    |> InputFormType.Record
    |> InputForm.Make path tupleName forType

let private buildFormForRecord root (path: Path) (recordName: string) (forType: Type) : InputForm =
    FSharpType.GetRecordFields(forType)
    |> Seq.map (fun field -> root (path.Append (PathSegment.RecordField field.Name)) field.Name field.PropertyType)
    |> Seq.toList
    |> InputFormType.Record
    |> InputForm.Make path recordName forType

let private buildFormForOption root (path: Path) (name: string) (forType: Type) : InputForm =
    match forType.GenericTypeArguments with
    | [| typeArgument |] ->
        let optionPath = path.Append PathSegment.Option
        let innerInputForm = root (optionPath.Append PathSegment.OptionSome) typeArgument.Name typeArgument
        InputFormType.Option innerInputForm
        |> InputForm.Make path name forType
    | _ -> failwith "Was expecting exactly one type argument for the Option class"

let private buildFormForList root (path: Path) (name: string) (forType: Type) : InputForm =
    match forType.GenericTypeArguments with
    | [| typeArgument |] ->
        let innerInputForm = root (path.Append PathSegment.List) typeArgument.Name typeArgument
        InputFormType.List innerInputForm
        |> InputForm.Make path name forType
    | _ -> failwith "Was expecting exactly one type argument for the List class"

let getFriendlyUnionPropertyName (propertyInfo: PropertyInfo) : string =
    unionCaseFieldName propertyInfo
    |> Option.getOrElse propertyInfo.PropertyType.Name

let private buildFormForUnion root (path: Path) (unionName: string) (forType: Type) : InputForm =
    FSharpType.GetUnionCases forType
    |> Seq.map (fun caseInfo ->
        let currCasePath = path.Append (PathSegment.Case caseInfo.Name)
        let fieldForms =
            caseInfo.GetFields()
            |> Seq.mapi (fun index field ->
                let currFieldPath = currCasePath.Append (PathSegment.MakeCaseField (unionCaseFieldName field) index)
                let currName      = getFriendlyUnionPropertyName field
                root currFieldPath currName field.PropertyType
            )
            |> Seq.toList
        {
            DisplayName = caseInfo.Name
            Tag         = caseInfo.Tag
            FieldForms  = fieldForms
        }
    )
    |> Seq.toList
    |> InputFormType.Union
    |> InputForm.Make path unionName forType

let buildInputForm (plugin: InputRendererPlugin) (name: string) (forType: Type) : InputForm =
    let defaults (root) (path: Path) (name: string) = function
        | t when FSharpType.IsTuple  t           -> buildFormForTuple  root path name t
        | t when FSharpType.IsUnion  t           -> buildFormForUnion  root path name t
        | t when FSharpType.IsRecord t           -> buildFormForRecord root path name t
        | t when t.Name = typeof<Option<_>>.Name -> buildFormForOption root path name t
        | t when t.Name = typeof<List<_>>.Name   -> buildFormForList   root path name t
        | t when t = typeof<TimeSpan>            -> InputForm.Make path name t (InputFormType.Primitive TimeSpanInput)
        | t when t = typeof<DateTime>            -> InputForm.Make path name t (InputFormType.Primitive DateTimeInput)
        | t when t = typeof<DateTimeOffset>      -> InputForm.Make path name t (InputFormType.Primitive DateTimeInput)
        | t when t = typeof<String>              -> InputForm.Make path name t (InputFormType.Primitive StringInput  )
        | t when t = typeof<Boolean>             -> InputForm.Make path name t (InputFormType.Primitive BooleanInput )
        | t when t = typeof<Guid>                -> InputForm.Make path name t (InputFormType.Primitive GuidInput    )
        | t when t = typeof<Int16>               -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal Int16.MinValue,  decimal Int16.MaxValue )))
        | t when t = typeof<UInt16>              -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal UInt16.MinValue, decimal UInt16.MaxValue)))
        | t when t = typeof<Int32>               -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal Int32.MinValue,  decimal Int32.MaxValue )))
        | t when t = typeof<UInt32>              -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal UInt32.MinValue, decimal UInt32.MaxValue)))
        | t when t = typeof<Int64>               -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal Int64.MinValue,  decimal Int64.MaxValue )))
        | t when t = typeof<UInt64>              -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal UInt64.MinValue, decimal UInt64.MaxValue)))
        | t when t = typeof<Byte>                -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal Byte.MinValue,   decimal Byte.MaxValue  )))
        | t when t = typeof<SByte>               -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (false, decimal SByte.MinValue,  decimal SByte.MaxValue )))
        | t when t = typeof<Single>              -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (true,  decimal Single.MinValue, decimal Single.MaxValue)))
        | t when t = typeof<Double>              -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (true,  decimal Double.MinValue, decimal Double.MaxValue)))
        | t when t = typeof<SByte>               -> InputForm.Make path name t (InputFormType.Primitive (NumericInput (true,  Decimal.MinValue,        Decimal.MaxValue       )))
        | t when t = typeof<Unit>                -> InputForm.Make path name t (InputFormType.Primitive UnitInput)
        // | t when t = typeof<Unsigned<_>>    -> InputForm.Value(name,path,NumericInput(true,Decimal.Zero,Decimal.MaxValue))
        | t                                      -> InputForm.Make path name t (InputFormType.Primitive UnsupportedInput)


    let rec getInput (path: Path) (name: string) (forType: Type) =
        match plugin (defaults getInput) path name forType with
        | Some v -> v
        | None   -> defaults getInput path name forType
    getInput Path.Empty name forType

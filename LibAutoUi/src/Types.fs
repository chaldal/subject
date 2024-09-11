module LibAutoUi.Types

open System
open LibClient.Services.Subscription

// SHARED

type Path = Path of List<PathSegment>
with
    static member Empty : Path =
        Path []

    member this.Append (segment: PathSegment) : Path =
        let segments = match this with Path segments -> segments
        segment :: segments |> Path

and [<RequireQualifiedAccess>] PathSegment =
| RecordField of Name: string
| TupleField  of Index: int
| CaseField   of CaseField
| Case        of Name: string
| Option
| OptionNone // TODO drop Option prefix
| OptionSome // TODO drop Option prefix
| List

and CaseField =
| Named      of string
| Positional of int

// FORM CONSTRUCTION

type InputType =
| UnitInput
| StringInput
| NumericInput of AllowDecimals: bool * MinValue: decimal * MaxValue: decimal
| BooleanInput
| GuidInput
| DateTimeInput
| TimeSpanInput
| UnsupportedInput
| FileInput of MaxFileSizeInBytes: int64

type InputForm = {
    Path:        Path
    DisplayName: string
    FormType:    InputFormType
    Type:        Type
}

and [<RequireQualifiedAccess>] InputFormType =
| Primitive of InputType
| Option    of InputForm
| List      of InputForm
| Record    of List<InputForm>
| Union     of List<CaseInputForm>

and CaseInputForm = {
    DisplayName: string
    Tag:         int
    FieldForms:  List<InputForm>
}


type DefaultInputRenderer = Path -> string -> Type -> InputForm
type InputRendererPlugin  = DefaultInputRenderer -> Path -> string -> Type -> Option<InputForm>


// VALUE CONSTRUCTION AND VALIDATION

type InputValidationResult<'T> = Result<'T, InputValidationFailure>

and InputValidationFailure =
| MissingValue         of Path
| InvalidValue         of Path * Message: string
| UnsupportedInputType of Path
| MultipleErrors       of List<InputValidationFailure>

type InputValue =
| UnitValue
| StringValue   of string
| NumericValue  of decimal
| BooleanValue  of bool
| GuidValue     of Guid
| DateTimeValue of DateTimeOffset
| FileValue     of byte[]

type Accumulator = {
    Type:            Type
    PathToType:      Map<Path, Type>
    UserInputValues: Map<Path, InputValue>
    PrefilledValues: Map<Path, InputValue>
    DerivedValues:   Map<Path, InputValidationResult<obj>>
    ValueRanges:     Map<Path, NonemptyList<obj>>
    HiddenPaths:     Set<Path>
}

type DefaultInputParser = Accumulator -> Path -> Type -> Async<Accumulator>
type InputParserPlugin  = DefaultInputParser -> Accumulator -> Path -> Type -> Async<Option<Accumulator>>


// SETTINGS

type Setting<'T> =
// redundant names to make space for helper methods
| StaticSetting  of SettingValue<'T>
| DynamicSetting of (Accumulator -> Async<Option<SettingValue<'T>>>)

and SettingValue<'T> =
| Hidden  of Value: 'T
| Visible of VisibleSettingValue<'T>

and VisibleSettingValue<'T> = {
    Initial: Option<'T>
    Range:   Option<NonemptyList<'T>>
}

type Settings = List<Path * Type * Setting<obj>>

type ResolvedSettings = Map<Path, SettingValue<obj>>

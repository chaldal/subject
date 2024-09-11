module LibAutoUi.SettingsDsl

open System
open LibAutoUi.Types

let rf (name: string):  PathSegment = PathSegment.RecordField name
let uc (name: string):  PathSegment = PathSegment.Case name
let cfn (name: string): PathSegment = PathSegment.CaseField (Named name)
let cfp (index: int):   PathSegment = PathSegment.CaseField (Positional index)
let opt:  PathSegment = PathSegment.Option
let some: PathSegment = PathSegment.OptionSome

let ( / ) (path: Path) (segment: PathSegment) =
    path.Append segment

let root: Path = Path.Empty

type Setting<'T> with
    static member DynamicValue (dynamicInitialValue: Accumulator -> Async<Option<'T>>) =
        (fun acc -> async {
            let! maybeInitialValue = dynamicInitialValue acc
            return
                maybeInitialValue
                |> Option.map (fun initialValue ->
                    {
                        Initial = Some initialValue
                        Range   = None
                    }
                    |> Visible
                )
        })
        |> DynamicSetting

    static member DynamicRange (dynamicValueRange: Accumulator -> Async<Option<NonemptyList<'T>>>) =
        (fun acc -> async {
            let! maybeValueRange = dynamicValueRange acc
            return
                maybeValueRange
                |> Option.map (fun valueRange ->
                    {
                        Initial = None
                        Range   = Some valueRange
                    }
                    |> Visible
                )
        })
        |> DynamicSetting

    static member StaticValue (initialValue: 'T) : Setting<'T> =
        {
            Initial = Some initialValue
            Range   = None
        }
        |> Visible
        |> StaticSetting

    static member HiddenStaticValue (value: 'T) : Setting<'T> =
        value
        |> Hidden
        |> StaticSetting

    static member StaticRange (valueRange: NonemptyList<'T>) : Setting<'T> =
        {
            Initial = None
            Range   = Some valueRange
        }
        |> Visible
        |> StaticSetting


let inline ( => ) (path: Path) (setting: Setting<'T>) : Path * Type * Setting<obj> =
    (path, typeof<'T>, setting :> obj :?> Setting<obj>)

let (|LeafValueIs|_|) (candidate: obj) (input: Option<InputValidationResult<obj>>) =
    match input with
    | Some v when v = Ok candidate -> Some candidate
    | _                            -> None

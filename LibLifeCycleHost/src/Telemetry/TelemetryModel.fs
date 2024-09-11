module LibLifeCycleHost.TelemetryModel

open System.Threading.Tasks
open LibLifeCycleCore

type TrackedOperationResult<'T> = {
    IsSuccess: Option<bool>
    ReturnValue: 'T
    AfterRunProperties: Map<string, string>
}

[<RequireQualifiedAccess>]
type OperationType =
| SideEffectVanilla
| SideEffectTransaction
| SideEffectDeleteSelf
| GrainCallVanilla
| GrainCallTriggerSubscription
| GrainCallTriggerTimer
| GrainCallConnector
| TestSimulation
| TestTimeForward
with
    interface Union

let private operationTypeToIcon =
    [
        OperationType.SideEffectVanilla             , "↘"
        OperationType.SideEffectTransaction         , "↘️"
        OperationType.SideEffectDeleteSelf          , "💀"
        OperationType.GrainCallVanilla              , "🌾"
        OperationType.GrainCallTriggerSubscription  , "⚡️"
        OperationType.GrainCallTriggerTimer         , "⏰"
        OperationType.GrainCallConnector            , "☢️"
        OperationType.TestSimulation                , "🧪"
        OperationType.TestTimeForward               , "⏩"
    ]
    |> Map.ofList

let private iconToOperationType = operationTypeToIcon |> Map.toSeq |> Seq.map (fun (op, icon) -> icon, op) |> Map.ofSeq

type OperationType with
    member this.IconString : string =
        match operationTypeToIcon |> Map.tryFind this with
        | Some icon -> icon
        | None -> UnionCase.nameOfCase this

    static member TryParseIconString (iconString: string) : Option<OperationType> =
        iconToOperationType |> Map.tryFind iconString


type TrackedOperationInput = {
    Partition: GrainPartition
    Type: OperationType
    Name: string
    MaybeParentActivityId: Option<string>
    MakeItNewParentActivityId: bool
    BeforeRunProperties: Map<string, string>
}

type OperationTracker =
    abstract member TrackOperation<'T> :
        input: TrackedOperationInput -> run: (unit -> Task<TrackedOperationResult<'T>>) -> Task<'T>
    abstract member SendMetric: name : string -> value: float -> unit
    abstract member Shutdown: unit -> Task

/// stub for tests or when telemetry disabled
type private NoopOperationTracker () = class end
with
    interface OperationTracker with
        member _.TrackOperation _input run =
            task { // must be context-sensitive task because invoked from Orleans grain call filter
                let! res = run ()
                return res.ReturnValue
            }

        member _.SendMetric _name _value = ()
        member this.Shutdown() = Task.CompletedTask

let noopOperationTracker : OperationTracker = NoopOperationTracker ()
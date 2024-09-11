module LibClient.Dialogs

open Fable.Core.JsInterop
open LibClient.Responsive
open LibClient.ServiceInstances

open EggShellReact

[<RequireQualifiedAccess>]
type DialogCloseMethod =
| HistoryForward
| HistoryBack

type ResponseChannel<'T> = {
    OnResult:      'T -> unit
    MaybeOnCancel: Option<unit -> unit>
}

type DialogProps<'Parameters, 'Result> = {
    PstoreKey:       string
    Parameters:      'Parameters
    ResponseChannel: ResponseChannel<'Result>
    Close:           DialogCloseMethod -> ReactEvent.Action -> unit
}

let hide (props: DialogProps<'T, 'R>) (method: DialogCloseMethod) (e: ReactEvent.Action) : unit =
    props.Close method e

let tryCancel (props: DialogProps<'T, 'R>) (canCancel: unit -> Async<bool>) (method: DialogCloseMethod) (e: ReactEvent.Action) : unit =
    async {
        match! canCancel() with
        | false -> Noop
        | true ->
            props.ResponseChannel.MaybeOnCancel |> Option.sideEffect (fun f -> f())
            hide props method e
    }
    |> Async.StartImmediate // TODO deal with error handling

[<AbstractClass>]
type DialogComponent<'Parameters, 'Result, 'Estate, 'Pstate, 'Actions, 'Self>(name: string, pstoreKey: string, initialProps: DialogProps<'Parameters, 'Result>, actionsConstructor: 'Self -> 'Actions, hasStyles: bool) as this =
    inherit PstatefulComponent<DialogProps<'Parameters, 'Result>, 'Estate, 'Pstate, 'Actions, 'Self>(name, pstoreKey, initialProps, actionsConstructor, hasStyles)

    do
        this.RunOnUnmount (LibClient.Components.TapCaptureDebugVisibility.addIsVisibleForDebugChangeListener (System.Action (fun () -> this.forceUpdate()))).Off

    abstract member CanCancel: unit -> Async<bool>

    member this.Hide (method: DialogCloseMethod) (e: ReactEvent.Action) : unit =
        hide this.props method e

    member this.TryCancel (method: DialogCloseMethod) (e: ReactEvent.Action) : unit =
        tryCancel this.props this.CanCancel method e

type ResultSideEffect = unit
type AnyError = obj

let doOpen<'Parameters, 'Result>
    (pstoreKey:       string)
    (parameters:      'Parameters)
    (make:            DialogProps<'Parameters, 'Result> -> array<Fable.React.ReactElement> -> Fable.React.ReactElement)
    (responseChannel: ResponseChannel<'Result>)
    (close:           DialogCloseMethod -> ReactEvent.Action -> unit)
    : ReactElement =

    make
        {
            Parameters      = parameters
            PstoreKey       = pstoreKey
            ResponseChannel = responseChannel
            Close           = close
        }
        [||]

module AdHoc =
    let mutable private goImplementation: Option<((DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement) -> ReactEvent.Action -> unit> = None

    let provideGoImplementation (value: ((DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement) -> ReactEvent.Action -> unit) : unit =
        goImplementation <- Some value

    let go (closeToDialog: (DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement) : ReactEvent.Action -> unit =
        fun e ->
            match goImplementation with
            | Some goImplementation -> goImplementation closeToDialog e
            | None -> failwith "LibClient.AdHocDialogs go implementation was not provided"

module LibClient.Components.TapCaptureDebugVisibility

// Doing this instead of using a context, because wrapping each and every
// tap capture in a context seems unnecessarily expensive, doing a forceUpdate
// on the entire app is just cheaper with no ill-effects.
let mutable isVisibleForDebug: bool = false
let mutable private listeners: List<System.Action> = []

let addIsVisibleForDebugChangeListener (listener: System.Action) : {| Off: unit -> unit |} =
    listeners <- listener :: listeners
    {|
        Off = fun () -> listeners <- listeners |> List.without listener
    |}

let setVisibleForDebug (value: bool) : unit =
    isVisibleForDebug <- value
    listeners
    |> List.iter (fun listener -> listener.Invoke())

let toggleVisibleForDebug () : unit =
    setVisibleForDebug (not isVisibleForDebug)

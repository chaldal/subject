module LibClient.EventBus

open LibLang
open LibClient.JsInterop
open System.Collections.Generic

type OnResult = {
    Off: unit -> unit
}

type EventListener<'T> = 'T -> unit

type private RegisteredEventListener<'T> = System.Action<'T>

type Queue<'T> = Queue of Id: string

// This event bus works on a "tell me what type you expect" basis. This gives us
// a large portion of the safety without having to pay the price of a centralized
// event registration mechanism which is a big hassle for arguably little value.
//
// NOTE XXX not bothering with thread safety, assuming that Fable's .NET runtime has
// the same event loop foundation as a JS runtime. Will revisit if this is not the case.
type EventBus () =
    let listeners = new Dictionary<Queue<obj>, list<RegisteredEventListener<obj>>>()

    let getListenersObj (queue: Queue<'T>) : list<RegisteredEventListener<obj>> =
        listeners.Get (queue :> obj :?> Queue<obj>)
        |> Option.getOrElse []

    let getListeners (queue: Queue<'T>) : list<RegisteredEventListener<'T>> =
        (getListenersObj queue) :> obj :?> list<RegisteredEventListener<'T>>

    let setListeners (queue: Queue<'T>) (newListeners: list<RegisteredEventListener<'T>>) : unit =
        listeners.[queue :> obj :?> Queue<obj>] <- (newListeners :> obj :?> list<RegisteredEventListener<obj>>)

    let unsubscribe (queue: Queue<'T>) (registeredListener: RegisteredEventListener<'T>) : unit =
        let updatedListeners =
            getListeners queue
            |> List.filterNot (fun curr -> curr = registeredListener)

        setListeners queue updatedListeners

    let broadcast (queue: Queue<'T>) (data: 'T) : unit =
        getListeners queue
        |> List.iter (fun curr -> curr.Invoke data)

    member _.On (queue: Queue<'T>) (listener: EventListener<'T>) : OnResult =
        let registeredListener: RegisteredEventListener<'T> = System.Action<'T>(listener)
        setListeners queue (registeredListener :: (getListeners queue))
        {
            Off = fun () -> unsubscribe queue registeredListener
        }

    member this.Once<'T> (queue: Queue<'T>) (listener: EventListener<'T>) : OnResult =
        // jumping through some hoops to get a self reference
        let mutable maybeResult: Option<OnResult> = None

        let wrappedListener = (fun (data: 'T) ->
            listener data
            match maybeResult with
            | Some { Off = off } -> off()
            | None -> failwith "result is none in EventBus' Once implementation, which should never happen"
        )

        let result = this.On queue wrappedListener
        maybeResult <- Some result
        result

    member _.Broadcast (queue: Queue<'T>) (data: 'T) : unit =
        // Navigation events are broadcast immediately otherwise the browser ignores the key modifiers (ctrl/cmd etc.)
        if queue = Queue "navigation" then
            broadcast queue data
        else
            runOnNextTick (fun () -> broadcast queue data)


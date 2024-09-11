module LibClient.Services.Subscription

open LibClient.EventBus
open LibClient.AsyncDataModule

type SubscriptionListener<'T> = AsyncData<'T> -> unit

type UnsubscribeFunction = unit -> unit

type SubscribeResult = {
    Off: UnsubscribeFunction
}

// Has to be stateful, as two distinct places in code need to connect through this
// at distinct points in time. We can probably restructure this class in such a way
// that "impossible states are impossible to represent", while keeping it "shared-by-reference"
// between the two locations, but I've got enough mental gymnastics to go through at the moment,
// and this project needs to be completed.
// Actually, I think it's impossible to do the above. The `Notify` method needs to be defined
// while the subscription is being constructed inside the service, at which point it is definitely
// not callable since only after being returned from the service to the user will the subscription's
// On method be called so that it gains an actual listener. So I'm fairly convinced this is
// impossible to model without a single `failwith`. Tejas?
type Subscription<'T>() =
    let mutable maybeListener: Option<SubscriptionListener<'T>> = None
    let mutable maybeEventBusOnResult: Option<OnResult> = None

    member _.Off (): unit =
        match maybeEventBusOnResult with
        | None -> failwith "Trying to unsubscribe from a subscription without OnResult"
        | Some onResult -> onResult.Off()

    member _.On (listener: SubscriptionListener<'T>): unit =
        maybeListener <-
            match maybeListener with
            | None -> Some(listener)
            | Some _ -> failwith "A subscription already has a listener, somebody is calling On more than once"

    // TODO reduce visibility to only be accessible to SubscriptionService
    member _.SetEventBusOnResult (onResult: OnResult): unit =
        maybeEventBusOnResult <-
            match maybeEventBusOnResult with
            | None -> Some(onResult)
            | Some _ -> failwith "A subscription already has onResult, somebody is calling SetEventBusOnResult more than once"

    // TODO reduce visibility to only be accessible to SubscriptionService
    member _.Notify (data: AsyncData<'T>) =
        match maybeListener with
        | None -> failwith "Trying to notify a subscription that nobody listens to"
        | Some listener -> listener data


type Subscribers<'T>(maybeOnZeroSubscribers: Option<Subscribers<'T> -> unit>) =
    let mutable subscribers: List<System.Action<'T>> = []

    member this.Add (subscriber: 'T -> unit) : SubscribeResult =
        let wrappedSubscriber = System.Action<'T>(subscriber)
        subscribers <- wrappedSubscriber :: subscribers
        {
            Off = fun () ->
                let updatedSubscribers = subscribers |> List.without wrappedSubscriber
                subscribers <- updatedSubscribers

                match maybeOnZeroSubscribers, updatedSubscribers with
                | Some onZeroSubscribers, [] -> onZeroSubscribers this
                | _ -> ()
        }

    member _.NotifyAll (t: 'T) : unit =
        subscribers |> List.iter (fun curr -> curr.Invoke t)

    member _.HasSubscribers : bool =
        subscribers.IsNonempty

// TODO seriously struggling for a good name here. In promise land,
// they "cheat" by calling the public-facing one a "promise" and the
// internal facing one a "deferred", which is nice and readable.
// In Scala land they have a somewhat less clear naming scheme of
// "future" and "promise".
type AdHocSubscriptionImplementation<'T>(initialValue: Option<'T>, maybeOnZeroSubscribers: Option<Subscribers<'T> -> unit>) =
    let subscribers = Subscribers<'T>(maybeOnZeroSubscribers)
    let mutable lastValue: Option<'T> = initialValue
    let mutable maybeRunOnTransitionFromZeroToOneSubscriber: Option<unit -> unit> = None

    member _.SetCallbackToRunOnTransitionFromZeroToOneSubscriber (fn: unit -> unit) : unit =
        maybeRunOnTransitionFromZeroToOneSubscriber <- Some fn

    member _.Subscribe (subscriber: 'T -> unit) : SubscribeResult =
        match (subscribers.HasSubscribers, maybeRunOnTransitionFromZeroToOneSubscriber) with
        | (false, Some fn) ->
            fn ()
            maybeRunOnTransitionFromZeroToOneSubscriber <- None
        | _ -> Noop

        let result = subscribers.Add subscriber
        match lastValue with
        | Some value -> subscriber value
        | None       -> ()
        result

    member _.Update (t: 'T) : unit =
        lastValue <- Some t
        subscribers.NotifyAll t

    member _.UpdateAndResetLastValue (t: 'T) : unit =
        lastValue <- None
        subscribers.NotifyAll t

    member _.MaybeUpdate (updater: Option<'T> -> Option<'T>) : unit =
        match updater lastValue with
        | None -> Noop
        | Some updatedValue ->
            lastValue <- Some updatedValue
            subscribers.NotifyAll updatedValue

    member _.LatestValue : Option<'T> =
        lastValue

    member _.HasSubscribers : bool =
        subscribers.HasSubscribers
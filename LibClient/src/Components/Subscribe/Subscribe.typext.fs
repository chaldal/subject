module LibClient.Components.Subscribe

open LibClient
open LibClient.Services.Subscription
open Fable.React
open System

// NEW hook implementation

type SubscriptionDataStoreEntry = {
    CurrentValue: obj
    Unsubscribe:  unit -> unit
}

type SubscriptionsDataStore =
| SubscriptionsDataStore of IStateHook<Map<string, SubscriptionDataStoreEntry>>
with
    member this.Subscribe (key: string) (subscription: (LibClient.AsyncDataModule.AsyncData<'T> -> unit) -> SubscribeResult) : AsyncData<'T> =
        let (SubscriptionsDataStore state) = this
        let entry =
            match state.current.TryFind key with
            | None ->
                // we need this because some subscription implementations call the subscriber
                // with a value immediately, in which case we wouldn't have added it to the map
                // yet, causing a "receiving an update for something we haven't subscribed to" error.
                let mutable maybeUnsubscribe = None

                let entry = {
                    CurrentValue = (AsyncData<'T>.Uninitialized :> obj)
                    Unsubscribe  = fun () -> maybeUnsubscribe |> Option.sideEffect (fun off -> off ())
                }
                state.update (state.current.Add (key, entry))

                maybeUnsubscribe <- (subscription (fun value ->
                    (this.Update key) value
                )).Off |> Some
                entry

            | Some entry -> entry

        entry.CurrentValue :?> AsyncData<'T>

    member private this.Update (key: string) (value: AsyncData<'T>) : unit =
        let (SubscriptionsDataStore state) = this
        state.update (fun map ->
            match map.TryFind key with
            | None       -> failwith $"Trying to update entry for a key {key} which we never subscribed on"
            | Some entry -> map.AddOrUpdate (key, { entry with CurrentValue = value :> obj })
        )

let useSubscriptions () : SubscriptionsDataStore =
    let store = Hooks.useStateLazy (fun () -> Map.empty) |> SubscriptionsDataStore

    Hooks.useEffectDisposable (
        (fun () ->
            { new IDisposable with
                member _.Dispose() =
                    let (SubscriptionsDataStore state) = store
                    state.current |> Map.iter (fun _ value ->
                        value.Unsubscribe ()
                    )
            }
        ),
        [||] // run only once
    )

    store

// LEGACY component implementation

type OnSubscriptionKeyChange =
| ShowCurrentDataAsFetching
| ShowCurrentDataAsAvailable

[<RequireQualifiedAccess>]
type With<'T> =
| Raw           of (AsyncData<'T> -> ReactElement)
| WhenAvailable of ('T -> ReactElement)

let Raw           = With.Raw
let WhenAvailable = With.WhenAvailable

type Props<'T> = (* GenerateMakeFunction *) {
    Subscription:            (LibClient.AsyncDataModule.AsyncData<'T> -> unit) -> SubscribeResult
    SubscriptionKey:         string option // defaultWithAutoWrap None
    OnSubscriptionKeyChange: OnSubscriptionKeyChange // default OnSubscriptionKeyChange.ShowCurrentDataAsFetching
    With:                    With<'T>

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'T when 'T: equality> = {
    Value: AsyncData<'T>
}

type Subscribe<'T when 'T: equality>(_initialProps) =
    inherit EstatefulComponent<Props<'T>, Estate<'T>, Actions<'T>, Subscribe<'T>>("LibClient.Components.Subscribe", _initialProps, Actions<'T>, hasStyles = false)

    let mutable maybeLastSubscribeResult: Option<SubscribeResult> = None

    let unsubscribeIfSubscribed () : unit =
        maybeLastSubscribeResult |> Option.sideEffect (fun lastSubscribeResult -> lastSubscribeResult.Off())
        maybeLastSubscribeResult <- None

    member this.Subscribe (subscription: (LibClient.AsyncDataModule.AsyncData<'T> -> unit) -> SubscribeResult) : unit =
        unsubscribeIfSubscribed ()

        maybeLastSubscribeResult <- Some (
            subscription (fun updatedAD ->
                let updatedValue =
                    match (this.state.Value, updatedAD) with
                    | (Available oldAvailableValue, WillStartFetchingSoonHack) ->
                        match this.props.OnSubscriptionKeyChange with
                        | ShowCurrentDataAsAvailable -> Available oldAvailableValue
                        | ShowCurrentDataAsFetching  -> Fetching (Some oldAvailableValue)
                    | _ -> updatedAD

                if this.state.Value <> updatedValue then
                    this.SetEstate (fun estate _ -> { estate with Value = updatedValue } )
            )
        )

    override _.GetInitialEstate(_initialProps: Props<'T>) : Estate<'T> = {
        Value = WillStartFetchingSoonHack
    }

    // NOTE this component is experimental, and we are not bothering to currently
    // rerun the subscription (and deal with managing multiple instances, unsubscribing, etc)
    // We'll just assume that in our experimental use cases we won't run into the issue
    // of a subscription changing on us.
    override this.ComponentDidMount () : unit =
        this.Subscribe this.props.Subscription

    override this.ComponentWillUnmount () : unit =
        base.ComponentWillUnmount ()
        unsubscribeIfSubscribed()

    override this.ComponentWillReceiveProps (nextProps: Props<'T>) : unit =
        if nextProps.SubscriptionKey <> this.props.SubscriptionKey then
            this.Subscribe nextProps.Subscription


and Actions<'T when 'T: equality>(_this: Subscribe<'T>) =
    class end

let Make = makeConstructor<Subscribe<'T>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

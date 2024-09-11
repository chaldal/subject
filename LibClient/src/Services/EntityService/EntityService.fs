module LibClient.Services.EntityService

open LibClient.EventBus
open LibClient.Services.Subscription
open LibClient.AsyncDataModule
open LibClient.JsInterop

type ServiceEvent<'Id, 'Entity> =
| ModificationStarted of id: 'Id * maybeOldValue: Option<'Entity>
| Updated of id: 'Id
| Created of id: 'Id
| Deleted of id: 'Id
| AllUpdated

[<AbstractClass>]
type EntitySubscribableService<'Id, 'Entity>(eventQueueId: string) =
    let eventQueue: Queue<ServiceEvent<'Id, 'Entity>> = Queue eventQueueId
    member _.EventQueue = eventQueue

    abstract member Get: 'Id -> Async<'Entity>

[<AbstractClass>]
type EntityService<'Id, 'Entity> (eventQueueId: string) =
    inherit EntitySubscribableService<'Id, 'Entity>(eventQueueId)

    abstract member Update: 'Id * ((* updater *) 'Entity -> 'Entity) -> unit
    abstract member MaybeUpdate: 'Id * ((* updater *) 'Entity -> Option<'Entity>) -> unit

    abstract member Gets: seq<'Id> -> Async<seq<'Entity>>

[<AbstractClass>]
type EntitySubscriptionService<'Id, 'Entity when 'Id : equality> (eventBus: EventBus, entityService: EntitySubscribableService<'Id, 'Entity>) =
    // have to make it a member just so that I can have named arguments, which are
    // apparently not available on `let` bound values... wth?
    member private _.UpdateSubscriberWithLatestEntity (id: 'Id, notifySubscriber: AsyncData<'Entity> -> unit, shouldNotifyFetching: bool) : unit =
        async {
            if shouldNotifyFetching then Fetching None |> notifySubscriber
            match! entityService.Get id |> Async.TryCatch with
            | Ok entity -> Available entity                                             |> notifySubscriber
            | Error e   -> Failed (UnknownFailure $"An error occurred: {e.ToString()}") |> notifySubscriber
        }
        |> Async.StartImmediate

    member private this.SubscribeToOneProcessEvent (theId: 'Id) (subscription: Subscription<'Entity>) (event: ServiceEvent<'Id, 'Entity>) : unit =
        match event with
        | ModificationStarted(id, maybeOldValue) -> if id = theId then Fetching maybeOldValue |> subscription.Notify
        | Updated id -> if id = theId then this.UpdateSubscriberWithLatestEntity(theId, subscription.Notify, shouldNotifyFetching = false)
        | Created id -> if id = theId then this.UpdateSubscriberWithLatestEntity(theId, subscription.Notify, shouldNotifyFetching = false)
        | Deleted id -> if id = theId then Unavailable |> subscription.Notify
        | AllUpdated -> this.UpdateSubscriberWithLatestEntity(theId, subscription.Notify, shouldNotifyFetching = false)
        // We specifically don't want to use conditions within the pattern, as that
        // would necessitate a catch-all clause, which would remove the safety of
        // having union type exhaustiveness check.

    member this.SubscribeToOne (theId: 'Id) : Subscription<'Entity> =
        let subscription = Subscription<'Entity>()

        runOnNextTick (fun () ->
            this.UpdateSubscriberWithLatestEntity(theId, subscription.Notify, shouldNotifyFetching = true)
        )

        let onResult = eventBus.On entityService.EventQueue (this.SubscribeToOneProcessEvent theId subscription)
        subscription.SetEventBusOnResult onResult

        subscription

[<AbstractClass>]
type StateMachineEntityService<'Id, 'Entity, 'Action> (eventQueueId: string, eventBus: EventBus) =
    inherit EntitySubscribableService<'Id, 'Entity>(eventQueueId)

    abstract member PerformBackendAction: 'Id -> 'Action -> Async<unit>

    member this.PerformAction (id: 'Id) (action: 'Action) : Async<unit> = async {
        eventBus.Broadcast this.EventQueue (ModificationStarted(id, None))
        do! this.PerformBackendAction id action
        eventBus.Broadcast this.EventQueue (Updated id)
    }

[<AbstractClass>]
type CachedStateMachineEntityService<'Id, 'Entity, 'Action when 'Id : comparison> (eventQueueId: string, eventBus: EventBus) =
    inherit EntitySubscribableService<'Id, 'Entity>(eventQueueId)

    let mutable cache: Map<'Id, 'Entity> = Map.empty

    abstract member BackendPerformAction: 'Id -> 'Action -> Async<unit>

    // NOTE this pair of methods are necessary to support the case where two subsequent
    // actions are expected on an entity, and we do not want the cache invalidated
    // (and thus spinners spinning over empty space) in between the two actions.
    // Essentially it's sort of an "actions session" which we close by calling the
    // FinishedPerformingSeriesOfActions method.
    member this.PerformOneOfSeriesOfActions (id: 'Id) (action: 'Action) : Async<unit> = async {
        eventBus.Broadcast this.EventQueue (ModificationStarted(id, this.GetCached id))
        do! this.BackendPerformAction id action
    }

    member this.FinishedPerformingSeriesOfActions (id: 'Id) : Async<unit> = async {
        this.InvalidateCache id
        eventBus.Broadcast this.EventQueue (Updated id)
    }

    member this.PerformAction (id: 'Id) (action: 'Action) : Async<unit> = async {
        eventBus.Broadcast this.EventQueue (ModificationStarted(id, this.GetCached id))
        do! this.BackendPerformAction id action
        this.InvalidateCache id
        eventBus.Broadcast this.EventQueue (Updated id)
    }

    member (* protected *) _.InvalidateCache (id: 'Id) : unit =
        cache <- cache.Remove id

    member (* protected *) _.GetCached (id: 'Id) : Option<'Entity> =
        cache.TryFind id

    member (* protected *) _.Cache (id: 'Id) (entity: 'Entity) : unit =
        cache <- cache.Add(id, entity)

    override this.Get (id: 'Id) : Async<'Entity> = async {
        match cache.TryFind id with
        | None ->
            let! entity = this.BackendGet id
            this.Cache id entity
            return entity
        | Some entity -> return entity
    }

    abstract member BackendGet: 'Id -> Async<'Entity>

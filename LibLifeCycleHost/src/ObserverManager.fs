// F# adaptation of https://gist.github.com/ReubenBond/0bb430a52eedce50e6976f2f89154f5a#file-observermanager-cs, which is itself an unofficial
// replacement for Orleans' ObserverSubscriptionManager. See https://github.com/dotnet/orleans/issues/3581 for discussion.

namespace LibLifeCycleHost

open System
open System.Collections
open System.Collections.Generic

type private ObserverEntry<'Observer> = {
    mutable Observer: 'Observer
    mutable LastSeen: DateTimeOffset
}

type ObserverManager<'Address, 'Observer when 'Address : comparison>
    (
        autoExpiration: TimeSpan,
        logger: IFsLogger,
        getNow: unit -> DateTimeOffset,
        onNonEmpty: unit -> unit,
        onEmpty: unit -> unit
    ) =

    let observerEntriesByAddress = Dictionary<'Address, ObserverEntry<'Observer>>()

    let observerCount () =
        observerEntriesByAddress.Count

    let addOrUpdateObserver address observer =
        let now = getNow ()

        match observerEntriesByAddress.TryGetValue(address) with
        | true, observerEntry ->
            logger.Trace "Updating entry for %a/%a. %a total observers." (logger.P "address") address (logger.P "observer") observer (logger.P "observerCount") observerEntriesByAddress.Count
            observerEntry.LastSeen <- now
            observerEntry.Observer <- observer
            ()
        | false, _ ->
            logger.Trace "Adding entry for %a/%a. %a total observers before add." (logger.P "address") address (logger.P "observer") observer (logger.P "observerCount") observerEntriesByAddress.Count
            observerEntriesByAddress.[address] <- { LastSeen = now; Observer = observer }

            if observerEntriesByAddress.Count = 1 then onNonEmpty ()

    let removeObserver (address: 'Address) =
        match observerEntriesByAddress.Remove (address) with
        | true ->
            if observerEntriesByAddress.Count = 0 then onEmpty ()

            logger.Trace "Removed entry for %a. %a total observers after remove." (logger.P "address") address (logger.P "observerCount") observerEntriesByAddress.Count
        | false ->
            logger.Trace "No entry for %a, so could not remove it. %a total observers." (logger.P "address") address (logger.P "observerCount") observerEntriesByAddress.Count

    let hasExpired now observerEntry =
        observerEntry.LastSeen + autoExpiration < now

    let notifyObservers predicate (notification: 'Observer -> unit) =
        logger.Trace "Notifying observers"
        let now = getNow ()

        let expiredObservers =
            observerEntriesByAddress
            |> Seq.map
                (fun kvp ->
                    match hasExpired now kvp.Value with
                    | true ->
                        kvp.Key |> Some
                    | false ->
                        let shouldNotify = predicate kvp.Value.Observer

                        match shouldNotify with
                        | true ->
                            try
                                // Side effect: perform the notification.
                                notification kvp.Value.Observer

                                // Notification succeeded, so ignore this observer until next time.
                                None
                            with
                            | _ ->
                                // Notification failed, so expire the observer.
                                Some kvp.Key
                        | false ->
                            None
                )

        expiredObservers
        |> Seq.iter (function
            | Some address -> removeObserver address
            | None -> Noop)

    let clearExpiredObservers () =
        logger.Trace "Clearing expired observers"

        let now = getNow ()

        observerEntriesByAddress
        |> Seq.choose
            (fun kvp ->
                match hasExpired now kvp.Value with
                | true -> Some kvp.Key
                | false -> None
            )
        |> Seq.iter removeObserver

    member _.Count =
        observerCount ()

    member _.AddOrUpdateObserver (address: 'Address) (observer: 'Observer) =
        addOrUpdateObserver address observer

    member _.RemoveObserver (address: 'Address) =
        removeObserver address

    member _.NotifyObservers (predicate: 'Observer -> bool) (notification: 'Observer -> unit) =
        notifyObservers predicate notification

    member _.ClearExpiredObservers () =
        clearExpiredObservers ()

    interface IEnumerable<'Observer> with
        member _.GetEnumerator(): IEnumerator<'Observer> =
            observerEntriesByAddress
            |> Seq.map (fun o -> o.Value.Observer)
            |> (fun seq -> seq.GetEnumerator())

        member this.GetEnumerator(): IEnumerator =
            (this :> IEnumerable<'Observer>).GetEnumerator() :> IEnumerator

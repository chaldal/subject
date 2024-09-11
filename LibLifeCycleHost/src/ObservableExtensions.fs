[<AutoOpen>]
module LibLifeCycleHost.ObservableExtensions

open System
open System.Reactive.Disposables
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Runtime.CompilerServices
open System.Threading

[<Extension>]
type ObservableExtensions() =
    /// Automatically completes <paramref name="observable"/> when <paramref name="cancellationToken"/> is cancelled.
    [<Extension>]
    static member TakeUntilCancelled(observable: IObservable<'T>, cancellationToken: CancellationToken) =
        let cancelSignal =
            Observable
                .Create(
                    (fun (observer: IObserver<unit>) ->
                        cancellationToken.Register(fun () -> observer.OnNext(()))
                        :> IDisposable
                    )
                )
        observable.TakeUntil(cancelSignal)

    /// Invokes <paramref name="callback"/> whenever <paramref name="observable"/> is disconnected, which could be multiple times throughout
    /// its life cycle.
    [<Extension>]
    static member OnConnectableDisconnected(observable: IConnectableObservable<'T>, callback: (unit -> unit)) =
        {
            new IConnectableObservable<'T> with
                member _.Subscribe(observer) =
                    observable.Subscribe(observer)

                member _.Connect() =
                    let connectDisposable = observable.Connect()
                    Disposable.Create(
                        fun () ->
                            connectDisposable.Dispose()
                            callback ()
                    )
        }

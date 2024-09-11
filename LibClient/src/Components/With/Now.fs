[<AutoOpen>]
module LibClient.Components.With_Now

open System
open Fable.React
open LibClient
open LibClient.Components
open LibClient.ServiceInstances

type LC.With with
    [<Component>]
    static member Now (``with``: DateTimeOffset -> ReactElement, ?updateFrequency: TimeSpan) : ReactElement =
        let updateFrequency = defaultArg updateFrequency (TimeSpan.FromSeconds 1.0)
        let nowState = Hooks.useStateLazy (fun () -> services().Date.GetNow)

        Hooks.useEffectDisposable (
            (fun () ->
                let subscribeResult =
                    services().Date.SubscribeToNow
                        (fun now ->
                            (*
                                React state updates are asynchronous. So, nowState.current is not being updated immediately.
                                To address this, we are using the callback form of the state update which provides the
                                previous state as an argument. We are using that prev state to calculate the time difference.
                            *)
                            nowState.update (fun prevNow ->
                                if now - prevNow >= updateFrequency then
                                    now
                                else
                                    prevNow
                            )
                        )

                { new IDisposable with
                    member _.Dispose() =
                        subscribeResult.Off ()
                }
            ),
            [||] // run only once
        )

        ``with`` nowState.current
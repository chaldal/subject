[<AutoOpen>]
module LibClient.Components.Async_Resolve

open Fable.React

open LibClient
open LibClient.Components

type LibClient.Components.Constructors.LC.Async with
    [<Component>]
    static member Resolve<'T>(
            data: Async<AsyncData<'T>>,
            whenAvailable: 'T -> ReactElement,
            ?whenUninitialized: unit -> ReactElement,
            ?whenFetching: Option<'T> -> ReactElement,
            ?whenFailed: AsyncDataFailure -> ReactElement,
            ?whenUnavailable: unit -> ReactElement,
            ?whenAccessDenied: unit -> ReactElement,
            ?whenElse: unit -> ReactElement,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let dataHook = Hooks.useState WillStartFetchingSoonHack

        Hooks.useEffect(
            (fun () ->
                // TODO: Should this trap failures and surface as AsyncData.Failed rather than use startSafely?
                async {
                    let! resolved = data
                    dataHook.update resolved
                } |> startSafely
            ),
            [||]
        )

        LC.AsyncData(
            data = dataHook.current,
            whenAvailable = whenAvailable,
            ?whenUninitialized = whenUninitialized,
            ?whenFetching = whenFetching,
            ?whenFailed= whenFailed,
            ?whenUnavailable = whenUnavailable,
            ?whenAccessDenied = whenAccessDenied,
            ?whenElse = whenElse
        )
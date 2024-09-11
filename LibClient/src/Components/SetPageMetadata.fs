[<AutoOpen>]
module LibClient.Components.SetPageMetadata

open Fable.React

open LibClient
open LibClient.ServiceInstances
open LibClient.Services.PageTitleService

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member SetPageMetadata(?title: string, ?description: string, ?key: string) : ReactElement =
        key |> ignore

        let maybeLastSetReceiptHook = Hooks.useRef<Option<SetReceipt>> None

        Hooks.useEffectDisposableFn(
            (fun () ->
                title
                |> Option.sideEffect (fun title ->
                    maybeLastSetReceiptHook.current <- services().PageTitle.SetRouteName title |> Some
                )
            ),
            (fun () ->
                maybeLastSetReceiptHook.current
                |> Option.sideEffect (services().PageTitle.RestoreIfStillTheSame)
            ),
            [| title; description |]
        )

        noElement
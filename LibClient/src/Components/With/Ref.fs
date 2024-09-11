[<AutoOpen>]
module LibClient.Components.With_Ref

open Fable.React
open LibClient
open LibClient.Components

let private bindRef<'T> (refState: IStateHook<Option<'T>>) (onInitialize: Option<'T -> unit>) (nullableInstance: LibClient.JsInterop.JsNullable<'T>) =
    let maybeNewRef = nullableInstance.ToOption

    match (refState.current, maybeNewRef, onInitialize) with
    | (None, Some ref, Some onInitialize) -> onInitialize ref
    | _ -> Noop

    refState.update maybeNewRef

type LC.With with
    [<Component>]
    static member Ref<'T> (``with``: (((* bindRef *) LibClient.JsInterop.JsNullable<'T> -> unit) * Option<'T>) -> ReactElement, ?onInitialize: ('T -> unit)) : ReactElement =
        let refState = Hooks.useState<Option<'T>> None

        // We need to ensure the function is not recreated on each render. Note that callers should likewise be using useMemo on the value they pass in
        // for onInitialize, otherwise we'll receive a new callback on each invocation, meaning the memo will be recreated.
        let bindRefMemo =
            Hooks.useMemo(
                (fun () -> bindRef refState onInitialize),
                [| onInitialize |]
            )

        ``with`` (bindRefMemo, refState.current)
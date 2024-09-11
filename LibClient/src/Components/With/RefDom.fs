[<AutoOpen>]
module LibClient.Components.With_RefDom

open Fable.React
open LibClient
open LibClient.Components

let private bindRef (refState: IStateHook<Option<Browser.Types.Element>>) (onInitialize: Option<Browser.Types.Element -> unit>) (instance: Browser.Types.Element) =
    let nullableInstance = instance :> obj :?> LibClient.JsInterop.JsNullable<Browser.Types.Element>
    let maybeNewRef = nullableInstance.ToOption

    match (refState.current, maybeNewRef, onInitialize) with
    | (None, Some ref, Some onInitialize) -> onInitialize ref
    | _ -> Noop

    refState.update maybeNewRef

type LC.With with
    [<Component>]
    static member RefDom (``with``: (((* bindRef *) Browser.Types.Element -> unit) * Option<Browser.Types.Element>) -> ReactElement, ?onInitialize: Browser.Types.Element -> unit) : ReactElement =
        let refState = Hooks.useState<Option<Browser.Types.Element>> None

        // We need to ensure the function is not recreated on each render. Note that callers should likewise be using useMemo on the value they pass in
        // for onInitialize, otherwise we'll receive a new callback on each invocation, meaning the memo will be recreated.
        let bindRefMemo =
            Hooks.useMemo(
                (fun () -> bindRef refState onInitialize),
                [| onInitialize |]
            )

        ``with`` (bindRefMemo, refState.current)


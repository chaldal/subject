module LibClient.Components.Dialog.Base

open LibClient
open Fable.Core
open Browser.Types


[<Emit("undefined")>]
let Undefined: Browser.Types.PointerEvent -> unit = jsNative

type ContentPosition =
| Free
| Center
| CenterTop

type CloseAction =
| OnEscape
| OnBackground
| OnCloseButton

type CanClose =
| When of List<CloseAction> * (ReactEvent.Action -> unit)
| Never
with
    member this.ShouldShowCloseButton : bool =
        match this with
        | When (value, _) -> value |> List.contains OnCloseButton
        | Never           -> false

    member this.OnClose (e: ReactEvent.Action) =
        match this with
        | When (_, action) ->
            e.MaybeEvent |> Option.sideEffect (fun event -> event.stopPropagation())
            action e
        | Never -> Noop


type Props = (* GenerateMakeFunction *) {
    CanClose:        CanClose
    ContentPosition: ContentPosition
}

type Estate = NoEstate
type Pstate = NoPstate

type Base(initialProps) =
    inherit PureStatelessComponent<Props, Actions, Base>("LibClient.Components.Dialog.Base", initialProps, Actions, hasStyles = true)

and Actions(this: Base) =
    member _.OnBackgroundPress (e: Browser.Types.PointerEvent) : unit =
        match this.props.CanClose with
        | When (value, action) when List.contains OnBackground value -> action (ReactEvent.Action.OfBrowserEvent e)
        | _ -> Noop

    member _.OnKeyPress (e: Browser.Types.KeyboardEvent) : unit =
        match (this.props.CanClose, e.key) with
        | (When (value, action), KeyboardEvent.Key.Escape) when List.contains OnEscape value -> action (ReactEvent.Action.OfBrowserEvent e)
        | _ -> Noop


let Make = makeConstructor<Base,_,_>

module LibClient.Components.Dialog.Shell.WhiteRounded.Raw

open LibClient
open Fable.Core
open Browser.Types

[<Emit("undefined")>]
let Undefined: Browser.Types.PointerEvent -> unit = jsNative

type CanClose = LibClient.Components.Dialog.Base.CanClose
let When      = CanClose.When

type CloseAction  = LibClient.Components.Dialog.Base.CloseAction
let OnEscape      = CloseAction.OnEscape
let OnBackground  = CloseAction.OnBackground
let OnCloseButton = CloseAction.OnCloseButton

type DialogPosition =
| Top
| Center
| Bottom

type Props = (* GenerateMakeFunction *) {
    CanClose:   CanClose
    Position:   DialogPosition // default Center
    InProgress: bool           // default false
}

type Raw(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Raw>("LibClient.Components.Dialog.Shell.WhiteRounded.Raw", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Raw, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

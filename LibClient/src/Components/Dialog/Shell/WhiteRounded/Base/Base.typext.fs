module LibClient.Components.Dialog.Shell.WhiteRounded.Base

open LibClient

type CanClose = LibClient.Components.Dialog.Base.CanClose
let When      = CanClose.When

type CloseAction = LibClient.Components.Dialog.Base.CloseAction
let OnEscape      = CloseAction.OnEscape
let OnBackground  = CloseAction.OnBackground
let OnCloseButton = CloseAction.OnCloseButton

type Props = (* GenerateMakeFunction *) {
    CanClose:   CanClose
    InProgress: bool         // default false
    key: string option // defaultWithAutoWrap JsUndefined
}

type Base(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Base>("LibClient.Components.Dialog.Shell.WhiteRounded.Base", _initialProps, Actions, hasStyles = true)

and Actions(_this: Base) =
    class end

let Make = makeConstructor<Base, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

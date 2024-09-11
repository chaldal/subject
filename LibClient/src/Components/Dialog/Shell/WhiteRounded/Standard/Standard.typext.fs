module LibClient.Components.Dialog.Shell.WhiteRounded.Standard

open Fable.React
open LibClient


type Mode =
| Default
| InProgress
| Error of Message: string

type CanClose = LibClient.Components.Dialog.Base.CanClose
let When      = CanClose.When
let Never     = CanClose.Never

type CloseAction = LibClient.Components.Dialog.Base.CloseAction
let OnEscape      = CloseAction.OnEscape
let OnBackground  = CloseAction.OnBackground
let OnCloseButton = CloseAction.OnCloseButton

type Props = (* GenerateMakeFunction *) {
    Heading:  string option // defaultWithAutoWrap None
    Body:     ReactElement  // default noElement
    Buttons:  ReactElement  // default noElement
    Mode:     Mode          // default Default
    CanClose: CanClose

    key: string option // defaultWithAutoWrap JsUndefined
}

type Standard(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Standard>("LibClient.Components.Dialog.Shell.WhiteRounded.Standard", _initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Standard, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

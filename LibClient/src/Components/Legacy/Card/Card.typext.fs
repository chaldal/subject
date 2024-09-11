module LibClient.Components.Legacy.Card

open LibClient
open ReactXP.Styles

type Style =
| Shadowed
| Flat

type Props = (* GenerateMakeFunction *) {
    OnPress:  (ReactEvent.Action -> unit) option // defaultWithAutoWrap None
    Style:    Style                              // default Style.Shadowed

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = NoEstate
type Pstate = NoPstate

type Card(initialProps) =
    inherit PureStatelessComponent<Props, Actions, Card>("LibClient.Components.Legacy.Card", initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Card,_,_>

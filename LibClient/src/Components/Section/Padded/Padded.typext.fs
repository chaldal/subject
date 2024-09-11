module LibClient.Components.Section.Padded

open LibClient
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {
    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Padded(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Padded>("LibClient.Components.Section.Padded", _initialProps, Actions, hasStyles = true)

and Actions(_this: Padded) =
    class end

let Make = makeConstructor<Padded, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

module LibClient.Components.Nav.Top.Heading

open LibClient
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {
    Text: string
    styles: array<TextStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Heading(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Heading>("LibClient.Components.Nav.Top.Heading", _initialProps, Actions, hasStyles = true)

and Actions(_this: Heading) =
    class end

let Make = makeConstructor<Heading, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

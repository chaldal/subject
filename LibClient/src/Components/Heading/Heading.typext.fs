module LibClient.Components.Heading

open LibClient
open ReactXP.Styles

type Level =
| Primary
| Secondary
| Tertiary

type Props = (* GenerateMakeFunction *) {
    Level:  Level        // default Primary
    styles: array<TextStyles> option // defaultWithAutoWrap None
}

type Estate = NoEstate
type Pstate = NoPstate

type Heading(initialProps) =
    inherit PureStatelessComponent<Props, Actions, Heading>("LibClient.Components.Heading", initialProps, NoActions, hasStyles = true)

and Actions = unit

let Make = makeConstructor<Heading,_,_>

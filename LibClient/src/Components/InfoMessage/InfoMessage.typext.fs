module LibClient.Components.InfoMessage

open LibClient
open ReactXP.Styles

type Level =
| Info
| Attention
| Caution

type Props = (* GenerateMakeFunction *) {
    Level:   Level // default Level.Info
    Message: string

    styles: array<TextStyles> option // defaultWithAutoWrap None
    key:    string            option // defaultWithAutoWrap JsUndefined
}

type InfoMessage(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, InfoMessage>("LibClient.Components.InfoMessage", _initialProps, Actions, hasStyles = true)

and Actions(_this: InfoMessage) =
    class end

let Make = makeConstructor<InfoMessage, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

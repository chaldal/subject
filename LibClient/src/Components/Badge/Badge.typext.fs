module LibClient.Components.Badge

open LibClient
open ReactXP.Styles

let Text      = Badge.Text
let Count     = Badge.Count
let NoContent = Badge.NoContent

type Props = (* GenerateMakeFunction *) {
    Badge: Output.Badge
    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Badge(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Badge>("LibClient.Components.Badge", _initialProps, Actions, hasStyles = true)

and Actions(_this: Badge) =
    class end

let Make = makeConstructor<Badge, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

module LibClient.Components.Sidebar.Item

open LibClient
open ReactXP.Styles

type State =
| Actionable of OnPress: (ReactEvent.Action -> unit)
| InProgress
| Disabled
| Selected
with
    member this.Name : string =
        unionCaseName this

type [<RequireQualifiedAccess>] Right =
| Badge of PositiveInteger
| Icon  of Icons.IconConstructor
| NoElement

type Props = (* GenerateMakeFunction *) {
    Label:      string
    LeftIcon:   Icons.IconConstructor option // defaultWithAutoWrap None
    Right:      Right                 option // defaultWithAutoWrap None
    State:      State
    styles:     array<ViewStyles> option // defaultWithAutoWrap None
}

type Item(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Item>("LibClient.Components.Sidebar.Item", _initialProps, Actions, hasStyles = true)

and Actions(_this: Item) =
    class end

let Make = makeConstructor<Item, _, _>

// Unfortunately necessary boilerplate
type Estate = unit
type Pstate = unit

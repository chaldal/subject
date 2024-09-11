module LibClient.Components.Nav.Bottom.Item

open LibClient
open ReactXP.Styles

type Badge = LibClient.Output.Badge
let Text  = Badge.Text
let Count = Badge.Count

type LabelPosition =
| SideLabel
| BottomLabel

type Style =
| Internal of MaybeLabel: Option<string> * MaybeIcon: Option<LibClient.Icons.IconConstructor> * LabelPosition * MaybeBadge: Option<Badge>
with
    static member With (?label: string, ?icon: LibClient.Icons.IconConstructor, ?labelPosition: LabelPosition, ?badge: Badge) : Style =
        let labelPosition = defaultArg labelPosition LabelPosition.SideLabel
        Internal (label, icon, labelPosition, badge)

type State =
| Actionable of OnPress: (ReactEvent.Action -> unit)
| InProgress
| Disabled
| Selected
| SelectedActionable of OnPress: (ReactEvent.Action -> unit)
with
    member this.Name : string =
        unionCaseName this

let labelOnly (label: string)                          : Style = Style.With(label = label)
let iconOnly  (icon:  LibClient.Icons.IconConstructor) : Style = Style.With(icon  = icon)

type Props = (* GenerateMakeFunction *) {
    State: State
    Style: Style
    styles: array<ViewStyles> option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

type Item(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Item>("LibClient.Components.Nav.Bottom.Item", _initialProps, Actions, hasStyles = true)

and Actions(_this: Item) =
    class end

let Make = makeConstructor<Item, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

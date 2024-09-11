module LibClient.Components.Legacy.Sidebar.Item

open LibClient

type [<RequireQualifiedAccess>] Right =
| Count of int
| Icon  of (int -> LibClient.Icons.Icon)

type Value =
| Primary   of Label: string * MaybeLeftIcon: Option<int -> LibClient.Icons.Icon> * MaybeRight: Option<Right>
| Secondary of Label: string

type Props = (* GenerateMakeFunction *) {
    Value:      Value
    IsSelected: bool
    OnPress:    (ReactEvent.Action -> unit)
}

type Item(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Item>("LibClient.Components.Legacy.Sidebar.Item", _initialProps, Actions, hasStyles = true)

and Actions(this: Item) =
    member _.OnPress (e: Browser.Types.PointerEvent) : unit =
        (ReactEvent.Pointer.OfBrowserEvent e).WithSource (this :> ReactElement)
        |> ReactEvent.Action.Make
        |> this.props.OnPress

let Make = makeConstructor<Item, _, _>

// Unfortunately necessary boilerplate
type Estate = unit
type Pstate = unit
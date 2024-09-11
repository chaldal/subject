module LibClient.Components.HandheldListItem

open LibClient

type Props = (* GenerateMakeFunction *) {
    Label:     Label
    State:     State
    LeftIcon:  (int -> LibClient.Icons.Icon) option // default None
    Right:     Right                         option // defaultWithAutoWrap None
}

// TODO generalize this as a state of all OnPressable things (needs to work with backreference ~ properly)
and State =
| Actionable of OnPress: (Browser.Types.Event -> unit)
| InProgress
| Disabled
with
    member this.GetName : string =
        unionCaseName this

and Label =
| Children
| Text of string

and Right =
| Icon          of (int -> LibClient.Icons.Icon)
| Number        of int
| NumberAndIcon of int * (int -> LibClient.Icons.Icon)

type Estate = EmptyRecord

type HandheldListItem(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, HandheldListItem>("LibClient.Components.HandheldListItem", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: HandheldListItem) =
    class end

let Make = makeConstructor<HandheldListItem, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

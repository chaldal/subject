module LibClient.Components.Dialog.Shell.FullScreen

open Fable.React
open LibClient

type BackButton =
| No
| Yes of OnPress: (ReactEvent.Action -> unit)
with
    member this.OnPressOption : Option<ReactEvent.Action -> unit> =
        match this with
        | No          -> None
        | Yes onPress -> Some onPress

type Scroll = NoScroll | Horizontal | Vertical | Both

type Props = (* GenerateMakeFunction *) {
    Heading:       string       option // defaultWithAutoWrap None
    BackButton:    BackButton          // default             BackButton.No
    HeaderRight:   ReactElement option // defaultWithAutoWrap None
    BottomSection: ReactElement option // defaultWithAutoWrap None
    Scroll:        Scroll              // default             Scroll.Vertical

}

type FullScreen(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, FullScreen>("LibClient.Components.Dialog.Shell.FullScreen", _initialProps, Actions, hasStyles = true)

and Actions(_this: FullScreen) =
    class end

let Make = makeConstructor<FullScreen, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

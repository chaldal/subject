module LibClient.Components.FloatingActionButton

open LibClient
open ReactXP.Styles

let Actionable = ButtonLowLevelState.Actionable
let InProgress = ButtonLowLevelState.InProgress
let Disabled   = ButtonLowLevelState.Disabled

type Props = (* GenerateMakeFunction *) {
    Icon:  LibClient.Icons.IconConstructor
    Label: string option // defaultWithAutoWrap None
    State: ButtonHighLevelState
    styles: array<ViewStyles> option // defaultWithAutoWrap None
}

type PropStateFactory = ButtonHighLevelStateFactory

type FloatingActionButton(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, FloatingActionButton>("LibClient.Components.FloatingActionButton", _initialProps, Actions, hasStyles = true)

and Actions(_this: FloatingActionButton) =
    class end

let Make = makeConstructor<FloatingActionButton, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
module LibClient.Components.Nav.Bottom.Button

open LibClient
open ReactXP.Styles
open LibClient.Components.Button

let Actionable = ButtonLowLevelState.Actionable
let InProgress = ButtonLowLevelState.InProgress
let Disabled   = ButtonLowLevelState.Disabled

let Primary = Level.Primary
let Secondary = Level.Secondary
let PrimaryB = Level.PrimaryB
let SecondaryB = Level.SecondaryB
let Tertiary = Level.Tertiary
let Cautionary = Level.Cautionary

type Icon = LibClient.Components.Button.Icon

let Text  = Badge.Text
let Count = Badge.Count

type Props = (* GenerateMakeFunction *) {
    Label: string
    Level: Level        // default Primary
    Icon:  Icon         // default No
    Badge: Badge option // defaultWithAutoWrap None
    State: ButtonHighLevelState
    styles: array<ViewStyles> option // defaultWithAutoWrap None
    contentContainerStyles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type PropStateFactory = ButtonHighLevelStateFactory

type Button(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Button>("LibClient.Components.Nav.Bottom.Button", _initialProps, Actions, hasStyles = true)

and Actions(_this: Button) =
    class end

let Make = makeConstructor<Button, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

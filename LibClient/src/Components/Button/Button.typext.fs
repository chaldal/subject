module LibClient.Components.Button

open LibClient
open ReactXP.Styles

type Level =
| Primary
| Secondary
| PrimaryB
| SecondaryB
| Tertiary
| Cautionary

type ButtonLowLevelState = LibClient.Input.ButtonLowLevelState
let Actionable = ButtonLowLevelState.Actionable
let InProgress = ButtonLowLevelState.InProgress
let Disabled   = ButtonLowLevelState.Disabled

type Icon =
| No
| Left  of LibClient.Icons.IconConstructor
| Right of LibClient.Icons.IconConstructor
with
    member this.LeftOption : Option<LibClient.Icons.IconConstructor> =
        match this with
        | Left icon -> Some icon
        | _         -> None

    member this.RightOption : Option<LibClient.Icons.IconConstructor> =
        match this with
        | Right icon -> Some icon
        | _          -> None

type Badge = LibClient.Output.Badge
let Text  = Badge.Text
let Count = Badge.Count

type Props = (* GenerateMakeFunction *) {
    Label: string
    Level: Level        // default Primary
    Icon:  Icon         // default No
    Badge: Badge option // defaultWithAutoWrap None
    State: ButtonHighLevelState

    styles:        array<ViewStyles> option // defaultWithAutoWrap None
    contentContainerStyles: array<ViewStyles> option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

type PropStateFactory = ButtonHighLevelStateFactory

type Button(initialProps) =
    inherit PureStatelessComponent<Props, Actions, Button>("LibClient.Components.Button", initialProps, Actions, hasStyles = true)

and Actions(_this: Button) =
    class end

let Make = makeConstructor<Button,_,_>

type Estate = NoEstate
type Pstate = NoPstate

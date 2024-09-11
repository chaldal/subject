module LibClient.Components.Input.Checkbox

open LibClient
open ReactXP.Styles
open Fable.React

type Label =
| String of string
| Children

type Props = (* GenerateMakeFunction *) {
    OnChange: bool -> unit
    Value:    Option<bool>
    Label:    Label        // default Children

    Validity: InputValidity

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Checkbox(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Checkbox>("LibClient.Components.Input.Checkbox", _initialProps, Actions, hasStyles = true)

and Actions(this: Checkbox) =
    member _.OnPress (_e: ReactEvent.Action) : unit =
        this.props.OnChange (this.props.Value |> Option.getOrElse false |> not)

let Make = makeConstructor<Checkbox,_,_>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
module LibClient.Components.Input.ChoiceList

open LibClient
open ReactXP.Styles

type SelectableValue<'T when 'T : comparison> = LibClient.Input.SelectableValue<'T>
let AtMostOne  = SelectableValue.AtMostOne
let ExactlyOne = SelectableValue.ExactlyOne
let AtLeastOne = SelectableValue.AtLeastOne
let Any        = SelectableValue.Any

type Group<'T when 'T : comparison> = {
    IsSelected: 'T -> bool
    Toggle:     'T -> ReactEvent.Action -> unit
    Value:      SelectableValue<'T>
}

type Props<'T when 'T : comparison> = (* GenerateMakeFunction *) {
    Items:    Group<'T> -> ReactElement
    Value:    SelectableValue<'T>
    Validity: InputValidity

    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'T when 'T : comparison> = {
    Group: Group<'T>
}

type ChoiceList<'T when 'T : comparison>(_initialProps) =
    inherit EstatefulComponent<Props<'T>, Estate<'T>, Actions<'T>, ChoiceList<'T>>("LibClient.Components.Input.ChoiceList", _initialProps, Actions<'T>, hasStyles = true)

    let stateFromProps (props: Props<'T>) : Estate<'T> = {
        Group = {
            IsSelected = fun (value: 'T) -> props.Value.IsSelected value
            Toggle     = fun (value: 'T) (_: ReactEvent.Action) -> props.Value.Toggle value
            Value      = props.Value
        }
    }

    override this.GetInitialEstate(initialProps: Props<'T>) : Estate<'T> =
        stateFromProps initialProps

    override this.ComponentWillReceiveProps(nextProps: Props<'T>) : unit =
        this.SetEstate (fun _ _ -> stateFromProps nextProps)

and Actions<'T when 'T : comparison>(_this: ChoiceList<'T>) =
    class end

let Make = makeConstructor<ChoiceList<'T>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

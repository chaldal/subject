module LibClient.Components.Input.Picker

open LibClient
open LibClient.Components.Input.PickerModel
open ReactXP.Styles

type SelectableValue<'T when 'T : comparison> = LibClient.Input.SelectableValue<'T>
let AtMostOne  = SelectableValue.AtMostOne
let ExactlyOne = SelectableValue.ExactlyOne
let AtLeastOne = SelectableValue.AtLeastOne
let Any        = SelectableValue.Any

let Static = Items.Static
let Async  = Items.Async

let Default = PickerItemView.Default
let Custom  = PickerItemView.Custom

type Props<'T when 'T : comparison> = (* GenerateMakeFunction *) {
    Label:         string option // defaultWithAutoWrap None
    Placeholder:   string option // defaultWithAutoWrap None
    Items:         Items<'T>
    ItemView:      PickerItemView<'T>
    Value:         SelectableValue<'T>
    Validity:      InputValidity
    ShowSearchBar: bool // default true
    styles: array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'T when 'T : comparison> = {
    PickerId: System.Guid
}

type PropItemViewFactory =
    static member Make<'T when 'T : comparison> (itemToLabel: 'T -> NonemptyString) : PickerItemView<'T> =
        (fun (item: 'T) ->
            {|
                Label = (itemToLabel item).Value
            |}
        )
        |> PickerItemView.Default

    static member Make<'T when 'T : comparison> (itemToLabel: 'T -> string) : PickerItemView<'T> =
        (fun (item: 'T) ->
            {|
                Label = itemToLabel item
            |}
        )
        |> PickerItemView.Default

    static member Make<'T when 'T : comparison> (itemToVisuals: 'T -> PickerItemVisuals) : PickerItemView<'T> =
        PickerItemView.Default itemToVisuals

type Picker<'T when 'T : comparison>(initialProps) =
    inherit EstatefulComponent<Props<'T>, Estate<'T>, Actions<'T>, Picker<'T>>("LibClient.Components.Input.Picker", initialProps, Actions<'T>, hasStyles = true)

    override _.GetInitialEstate(_) : Estate<'T> =
        {
            PickerId = System.Guid.NewGuid()
        }

and Actions<'T when 'T : comparison>(_this: Picker<'T>) =
    class end

let Make = makeConstructor<Picker<'T>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

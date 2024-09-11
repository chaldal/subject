module LibClient.Components.Input.ChoiceListItem

open LibClient
open Fable.React
open ReactXP.Styles

type Label =
| String of string
| Children

[<RequireQualifiedAccess>]
type IconPosition =
| Left
| Right

type Props<'T when 'T : comparison> = (* GenerateMakeFunction *) {
    Value:        'T
    Label:        Label        // default Children
    IconPosition: IconPosition // default IconPosition.Left

    Group:        LibClient.Components.Input.ChoiceList.Group<'T>
    styles:       array<ViewStyles> option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

type ChoiceListItem<'T when 'T : comparison>(_initialProps) =
    inherit PureStatelessComponent<Props<'T>, Actions<'T>, ChoiceListItem<'T>>("LibClient.Components.Input.ChoiceListItem", _initialProps, Actions, hasStyles = true)

and Actions<'T when 'T : comparison>(_this: ChoiceListItem<'T>) =
    class end

let Make = makeConstructor<ChoiceListItem<'T>, _, _>

// Unfortunately necessary boilerplate
type Estate<'T> = NoEstate1<'T>
type Pstate = NoPstate
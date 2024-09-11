module AppEggShellGallery.Components.Content.ItemList

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Fruit = {
    Name:  string
    Color: Color
}

let fruits = [
    { Name = "Mango";    Color = Color.Hex "#ff9000" }
    { Name = "Kiwi";     Color = Color.Hex "#1d6308" }
    { Name = "Raspbery"; Color = Color.Hex "#b90041" }
    { Name = "Apple";    Color = Color.Hex "#76b902" }
]

type ItemList(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ItemList>("AppEggShellGallery.Components.Content.ItemList", _initialProps, Actions, hasStyles = true)

and Actions(_this: ItemList) =
    class end

let Make = makeConstructor<ItemList, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

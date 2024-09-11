module AppEggShellGallery.Components.Content.Draggable

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Draggable(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Draggable>("AppEggShellGallery.Components.Content.Draggable", _initialProps, Actions, hasStyles = true)

and Actions(_this: Draggable) =
    class end

let Make = makeConstructor<Draggable, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

module AppEggShellGallery.Components.Content.Section.Padded

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Padded(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Padded>("AppEggShellGallery.Components.Content.Section.Padded", _initialProps, Actions, hasStyles = true)

and Actions(_this: Padded) =
    class end

let Make = makeConstructor<Padded, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

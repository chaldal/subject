module AppEggShellGallery.Components.Content.Timestamp

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Timestamp(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Timestamp>("AppEggShellGallery.Components.Content.Timestamp", _initialProps, Actions, hasStyles = false)

and Actions(_this: Timestamp) =
    class end

let Make = makeConstructor<Timestamp, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

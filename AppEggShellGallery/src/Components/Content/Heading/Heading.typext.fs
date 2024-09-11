module AppEggShellGallery.Components.Content.Heading

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Heading(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Heading>("AppEggShellGallery.Components.Content.Heading", _initialProps, Actions, hasStyles = true)

and Actions(_this: Heading) =
    class end

let Make = makeConstructor<Heading, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

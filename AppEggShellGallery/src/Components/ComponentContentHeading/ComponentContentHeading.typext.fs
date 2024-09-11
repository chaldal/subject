module AppEggShellGallery.Components.ComponentContentHeading

open LibClient

type Props = (* GenerateMakeFunction *) {
    DisplayName:  string
    IsResponsive: bool

    key: string option // defaultWithAutoWrap JsUndefined
}

type ComponentContentHeading(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ComponentContentHeading>("AppEggShellGallery.Components.ComponentContentHeading", _initialProps, Actions, hasStyles = true)

and Actions(_this: ComponentContentHeading) =
    class end

let Make = makeConstructor<ComponentContentHeading, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

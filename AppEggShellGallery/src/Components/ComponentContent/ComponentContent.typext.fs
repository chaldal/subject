module AppEggShellGallery.Components.ComponentContent

open LibClient

type Props = (* GenerateMakeFunction *) {
    DisplayName:  string
    IsResponsive: bool                // default false
    Props:        Option<PropsConfig> // defaultWithAutoWrap None
    Samples:      ReactElement
    Notes:        ReactElement        // default noElement
    ThemeSamples: ReactElement        // default noElement
}

and PropsConfig =
| ForFullyQualifiedName of string
| Manual                of ReactElement

type ComponentContent(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ComponentContent>("AppEggShellGallery.Components.ComponentContent", _initialProps, Actions, hasStyles = true)

and Actions(_this: ComponentContent) =
    class end

let Make = makeConstructor<ComponentContent, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

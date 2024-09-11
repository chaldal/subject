module AppEggShellGallery.Components.ComponentSampleGroup

open LibClient

type Props = (* GenerateMakeFunction *) {
    Samples: ReactElement
    Notes:   ReactElement  // default             noElement
    Heading: string option // defaultWithAutoWrap None
    key:     string option // defaultWithAutoWrap JsUndefined
}

type ComponentSampleGroup(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ComponentSampleGroup>("AppEggShellGallery.Components.ComponentSampleGroup", _initialProps, Actions, hasStyles = true)

and Actions(_this: ComponentSampleGroup) =
    class end

let Make = makeConstructor<ComponentSampleGroup, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

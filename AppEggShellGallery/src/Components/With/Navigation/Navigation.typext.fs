module AppEggShellGallery.Components.With.Navigation

open LibClient

// this component is actually completely unnecessary, it's just here
// to prevent a large number of changes caused by removal with With.Navigation
// and using of the nav value directly

type Props = (* GenerateMakeFunction *) {
    With: AppEggShellGallery.Navigation.Navigation -> ReactElement

    key: string option // defaultWithAutoWrap JsUndefined
}

let renderFn (props: Props) : ReactElement =
    props.With AppEggShellGallery.Navigation.nav

let Make : MakeFnComponent<Props> =
    makeFnConstructor "AppEggShellGallery.Components.With.Navigation" renderFn


// Unfortunately necessary boilerplate
type Actions = NoActions
type Estate = NoEstate
type Pstate = NoPstate

module AppEggShellGallery.Components.AppContext

open LibClient

type Props = (* GenerateMakeFunction *) {

    key:      string option // defaultWithAutoWrap JsUndefined
}

type AppContext(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, AppContext>("AppEggShellGallery.Components.AppContext", _initialProps, Actions, hasStyles = false)

and Actions(_this: AppContext) =
    class end

let Make = makeConstructor<AppContext, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

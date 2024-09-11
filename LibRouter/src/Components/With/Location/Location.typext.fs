module LibRouter.Components.With.Location

open LibClient

type Props = (* GenerateMakeFunction *) {
    With: LibRouter.Components.Router.Location -> ReactElement
    key: string option // defaultWithAutoWrap JsUndefined
}

let Make : MakeFnComponent<Props> =
    makeFnComponent "LibRouter.Components.With.Location" (* hasStyles *) false NoFnComponentActions

// Unfortunately necessary boilerplate
type Actions = NoActions
type Estate = NoEstate
type Pstate = NoPstate

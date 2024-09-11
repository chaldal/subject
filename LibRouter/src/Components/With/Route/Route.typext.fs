module LibRouter.Components.With.Route

open LibClient
open LibRouter.RoutesSpec

type Props<'Route, 'ResultlessDialog, 'ResultfulDialog> = (* GenerateMakeFunction *) {
    Spec: LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>
    With: Option<NavigationFrame<'Route, 'ResultlessDialog>> -> ReactElement

    key: string option // defaultWithAutoWrap JsUndefined
}

let decodeLocation<'Route, 'ResultlessDialog, 'ResultfulDialog> (spec: LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>) (location: LibRouter.Components.Router.Location) : Option<NavigationFrame<'Route, 'ResultlessDialog>> =
    spec.FromLocation { Path = location.pathname; Query = location.search }

let Make<'Route, 'ResultlessDialog, 'ResultfulDialog> =
    makeFnComponent "LibRouter.Components.With.Route" (* hasStyles *) false NoFnComponentActions


// Unfortunately necessary boilerplate
type Actions<'Route, 'ResultlessDialog, 'ResultfulDialog> = NoActions
type Estate<'Route, 'ResultlessDialog, 'ResultfulDialog> = NoEstate3<'Route, 'ResultlessDialog, 'ResultfulDialog>
type Pstate = NoPstate

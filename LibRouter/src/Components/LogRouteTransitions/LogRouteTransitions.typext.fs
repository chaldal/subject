module LibRouter.Components.LogRouteTransitions

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

let makeRenderFn () : Props -> ReactElement =
    let mutable maybeLastUrl: Option<string> = None

    fun (_props: Props) ->
        let location = LibRouter.Components.Router.useLocation()
        if maybeLastUrl <> Some location.Url then
            maybeLastUrl <- Some location.Url
            Telemetry.TrackScreenView location.Url Map.empty
        noElement

let Make : MakeFnComponent<Props> =
    makeFnConstructor "LibRouter.Components.LogRouteTransitions" (makeRenderFn ())

// Unfortunately necessary boilerplate
type Actions = NoActions
type Estate = NoEstate
type Pstate = NoPstate

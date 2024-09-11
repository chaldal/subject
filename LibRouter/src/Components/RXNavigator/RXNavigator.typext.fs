module LibRouter.Components.RXNavigator

open Fable.Core.JsInterop
open Fable.React

type NavigatorSceneConfigType =
| FloatFromRight  = 0
| FloatFromLeft   = 1
| FloatFromBottom = 2
| Fade            = 3
| FadeWithSlide   = 4

type CustomNavigatorSceneConfig = {
    foo: int
}

type NavigatorRoute = (* GenerateMakeFunction *) {
    routeId:                 int
    sceneConfigType:         NavigatorSceneConfigType
    gestureResponseDistance: int                        option // defaultWithAutoWrap LibClient.JsInterop.Undefined
    customSceneConfig:       CustomNavigatorSceneConfig option // defaultWithAutoWrap LibClient.JsInterop.Undefined
}

[<AbstractClass>]
type RXNavigator =
    // Returns the current list of routes
    abstract member getCurrentRoutes: unit -> array<NavigatorRoute>

    // Replaces the current list of routes with a new list
    abstract member immediatelyResetRouteStack: ((* nextRouteStack *)array<NavigatorRoute>) -> unit

    // Pops the top route off the stack
    abstract member pop: unit -> unit

    // Pops zero or more routes off the top of the stack until
    // the specified route is top-most
    abstract member popToRoute: ((* route *) NavigatorRoute) -> unit

    // Pops all routes off the stack except for the last
    // remaining item in the stack
    abstract member popToTop: unit -> unit

    // Push a new route onto the stack
    abstract member push: ((* route *) NavigatorRoute) -> unit

    // Replaces the top-most route with a new route
    abstract member replace: ((* route *) NavigatorRoute) -> unit

    // Replaces an existing route (identified by index) with
    // a new route
    abstract member replaceAtIndex: ((* route *) NavigatorRoute * (* index *) int) -> unit

    // Replaces the next-to-top-most route with a new route
    abstract member replacePrevious: ((* route *) NavigatorRoute) -> unit

type Props = (* GenerateMakeFunction *) {
    ref:         (RXNavigator) -> unit
    renderScene: (NavigatorRoute) -> ReactElement
}

let ReactXPNavigationRaw: obj = Fable.Core.JsInterop.import "*" "reactxp-navigation"
let Make = LibClient.ThirdParty.wrapComponent<Props>(ReactXPNavigationRaw?Navigator)

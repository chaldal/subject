namespace LibRouter.Components

open LibClient
open Fable.Core.JsInterop
open Fable.React
open LibRouter.Components.RXNavigator
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module RXNavigatorTypeExtensions =
    type LibRouter.Components.Constructors.LR with
        static member MakeRXNavigatorNavigatorRoute(prouteId: int, psceneConfigType: NavigatorSceneConfigType, ?pchildren: ReactChildrenProp, ?pgestureResponseDistance: int, ?pcustomSceneConfig: CustomNavigatorSceneConfig) =
            {
                routeId = prouteId
                sceneConfigType = psceneConfigType
                gestureResponseDistance = pgestureResponseDistance |> Option.orElse (LibClient.JsInterop.Undefined)
                customSceneConfig = pcustomSceneConfig |> Option.orElse (LibClient.JsInterop.Undefined)
            }
        
        static member RXNavigator(ref: (RXNavigator) -> unit, renderScene: (NavigatorRoute) -> ReactElement, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    ref = ref
                    renderScene = renderScene
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.RXNavigator.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
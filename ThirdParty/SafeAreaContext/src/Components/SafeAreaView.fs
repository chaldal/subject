[<AutoOpen>]
module ThirdParty.SafeAreaContext.Components.SafeAreView

open LibClient
open ReactXP.Styles

#if !EGGSHELL_PLATFORM_IS_WEB
open Fable.Core.JsInterop

let private safeAreaView : obj = import "SafeAreaView" "react-native-safe-area-context"
let private MakeSafeAreaView: obj -> ReactElements -> ReactElement =
    ThirdParty.wrapComponent<obj>(safeAreaView)
#else
open ReactXP.Components
#endif

type SafeAreaContext with
    static member SafeAreaView (
        ?styles:   array<ViewStyles>,
        ?children: ReactElements,
        ?key:      string
    ) =
        let children =
            children
            |> Option.map tellReactArrayKeysAreOkay
            |> Option.getOrElse [||]
            |> ThirdParty.fixPotentiallySingleChild

        #if !EGGSHELL_PLATFORM_IS_WEB
        let __props = createObj [
            "style" ==> styles
            "key"   ==> key
        ]

        MakeSafeAreaView
            __props
            children
        #else
        RX.View (?styles = styles, children = children)
        #endif

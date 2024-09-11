[<AutoOpen>]
module ReactNative.Components.StatusBar

#if !EGGSHELL_PLATFORM_IS_WEB
open LibClient
open Fable.Core.JsInterop
let private reactNativeStatusBar : obj = import "StatusBar" "react-native"
let private MakeReactNativeStatusBar: obj -> ReactElements -> ReactElement =
    ThirdParty.wrapComponent<obj>(reactNativeStatusBar)

[<RequireQualifiedAccess>]
type ReactNativeStatusBarStyle =
| Default
| Light
| Dark

type RN with
    static member StatusBar (
        ?key:      string,
        ?barStyle: ReactNativeStatusBarStyle
    ) =
        let __props = createObj [
            "key" ==> key
        ]

        barStyle
        |> Option.sideEffect (function
            | ReactNativeStatusBarStyle.Light   -> __props?barStyle <- "light-content"
            | ReactNativeStatusBarStyle.Dark    -> __props?barStyle <- "dark-content"
            | ReactNativeStatusBarStyle.Default -> __props?barStyle <- "default"
        )

        MakeReactNativeStatusBar
            __props
            [|  |]
#endif

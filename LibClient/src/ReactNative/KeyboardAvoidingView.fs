[<AutoOpen>]
module ReactNative.Components.KeyboardAvoidingView

open LibClient
open Fable.Core.JsInterop

type KeyboardBehavior =
| Height
| Position
| Padding
    with
        member this.toJS =
            match this with
            | Height -> "height"
            | Position -> "position"
            | Padding -> "padding"

#if !EGGSHELL_PLATFORM_IS_WEB
let ReactNativeKeyboardAvoidingViewRaw: obj = import "KeyboardAvoidingView" "react-native"
let KeyboardRaw: obj = import "Keyboard" "react-native"

let private MakeReactNativeKeyboardAvoidingView: obj -> ReactElements -> ReactElement =
    LibClient.ThirdParty.wrapComponent<obj>(ReactNativeKeyboardAvoidingViewRaw)

type ReactNative.Components.Constructors.RN with
    static member KeyboardAvoidingView(
        ?children: ReactElements,
        ?behavior: KeyboardBehavior,
        ?keyboardVerticalOffset: int,
        ?key:      string
    ) =
        let keyboardBehavior = defaultArg behavior KeyboardBehavior.Height
        let keyboardVerticalOffset = defaultArg keyboardVerticalOffset 0

        let __props = createEmpty
        __props?key      <- key
        __props?behavior <- keyboardBehavior.toJS
        __props?keyboardVerticalOffset <- keyboardVerticalOffset
        __props?style    <- createObj [
            "flex" ==> 1
        ]

        let children =
            children
            |> Option.map tellReactArrayKeysAreOkay
            |> Option.getOrElse [||]
            |> ThirdParty.fixPotentiallySingleChild

        let keyboardAvoidingView =
            MakeReactNativeKeyboardAvoidingView
                __props
                children

        RN.TouchableWithoutFeedback (
            onPress = (fun _ -> KeyboardRaw?dismiss()),
            children = [|keyboardAvoidingView|]
        )
#else
type ReactNative.Components.Constructors.RN with
    static member KeyboardAvoidingView(
        ?children: ReactElements,
        ?behavior: KeyboardBehavior,
        ?keyboardVerticalOffset: int,
        ?key:      string
    ) =
        ignore key
        ignore behavior
        ignore keyboardVerticalOffset
        ThirdParty.fixPotentiallySingleChild (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
#endif
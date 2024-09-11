[<AutoOpen>]
module ReactNative.Components.TouchableWithoutFeedback

open LibClient
open Fable.Core.JsInterop

let ReactNativeTouchableWithoutFeedbackRaw: obj = import "TouchableWithoutFeedback" "react-native"

let private MakeReactNativeTouchableWithoutFeedbackRaw: obj -> ReactElements -> ReactElement =
    ThirdParty.wrapComponent<obj>(ReactNativeTouchableWithoutFeedbackRaw)

type ReactNative.Components.Constructors.RN with
    static member TouchableWithoutFeedback(
        onPress:   unit -> unit,
        ?children: ReactElements,
        ?key:      string
    ) =
        let __props = createEmpty
        __props?key                              <- key
        __props?onPress                          <- onPress

        let children =
            children
            |> Option.map tellReactArrayKeysAreOkay
            |> Option.getOrElse [||]
            |> ThirdParty.fixPotentiallySingleChild

        MakeReactNativeTouchableWithoutFeedbackRaw
            __props
            children
namespace ReactNative

#if !EGGSHELL_PLATFORM_IS_WEB
open Fable.Core.JsInterop

module Vibration =
    let private vibrationRaw: obj = import "Vibration" "react-native"

    let vibrateOnce (durationMs: int) : unit =
        vibrationRaw?vibrate durationMs // duration is fixed on ios
#endif
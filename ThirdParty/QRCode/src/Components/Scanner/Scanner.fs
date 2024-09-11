[<AutoOpen>]
module ThirdParty.QRCode.Components.Scanner

open Fable.Core
open Fable.Core.JsInterop
open LibClient

#if !EGGSHELL_PLATFORM_IS_WEB
let private QRCodeScanner: obj -> ReactElement = importDefault "react-native-qrcode-scanner"

type ThirdParty.QRCode.Components.Constructors.QRCode with
    static member Scanner (onRead: obj -> unit, ?bottomContent: ReactElement) =
        let props = createObj [
            "onRead"        ==> fun e -> onRead (e)
            "bottomContent" ==> bottomContent
        ]
        Fable.React.ReactBindings.React.createElement(QRCodeScanner, props, [])
#else
type ThirdParty.QRCode.Components.Constructors.QRCode with
    static member Scanner (onRead: obj -> unit, ?bottomContent: ReactElement) =
        noElement
#endif
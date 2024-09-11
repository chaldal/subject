[<AutoOpen>]
module ThirdParty.QRCode.Components.Generator

open Fable.Core
open Fable.Core.JsInterop
open LibClient

let private QRCode : obj -> ReactElement = importDefault "react-qr-code"

type ThirdParty.QRCode.Components.Constructors.QRCode with
    static member Generator (value: string, ?size: int, ?bgColor: Color, ?fgColor: Color) =
        let size = defaultArg size 200
        let bgColor = defaultArg bgColor Color.White
        let fgColor = defaultArg fgColor Color.Black

        let props = createObj [
            "value"   ==> value
            "size"    ==> size
            "bgColor" ==> bgColor.ToCssString
            "fgColor" ==> fgColor.ToCssString
        ]
        Fable.React.ReactBindings.React.createElement(QRCode, props, [])
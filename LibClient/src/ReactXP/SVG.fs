module ReactXP.SVG

open Fable.Core.JsInterop

let RXSVG: obj = Fable.Core.JsInterop.import "*" "@chaldal/reactxp-imagesvg"

let ImageSvg: obj -> array<Fable.React.ReactElement> -> Fable.React.ReactElement = LibClient.ThirdParty.wrapComponent<obj>(RXSVG?("default"))
let SvgPath: obj -> array<Fable.React.ReactElement> -> Fable.React.ReactElement = LibClient.ThirdParty.wrapComponent<obj>(RXSVG?("SvgPath"))

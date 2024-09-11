module AppEggShellGallery.Components.ColorVariant.ColorBlock

open Fable.Core.JsInterop

open LibClient


type Props = (* GenerateMakeFunction *) {
    Color:  Color
    IsMain: bool // default false

    key: string option // defaultWithAutoWrap JsUndefined
}

type ColorBlock(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ColorBlock>("AppEggShellGallery.Components.ColorVariant.ColorBlock", _initialProps, Actions, hasStyles = false)

    override this.render() =
        let props = createObj [
            // Convert to inline styles when #172 is implemented
            "style" ==> createObj [
                "display"         ==> "flex"
                "justifyContent"  ==> "center"
                "alignItems"      ==> "center"
                "marginBottom"    ==> 3
                "color"           ==> "white"
                "fontWeight"      ==> "900"
                "width"           ==> 50
                "height"          ==> 50
                if _initialProps.IsMain then "borderRadius" ==> "50%"
                "backgroundColor" ==> _initialProps.Color.ToCssString
            ]
            if _initialProps.IsMain then
                "dangerouslySetInnerHTML" ==> createObj [
                    "__html" ==> "Main"
                ]
        ]

        Fable.React.ReactBindings.React.createElement("div", props, [])

and Actions(_this: ColorBlock) =
    class end

let Make = makeConstructor<ColorBlock, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

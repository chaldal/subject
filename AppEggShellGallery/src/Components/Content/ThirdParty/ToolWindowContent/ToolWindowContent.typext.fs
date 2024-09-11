module AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent

open LibClient
open ThirdParty.Map
    
type Props = (* GenerateMakeFunction *) {
    Handle: InfoWindowHandle
    key: string option // defaultWithAutoWrap JsUndefined
}


type ToolWindowContent(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ToolWindowContent>("AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent", _initialProps, Actions, hasStyles = true)

and Actions(_this: ToolWindowContent) =
    class end

let Make = makeConstructor<ToolWindowContent, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

module AppEggShellGallery.Components.Content.Icon

open LibClient
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Icon(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Icon>("AppEggShellGallery.Components.Content.Icon", _initialProps, Actions, hasStyles = true)

and Actions(_this: Icon) =
    class end

let Make = makeConstructor<Icon, _, _>

type Styles() =
    static member val c = makeTextStyles {
        fontSize 60
        color Color.DevPink
    }

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

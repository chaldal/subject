module AppEggShellGallery.Components.Content.Dialogs

open LibClient
open AppEggShellGallery.LocalImages

let imageSources =
    [1..4]
    |> List.map (fun i -> $"/images/wlop%i{i}.jpg")
    |> List.map localImage

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Dialogs(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Dialogs>("AppEggShellGallery.Components.Content.Dialogs", _initialProps, Actions, hasStyles = true)

and Actions(_this: Dialogs) =
    class end

let Make = makeConstructor<Dialogs, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

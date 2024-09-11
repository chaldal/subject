module AppEggShellGallery.Components.Content.TimeSpan

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type TimeSpan(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, TimeSpan>("AppEggShellGallery.Components.Content.TimeSpan", _initialProps, Actions, hasStyles = true)

and Actions(_this: TimeSpan) =
    class end

let Make = makeConstructor<TimeSpan, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

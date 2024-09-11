module LibClient.Components.ImageCard

open LibClient
open ReactXP.Styles

[<RequireQualifiedAccess>]
type UseScrim =
| Yes
| No

type Label =
| Text     of string * UseScrim
| Children of UseScrim
with
    member this.UseScrim : bool =
        match this with
        | Text     (_, UseScrim.Yes) -> true
        | Children     UseScrim.Yes  -> true
        | _                          -> false

type Style =
| Flat
| Shadowed

type Corners =
| Sharp
| Rounded

type Props = (* GenerateMakeFunction *) {
    Source:      LibClient.Services.ImageService.ImageSource
    Label:       Label                       option // defaultWithAutoWrap None
    Style:       Style                              // default Style.Flat
    Corners:     Corners                            // default Corners.Sharp
    OnPress:     (ReactEvent.Action -> unit) option // defaultWithAutoWrap None
    styles:      array<ViewStyles> option           // defaultWithAutoWrap None
    labelStyles: array<TextStyles> option           // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

let ofUrl = LibClient.Services.ImageService.ImageSource.ofUrl

type ImageCard(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ImageCard>("LibClient.Components.ImageCard", _initialProps, Actions, hasStyles = true)

and Actions(_this: ImageCard) =
    class end

let Make = makeConstructor<ImageCard, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

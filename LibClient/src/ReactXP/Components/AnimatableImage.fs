[<AutoOpen>]
module ReactXP.Components.AnimatableImage

open ReactXP.Helpers
open ReactXP.Styles
open ReactXP.Types

open Fable.Core.JsInterop
open Fable.Core
open Browser.Types

module RX =
    module AnimatableImage =
        [<StringEnum>]
        type ResizeMethod =
        | Auto
        | Resize
        | Scale

        type ResizeMode = ReactXP.Components.Image.ResizeMode
        let Stretch = ReactXP.Components.Image.ResizeMode.Stretch
        let Contain = ReactXP.Components.Image.ResizeMode.Contain
        let Cover   = ReactXP.Components.Image.ResizeMode.Cover
        let Auto    = ReactXP.Components.Image.ResizeMode.Auto
        let Repeat  = ReactXP.Components.Image.ResizeMode.Repeat

        type Dimensions = {
            width:  int
            height: int
        }

open RX.AnimatableImage

type ReactXP.Components.Constructors.RX with
    static member AnimatableImage(
        source:              string,
        ?headers:            Headers,
        ?accessibilityLabel: string,
        ?resizeMode:         ResizeMode,
        ?resizeMethod:       ResizeMethod,
        ?title:              string,
        ?onLoad:             Dimensions -> unit,
        ?onError:            ErrorEvent -> unit,
        ?styles:             array<AnimatableViewStyles>
    ) =
        let __props = createEmpty

        __props?source             <- source
        __props?headers            <- headers
        __props?accessibilityLabel <- accessibilityLabel
        __props?resizeMode         <- resizeMode
        __props?resizeMethod       <- resizeMethod
        __props?title              <- title
        __props?onLoad             <- onLoad
        __props?onError            <- onError
        __props?style              <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?Animated?Image,
            __props,
            [||]
        )


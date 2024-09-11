[<AutoOpen>]
module ReactXP.Components.ActivityIndicator

open ReactXP.Helpers

open Fable.Core.JsInterop
open Fable.Core

[<StringEnum>]
type Size =
| Large
| Medium
| Small
| Tiny

type ReactXP.Components.Constructors.RX with
    static member ActivityIndicator(
        color:          string,
        ?size:          Size,
        ?deferTime:     int,
        ?key:           string,
        ?styles:        array<ReactXP.Styles.FSharpDialect.ViewStyles>,
        ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>
    ) =
        let __props = createEmpty

        __props?color     <- color
        __props?size      <- size
        __props?deferTime <- deferTime
        __props?key       <- key
        __props?style     <- styles

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?ActivityIndicator,
            __props,
            [||]
        )

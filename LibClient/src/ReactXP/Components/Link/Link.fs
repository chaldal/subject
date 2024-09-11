[<AutoOpen>]
module ReactXP.Components.Link

open ReactXP.Helpers

open Fable.Core.JsInterop
open Browser.Types
open LibClient

type ReactXP.Components.Constructors.RX with
    static member Link(
        url:                       string,
        ?children:                 ReactChildrenProp,
        ?title:                    string,
        ?selectable:               bool,
        ?numberOfLines:            float,
        ?allowFontScaling:         bool,
        ?maxContentSizeMultiplier: float,
        ?tabIndex:                 int,
        ?accessibilityId:          string,
        ?autoFocus:                bool,
        ?onPress:                  (Event -> string -> unit),
        ?onLongPress:              (Event -> string -> unit),
        ?onHoverStart:             (Event -> unit),
        ?onHoverEnd:               (Event -> unit),
        ?onContextMenu:            (MouseEvent -> unit),
        ?styles:                   array<ReactXP.Styles.FSharpDialect.ViewStyles>,
        ?xLegacyStyles:            List<ReactXP.LegacyStyles.RuntimeStyles>
    ) =
        let __props = createEmpty

        __props?url                      <- url
        __props?title                    <- title
        __props?selectable               <- selectable
        __props?numberOfLines            <- numberOfLines
        __props?allowFontScaling         <- allowFontScaling
        __props?maxContentSizeMultiplier <- maxContentSizeMultiplier
        __props?tabIndex                 <- tabIndex
        __props?accessibilityId          <- accessibilityId
        __props?autoFocus                <- autoFocus
        __props?onPress                  <- onPress
        __props?onLongPress              <- onLongPress
        __props?onHoverStart             <- onHoverStart
        __props?onHoverEnd               <- onHoverEnd
        __props?onContextMenu            <- onContextMenu
        __props?style                    <- styles

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?Link,
            __props,
            ThirdParty.fixPotentiallySingleChild (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
        )

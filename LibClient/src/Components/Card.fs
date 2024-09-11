[<AutoOpen>]
module LibClient.Components.Card

open Fable.React
open ReactXP.Components
open ReactXP.Styles
open LibClient.Components
open LibClient
open ReactXP.Styles.RulesRestricted

module LC =
    module Card =
        type ShadowedCardProperties = {
            ShadowColor:  Color
            Elevation:    int
            ShadowRadius: int
            ShadowOffset: int * int
        }

        type FlatCardProperties = {
            BorderColor: Color
            BorderWidth: int
        }

        type CardType =
        | Flat of FlatCardProperties
        | Shadowed of ShadowedCardProperties

        type Theme = {
            CardType:     CardType
            BorderRadius: int
            Padding:      int
        }
        with
            static member ShadowedCard =
                {
                    BorderRadius = 2
                    Padding      = 8
                    CardType     = Shadowed {
                        ShadowColor  = Color.BlackAlpha 0.3
                        Elevation    = 10
                        ShadowRadius = 4
                        ShadowOffset = (0, 1)
                    }
                }

            static member FlatCard =
                {
                    BorderRadius = 6
                    Padding      = 8
                    CardType     = Flat {
                        BorderColor = MaterialDesignColors.grey.B300
                        BorderWidth = 1
                    }
                }

open LC.Card

[<RequireQualifiedAccess>]
module private Styles =
    let outerContainerDefaults = makeViewStyles {
        backgroundColor Color.White
        margin 8
    }

    let outerContainer = ViewStyles.Memoize (
        fun (theme: Theme) -> makeViewStyles {
            padding 0
            overflow Overflow.Visible
            borderRadius theme.BorderRadius

            match theme.CardType with
            | Flat flatTheme ->
                border flatTheme.BorderWidth flatTheme.BorderColor
            | Shadowed shadowedTheme ->
                shadow
                    shadowedTheme.ShadowColor
                    shadowedTheme.ShadowRadius
                    shadowedTheme.ShadowOffset
                elevation shadowedTheme.Elevation
        }
    )

    let contentContainer = ViewStyles.Memoize (
        fun (theme: Theme) -> makeViewStyles {
            borderRadius theme.BorderRadius
            overflow Overflow.Hidden
            padding theme.Padding
        }
    )

    let outerContainerWithOnPress = ViewStyles.Memoize (
        fun (isHovered, isActive) -> makeViewStyles {
            if isHovered then
                backgroundColor MaterialDesignColors.grey.B050

            if isActive then
                opacity 0.4
                backgroundColor MaterialDesignColors.grey.B100
        }
    )

type LC with
    [<Component>]
    (*
        A layered card is required because iOS shadows get clipped when overflow is hidden.
        Having a layered card with proper overflow built in provides a bug free shadow.
        Control over the outer container with the shadow is limited to avoid accidentaly issues with shadow.
        Card content layout can be controlled granularly using a wrapper around the content.
    *)
    static member Card (
        children:     array<ReactElement>,
        ?onPress:     (ReactEvent.Action -> unit),
        ?theme:       Theme -> Theme,
        ?outerStyles: array<ViewStyles>,
        ?key:         string
    ) : ReactElement =
        ignore key
        // noElement
        let theTheme    = Themes.GetMaybeUpdatedWith theme
        let outerStyles = defaultArg outerStyles Array.empty

        match onPress with
        | None ->
            RX.View (
                styles = [|
                    Styles.outerContainerDefaults
                    yield! outerStyles
                    Styles.outerContainer theTheme
                |],
                children = [|
                    RX.View (
                        children = children,
                        styles   = [| Styles.contentContainer theTheme |]
                    )
                |]
            )
        | Some onPress ->
            LC.Pointer.State (fun pointerState ->
                RX.View (
                    styles = [|
                        Styles.outerContainerDefaults
                        yield! outerStyles
                        Styles.outerContainer theTheme
                        Styles.outerContainerWithOnPress (pointerState.IsHovered, pointerState.IsDepressed)
                    |],
                    children = [|
                        RX.View (
                            children = children,
                            styles   = [| Styles.contentContainer theTheme |]
                        )
                        LC.TapCapture (
                            onPress      = onPress,
                            pointerState = pointerState
                        )
                    |]
                )
            )

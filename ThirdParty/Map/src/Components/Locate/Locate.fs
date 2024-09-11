[<AutoOpen>]
module ThirdParty.Map.Components.Locate

open Fable.React
open ReactXP.Components
open ReactXP.Styles
open LibClient.Components
open LibClient.LocalImages

module private Styles =
    let locateButtonContainer = makeViewStyles {
        Position.Absolute
        height 50
        width 50
        Overflow.VisibleForDropShadow
        #if EGGSHELL_PLATFORM_IS_WEB
        bottom 90
        #else
        bottom 55
        #endif
        right 10

        AlignItems.Center
        JustifyContent.Center
    }

    let locateImage = makeViewStyles {
        size 25 25
    }

    let locateImageContainer = makeViewStyles {
        minWidth 30
        minHeight 30
        heightPercent 100
        widthPercent 100
        AlignItems.Center
        JustifyContent.Center
    }

    let locateButtonCardTheme _ = { LC.Card.Theme.ShadowedCard with BorderRadius = 30 }

type Map.Locate with
    [<Component>]
    static member LocateButtonWrapper (children: ReactElement[]) =
        RX.View (styles = [| Styles.locateButtonContainer |], children = children)

    [<Component>]
    static member LocateButton (onPress: LibClient.Input.ReactEvent.Action -> unit) =
        LC.Card ([|
            RX.View (styles = [| Styles.locateImageContainer |], children = [|
                RX.Image (
                    styles = [|Styles.locateImage|],
                    size   = Image.Size.FromStyles,
                    source = localImage "/libs/ThirdParty/Map/images/locate_icon.png"
                )
            |])
        |], theme = Styles.locateButtonCardTheme, onPress = onPress)
[<AutoOpen>]
module LibClient.Components.Stencil

open Fable.React

open LibClient
open LibClient.LocalImages

open ReactXP.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
type Animation =
// | Wave
| Pulsate
| NoAnimation

[<RequireQualifiedAccess>]
type Variant =
| Circle           of Diameter: int
| Rectangle        of Width: int  * Height: int
| RoundedRectangle of Width: int  * Height: int
| Raw              of Width: int  * Height: int * BorderRadius: int
| FromStyles

[<RequireQualifiedAccess>]
module private Styles =
    let stencil =
        ViewStyles.Memoize(
            fun (variant: Variant) (animation: Animation) ->
                makeViewStyles {
                    opacity         0.2
                    backgroundColor (Color.Grey "cc")
                    Overflow.Hidden

                    match variant with
                    | Variant.Raw(theWidth, theHeight, theBorderRadius) ->
                        width        theWidth
                        height       theHeight
                        borderRadius theBorderRadius
                    | Variant.Circle diameter ->
                        width        diameter
                        height       diameter
                        borderRadius (diameter / 2)
                    | Variant.Rectangle(theWidth, theHeight) ->
                        width        theWidth
                        height       theHeight
                        borderRadius 0
                    | Variant.RoundedRectangle(theWidth, theHeight) ->
                        width        theWidth
                        height       theHeight
                        borderRadius (int (float (min theHeight theWidth) * 0.33))
                    | Variant.FromStyles ->
                        ()

                    match animation with
                    | Animation.Pulsate ->
                        opacity 0.2
                    | Animation.NoAnimation ->
                        ()
                }
        )

    let stencilImage =
        makeViewStyles {
            flex 1
        }


type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Stencil(
        variant:    Variant,
        ?animation: Animation,
        ?styles:    array<ViewStyles>
    ) : ReactElement =
        let styles = defaultArg styles Array.empty
        let animation = defaultArg animation Animation.Pulsate

        match animation with
        | Animation.Pulsate ->
            RX.View (
                styles = [|
                    yield Styles.stencil variant animation
                    yield! styles
                |],
                children = [|
                    RX.Image (
                        styles = [|Styles.stencilImage|],
                        size   = Image.Size.FromStyles,
                        source = localImage "/libs/LibClient/images/stencil.gif",
                        resizeMode = ResizeMode.Cover
                    )
                |]
            )
        | Animation.NoAnimation ->
            RX.View (
                styles = [|
                    yield Styles.stencil variant animation
                    yield! styles
                |],
                children = [|
                    nothing
                |]
            )

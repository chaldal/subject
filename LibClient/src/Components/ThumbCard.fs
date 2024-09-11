[<AutoOpen>]
module LibClient.Components.ThumbCard

open Fable.React
open LibClient.Components.Layout.LC
open ReactXP.Styles
open ReactXP.Components
open LibClient.Components
open LibClient

module LC =
    module ThumbCard =
        [<RequireQualifiedAccess>]
        type ThumbPosition =
        | Right
        | Left

open LC.ThumbCard

module private Styles =
    let row = makeViewStyles {
        AlignSelf.Stretch
    }

    let thumb = ViewStyles.Memoize (fun thumbPosition -> makeViewStyles {
        AlignSelf.Stretch
        width 112
        minHeight 112
        match thumbPosition with
        | ThumbPosition.Left  -> marginRight 12
        | ThumbPosition.Right -> marginLeft  12
    })

    let image = makeViewStyles {
        Position.Absolute
        trbl 0 0 0 0
    }

type private Helpers =
    [<Component>]
    static member Thumb (
        imageSource:   LibClient.Services.ImageService.ImageSource,
        thumbPosition: ThumbPosition,
        ?onPress:      ReactEvent.Action -> unit
    ) : ReactElement =
        LC.With.Layout(
            initialOnly = true,
            ``with`` =
                fun (onLayoutOption, maybeLayout) ->
                    RX.View(
                        styles = [|Styles.thumb thumbPosition|],
                        ?onLayout = onLayoutOption,
                        children =
                            elements {
                                RX.Image(
                                    styles = [| Styles.image |],
                                    source = imageSource,
                                    resizeMode = ReactXP.Components.Image.ResizeMode.Cover,
                                    size = (Size.FromParentLayout maybeLayout)
                                )

                                match onPress with
                                | Some onPress ->
                                    LC.TapCapture(
                                        onPress = onPress
                                    )
                                | None ->
                                    noElement
                            }
                    )
        )

    static member CustomCardTheme (theme: LC.Card.Theme) : LC.Card.Theme =
        { theme with Padding = 0; BorderRadius = 12 }

type LC with
    [<Component>]
    static member ThumbCard (
        imageSource:    LibClient.Services.ImageService.ImageSource,
        child:          ReactElement,
        ?thumbPosition: ThumbPosition,
        ?onPress:       ReactEvent.Action -> unit,
        ?styles:        array<ViewStyles>
    ) : ReactElement =
        let thumbPosition = defaultArg thumbPosition ThumbPosition.Left
        LC.Card (
            theme    = Helpers.CustomCardTheme,
            children = [|
                LC.Row (
                    crossAxisAlignment = CrossAxisAlignment.Stretch,
                    styles = [|Styles.row|],
                    children =
                        match thumbPosition with
                        | ThumbPosition.Left ->
                            [|
                                Helpers.Thumb (imageSource, thumbPosition)
                                child
                            |]
                        | ThumbPosition.Right ->
                            [|
                                child
                                Helpers.Thumb (imageSource, thumbPosition)
                            |]
                )
            |],
            ?outerStyles = styles,
            ?onPress     = onPress
        )


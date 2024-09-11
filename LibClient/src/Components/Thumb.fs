[<AutoOpen>]
module LibClient.Components.Thumb

open Fable.React

open LibClient
open LibClient.Services.ImageService

open ReactXP.Components
open ReactXP.Styles

module LC =
    module Thumb =
        type For<'T> =
        | Value of 'T * ('T -> ImageSource)
        with
            member this.Source : ImageSource =
                match this with
                | Value (value, toSource) -> toSource value

        type For =
            static member Of(source: ImageSource) : For<ImageSource> = Value (source, identity)

            static member Of(value: 'T, toSource: 'T -> ImageSource) : For<'T> = Value (value, toSource)

        let ofUrl = LibClient.Services.ImageService.ImageSource.ofUrl

        type Corners =
        | Sharp
        | Rounded

        module private Theme =
            let thumb = ViewStyles.Memoize (fun size -> makeViewStyles {
                flex 0
                borderWidth 2
                borderColor Color.Transparent
                height size
                width size
            })

            let selected = ViewStyles.Memoize (fun color -> makeViewStyles {
                borderColor color
            })

        type Theme = {
            Size: int
            SelectedBorderColor: Color
        }
        with
            member this.Thumb =
                Theme.thumb this.Size

            member this.Selected =
                Theme.selected this.SelectedBorderColor

open LC.Thumb

// TODO: delete after RenderDSL migration
type PropForFactory =
    static member Make (source: ImageSource) : For<ImageSource> = For.Of(source, identity)
    static member Make (value: 'T, toSource: 'T -> ImageSource) : For<'T> = For.Of(value, toSource)

[<RequireQualifiedAccess>]
module private Styles =
    let image =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
        }

    let corners = ViewStyles.Memoize (fun corners -> makeViewStyles {
        if corners = Corners.Rounded then
            borderRadius 12
    })

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Thumb<'T when 'T: comparison>(
            source:      ImageSource,
            ?isSelected: bool,
            ?onPress:    ReactEvent.Action -> unit,
            ?styles:     array<ViewStyles>,
            ?theme:      Theme -> Theme,
            ?corners:    Corners,
            ?key:        string
        ) : ReactElement =
        ignore key

        LibClient.Components.Constructors.LC.Thumb(
            ``for``     = For.Of source,
            ?isSelected = isSelected,
            ?onPress    = onPress,
            ?styles     = styles,
            ?theme      = theme,
            ?corners    = corners,
            ?key        = key
        )

    [<Component>]
    static member Thumb<'T when 'T: comparison>(
            ``for``:     For<'T>,
            ?isSelected: bool,
            ?onPress:    ReactEvent.Action -> unit,
            ?styles:     array<ViewStyles>,
            ?theme:      Theme -> Theme,
            ?corners:    Corners,
            ?key:        string
        ) : ReactElement =
        ignore key

        let theTheme = Themes.GetMaybeUpdatedWith theme
        let isSelected = defaultArg isSelected false
        let corners    = defaultArg corners    Corners.Sharp

        LC.With.Layout(
            initialOnly = true,
            ``with`` =
                fun (onLayoutOption, maybeLayout) ->
                    RX.View (
                        styles =
                            [|
                                theTheme.Thumb

                                if isSelected then
                                    theTheme.Selected

                                Styles.corners corners
                                yield! (styles |> Option.defaultValue [||])
                            |],
                        ?onLayout = onLayoutOption,
                        children =
                            elements {
                                RX.Image (
                                    styles     = [| Styles.image |],
                                    source     = ``for``.Source,
                                    resizeMode = ReactXP.Components.Image.ResizeMode.Cover,
                                    size       = Size.FromParentLayout maybeLayout
                                )

                                onPress |> Option.map LC.TapCapture
                            }
                    )
        )
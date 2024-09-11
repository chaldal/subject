[<AutoOpen>]
module LibClient.Components.Dialog_ImageViewer

open Fable.React

open LibClient
open LibClient.Components
open LibClient.Components.Dialog
open LibClient.Dialogs
open LibClient.Icons
open LibClient.Responsive
open LibClient.Services.ImageService

open ReactXP.Components
open ReactXP.Styles

module LC =
    module Dialog =
        module ImageViewer =
            type Theme = {
                DotColor: Color
                SelectedDotColor: Color
                NavigationButtonColor: Color
                NavigationButtonBackgroundColor: Color
                ButtonIconSize: int
            }

            type internal Parameters = {
                Sources:      seq<ImageSource>
                InitialIndex: uint32
                ResizeMode:   ResizeMode
            }

            type internal DialogProps = DialogProps<Parameters, unit>

open LC.Dialog.ImageViewer

[<RequireQualifiedAccess>]
module private Styles =
    let private theHeight (screenSize: ScreenSize) : int =
        match screenSize with
        | ScreenSize.Handheld -> 300
        | ScreenSize.Desktop  -> 600

    let carousel =
        ViewStyles.Memoize(
            fun (screenSize: ScreenSize) ->
                makeViewStyles {
                    AlignSelf.Stretch
                    height (theHeight screenSize)

                    if screenSize = ScreenSize.Desktop then
                        width 800
                }
        )

    let image =
        ViewStyles.Memoize(
            fun (screenSize: ScreenSize) ->
                makeViewStyles {
                    flex 1
                    height (theHeight screenSize)

                    if screenSize = ScreenSize.Desktop then
                        width 800
                }
        )

    let closeButton =
        makeViewStyles {
            Position.Absolute
            top  16
            left 16
            backgroundColor (Color.BlackAlpha 0.7)
        }

    let closeButtonTheme (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = Color.White
                }
        }

type private Helpers =
    [<Component>]
    static member Carousel(
            sources: NonemptyList<ImageSource>,
            initialIndex: uint32,
            resizeMode: ResizeMode,
            screenSize: ScreenSize,
            theTheme: Theme,
            ?size: Image.Size
        ) : ReactElement =
        let sourcesList = sources.ToList
        let size = defaultArg size Image.Size.FromStyles

        LC.Carousel(
            styles = [| Styles.carousel screenSize |],
            theme =
                (fun theme ->
                    { theme with
                        DotColor = theTheme.DotColor
                        SelectedDotColor = theTheme.SelectedDotColor
                        NavigationButtonColor = theTheme.NavigationButtonColor
                        NavigationButtonBackgroundColor = theTheme.NavigationButtonBackgroundColor
                        ButtonIconSize = theTheme.ButtonIconSize
                    }
                ),
            count = (sourcesList.Length |> PositiveInteger.ofIntUnsafe),
            initialIndex = initialIndex,
            requestFocusOnMount = true,
            slide =
                fun index ->
                    RX.Image(
                        styles = [| Styles.image screenSize |],
                        source = sourcesList[index],
                        resizeMode = resizeMode,
                        size = size
                    )
        )

    [<Component>]
    static member ImageViewer(
            sources: NonemptyList<ImageSource>,
            initialIndex: uint32,
            resizeMode: ResizeMode,
            tryCancel: ReactEvent.Action -> unit,
            ?theme: Theme -> Theme,
            ?size: Image.Size
        ) : ReactElement =
        let theTheme = Themes.GetMaybeUpdatedWith theme

        LC.With.ScreenSize(
            fun screenSize ->
                LC.Responsive(
                    desktop =
                        (fun _ ->
                            LC.Dialog.Shell.WhiteRounded.Base(
                                canClose =
                                    Shell.WhiteRounded.Base.CanClose.When
                                        (
                                            [
                                                Base.CloseAction.OnEscape
                                                Base.CloseAction.OnBackground
                                                Base.CloseAction.OnCloseButton
                                            ],
                                            tryCancel
                                        ),
                                children =
                                    elements {
                                        Helpers.Carousel(
                                            sources,
                                            initialIndex,
                                            resizeMode,
                                            screenSize,
                                            theTheme,
                                            ?size = size
                                        )
                                    }
                            )
                        ),
                    handheld =
                        (fun _ ->
                            LC.Dialog.Base(
                                contentPosition = Base.ContentPosition.Center,
                                canClose =
                                    Shell.WhiteRounded.Base.CanClose.When
                                        (
                                            [
                                                Base.CloseAction.OnEscape
                                                Base.CloseAction.OnBackground
                                            ],
                                            tryCancel
                                        ),
                                children =
                                    elements {
                                        Helpers.Carousel(
                                            sources,
                                            initialIndex,
                                            resizeMode,
                                            screenSize,
                                            theTheme,
                                            ?size = size
                                        )
                                        LC.IconButton(
                                            styles = [| Styles.closeButton |],
                                            theme = Styles.closeButtonTheme,
                                            icon = Icon.X,
                                            state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable tryCancel)
                                        )
                                    }
                            )
                        )
                )
        )

type LibClient.Components.Constructors.LC.Dialog with
    static member OpenImageViewer(
            sources: seq<ImageSource>,
            close: DialogCloseMethod -> ReactEvent.Action -> unit,
            ?initialIndex: uint32,
            ?resizeMode: ResizeMode,
            ?theme: Theme -> Theme,
            ?size: Image.Size
        ) : ReactElement =
        let initialIndex = defaultArg initialIndex 0u
        let resizeMode = defaultArg resizeMode ResizeMode.Cover

        doOpen
            "ImageViewer"
            {
                Sources = sources
                InitialIndex = initialIndex
                ResizeMode = resizeMode
            }
            (fun dialogProps _ ->
                let canCancel () = Async.Of true

                let tryCancel (e: ReactEvent.Action) =
                    tryCancel dialogProps canCancel DialogCloseMethod.HistoryBack e

                match sources |> NonemptyList.ofSeq with
                | Some nonemptySources ->
                    Helpers.ImageViewer(
                        sources = nonemptySources,
                        initialIndex = dialogProps.Parameters.InitialIndex,
                        resizeMode = dialogProps.Parameters.ResizeMode,
                        tryCancel = tryCancel,
                        ?theme = theme,
                        ?size = size
                    )
                | None ->
                    noElement
            )
            {
                OnResult = ignore
                MaybeOnCancel = None
            }
            close

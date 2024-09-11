[<AutoOpen>]
module LibClient.Components.Carousel

open System
open Fable.React

open LibClient
open LibClient.Icons

open ReactXP.Components
open ReactXP.Styles
open ReactXP.Styles.Animation

module LC =
    module Carousel =
        type Theme = {
            DotColor:                        Color
            SelectedDotColor:                Color
            NavigationButtonColor:           Color
            NavigationButtonBackgroundColor: Color
            ButtonIconSize:                  int
            NavigationButtonStyle:           Option<ViewStyles>
            DotContainerStyle:               Option<ViewStyles>
            InactiveDotStyle:                Option<ViewStyles>
            ActiveDotStyle:                  Option<ViewStyles>
        }

open LC.Carousel

[<RequireQualifiedAccess>]
module private Styles =
    let gestureView =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
        }

    let view =
        makeViewStyles {
            Position.Relative
        }

    let animatableView (translateX: AnimatableValue) =
        makeAnimatableViewStyles {
            Position.Absolute
            trbl 0 0 0 0
            animatedTransform [
                [ animatedTranslateX translateX ]
            ]
        }

    let iconButton =
        ViewStyles.Memoize(
            fun (theme: Theme) ->
                makeViewStyles {
                    borderRadius    20
                    width           40
                    height          40
                    backgroundColor theme.NavigationButtonBackgroundColor
                    opacity         0.7
                    Overflow.Hidden
                }
        )

    let iconButtonTheme (theTheme: Theme) (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconColor = theTheme.NavigationButtonColor
                    IconSize = theTheme.ButtonIconSize
                }
        }

    let side =
        makeViewStyles {
            Position.Absolute
            top 0
            bottom 0
            flex  0
            JustifyContent.Center
        }

    let zeroLeftOffset =
        makeViewStyles {
            left 0
        }

    let zeroRightOffset =
        makeViewStyles {
            right 0
        }

    let dotsContainer =
        makeViewStyles {
            Position.Absolute
            height 14
            bottom 3
            left   0
            right  0
            AlignItems.Center
        }

    let dots =
        makeViewStyles {
            FlexDirection.Row
        }

    let dot =
        ViewStyles.Memoize(
            fun (theme: Theme) (isCurrent: bool) ->
                makeViewStyles {
                    margin       2
                    size         8 8
                    borderRadius 4
                    shadow       (Color.BlackAlpha 0.5) 5 (0, 1)

                    if isCurrent then
                        backgroundColor theme.SelectedDotColor
                    else
                        backgroundColor theme.DotColor
                }
        )

let private slideDuration = TimeSpan.FromMilliseconds(200)

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Carousel(
            count: PositiveInteger,
            slide: int -> ReactElement,
            ?initialIndex: uint32,
            ?tabIndex: int,
            ?continuousLoop: bool,
            ?autoScrollAtInterval: TimeSpan,
            ?requestFocusOnMount: bool,
            ?styles: array<ViewStyles>,
            ?theme: Theme -> Theme,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let initialIndex        = defaultArg initialIndex 0u
        let tabIndex            = defaultArg tabIndex -1
        let continuousLoop      = defaultArg continuousLoop false
        let requestFocusOnMount = defaultArg requestFocusOnMount false
        let theTheme            = Themes.GetMaybeUpdatedWith theme

        let index                          = int initialIndex
        let currentMaybeTargetIndexesState = Hooks.useState (index, None)
        let currentPageX                   = Hooks.useState 0
        let panThreshold                   = 20

        let maybeWidthRef            = Hooks.useRef None
        let maybeCurrentAnimationRef = Hooks.useRef None

        let currentImageTranslateX =
            Hooks.useMemo(
                (fun () -> AnimatedValue.Create(0.0)),
                [||]
            )
        let targetImageTranslateX =
            Hooks.useMemo(
                (fun () -> AnimatedValue.Create(0.0)),
                [||]
            )

        Hooks.useEffect(
            (fun () ->
                match maybeWidthRef.current, maybeCurrentAnimationRef.current, currentMaybeTargetIndexesState.current with
                | Some width, None, (currentIndex, Some targetIndex) ->
                    let animation =
                        if targetIndex > currentIndex then
                            // Sliding from right to left
                            0. |> currentImageTranslateX.SetValue
                            width |> double |> targetImageTranslateX.SetValue

                            Animation.Parallel(
                                Animation.Timing(
                                    currentImageTranslateX,
                                    toValue = -width,
                                    duration = slideDuration
                                ),
                                Animation.Timing(
                                    targetImageTranslateX,
                                    toValue = 0.,
                                    duration = slideDuration
                                )
                            )
                        else
                            // Sliding from left to right
                            0. |> currentImageTranslateX.SetValue
                            -width |> double |> targetImageTranslateX.SetValue

                            Animation.Parallel(
                                Animation.Timing(
                                    currentImageTranslateX,
                                    toValue = width,
                                    duration = slideDuration
                                ),
                                Animation.Timing(
                                    targetImageTranslateX,
                                    toValue = 0.,
                                    duration = slideDuration
                                )
                            )

                    maybeCurrentAnimationRef.current <- Some animation

                    animation.Start(
                        fun () ->
                            maybeCurrentAnimationRef.current <- None
                            currentMaybeTargetIndexesState.update ((targetIndex, None))
                    )
                | _ ->
                    Noop
            ),
            [| maybeWidthRef; currentMaybeTargetIndexesState |]
        )

        let goToIndex (index: int): unit =
            let currentIndex, _ = currentMaybeTargetIndexesState.current
            (currentIndex, Some index) |> currentMaybeTargetIndexesState.update

        let showNext (): unit =
            let currentIndex, _ = currentMaybeTargetIndexesState.current
            let nextIndex =
                if continuousLoop then
                    (currentIndex + 1) % count.Value
                else
                    currentIndex + 1
            if nextIndex < count.Value then
                goToIndex nextIndex

        let showPrevious (): unit =
            let currentIndex, _ = currentMaybeTargetIndexesState.current
            let previousIndex =
                if continuousLoop then
                    (currentIndex - 1 + count.Value) % count.Value
                else
                    currentIndex - 1
            if previousIndex >= 0 then
                goToIndex previousIndex

        let onKeyPress (e: Browser.Types.KeyboardEvent): unit =
            match e.key with
            | KeyboardEvent.Key.ArrowLeft  ->
                e.stopPropagation()
                showPrevious()
            | KeyboardEvent.Key.ArrowRight ->
                e.stopPropagation()
                showNext()
            | _ ->
                Noop

        let onPanHorizontal (rawGestureState: ReactXP.Components.GestureView.PanGestureState) : unit =
            if rawGestureState.isComplete && rawGestureState.isTouch then
                let initialX = int rawGestureState.initialPageX
                let distance = currentPageX.current - initialX

                if abs distance > panThreshold then
                    if distance > 0 then
                        showPrevious ()
                    else
                        showNext ()

                    currentPageX.update 0
            else
                currentPageX.update (int rawGestureState.pageX)

        match autoScrollAtInterval with
        | Some interval ->
            Hooks.useEffectDisposable (
                (fun () ->
                    let disposableScrolltimer =
                        JsInterop.runEvery
                            interval
                            (fun () ->
                                let currentIndex, _ = currentMaybeTargetIndexesState.current
                                let nextIndex       = (currentIndex + 1) % count.Value

                                goToIndex nextIndex
                            )

                    { new IDisposable with
                        member _.Dispose () : unit =
                            disposableScrolltimer.Dispose ()
                    }
                ),
                [|currentMaybeTargetIndexesState|]
            )
        | None -> Noop

        let currentIndex, maybeTargetIndex = currentMaybeTargetIndexesState.current

        LC.With.Layout(
            fun (onLayoutOption, maybeLayout) ->
                maybeLayout
                |> Option.map (fun l -> double l.Width)
                |> (fun maybeWidth -> maybeWidthRef.current <- maybeWidth)

                RX.View(
                    styles =
                        [|
                            Styles.view
                            yield! (styles |> Option.defaultValue [||])
                        |],
                    tabIndex = tabIndex,
                    autoFocus = requestFocusOnMount,
                    onKeyPress = onKeyPress,
                    ?onLayout = onLayoutOption,
                    children =
                        elements {
                            RX.GestureView(
                                onPanHorizontal = onPanHorizontal,
                                styles          = [| Styles.gestureView |],
                                children        = [|
                                    RX.AnimatableView(
                                        styles = [| currentImageTranslateX |> AnimatableValue.Value |> Styles.animatableView |],
                                        key = $"current_slide",
                                        children =
                                            elements {
                                                currentIndex |> int |> slide
                                            }
                                    )

                                    match maybeTargetIndex with
                                    | Some targetIndex ->
                                        RX.AnimatableView(
                                            styles = [| targetImageTranslateX |> AnimatableValue.Value |> Styles.animatableView |],
                                            key = $"target_slide",
                                            children =
                                                elements {
                                                    targetIndex |> int |> slide
                                                }
                                        )
                                    | None ->
                                        // We've finished animating to the target, so ensure the
                                        // translation of the current image is zero. Doing this
                                        // in the end animation handler is not sufficient because
                                        // there are occasionally frames rendered with the old
                                        // value.
                                        0. |> currentImageTranslateX.SetValue
                                        noElement

                                    if count.Value > 1 then
                                        let maybePreviousIndex =
                                            if continuousLoop then
                                                (currentIndex - 1 + count.Value) % count.Value |> Some
                                            else if currentIndex > 0 then
                                                currentIndex - 1 |> Some
                                            else
                                                None
                                        let maybeNextIndex =
                                            if continuousLoop then
                                                (currentIndex + 1) % count.Value |> Some
                                            else if currentIndex < (count.Value - 1) then
                                                currentIndex + 1 |> Some
                                            else
                                                None

                                        let iconStyles = [|
                                            Styles.iconButton theTheme
                                            yield!
                                                theTheme.NavigationButtonStyle
                                                |> Option.map(fun style -> [|style|])
                                                |> Option.defaultValue [||]
                                        |]

                                        RX.View(
                                            styles =
                                                [|
                                                    Styles.side
                                                    Styles.zeroLeftOffset
                                                |],
                                            children =
                                                elements {
                                                    match maybePreviousIndex with
                                                    | Some previousIndex ->
                                                        LC.IconButton(
                                                            styles = iconStyles,
                                                            theme  = Styles.iconButtonTheme theTheme,
                                                            icon   = Icon.ChevronLeft,
                                                            state  =
                                                                ButtonHighLevelState.LowLevel (
                                                                    ButtonLowLevelState.Actionable (
                                                                        fun _ -> goToIndex previousIndex
                                                                    )
                                                                )
                                                        )
                                                    | None ->
                                                        noElement
                                                }
                                        )

                                        RX.View(
                                            styles = [| Styles.dotsContainer |],
                                            children =
                                                elements {
                                                    RX.View(
                                                        styles = [|
                                                            Styles.dots
                                                            yield!
                                                                theTheme.DotContainerStyle
                                                                |> Option.map(fun style -> [|style|])
                                                                |> Option.defaultValue [||]
                                                        |],
                                                        children =
                                                            (
                                                                [| 0 .. count.Value - 1 |]
                                                                |> Array.map (fun i ->
                                                                    let isCurrent = i = currentIndex

                                                                    let maybeDotThemeStyle =
                                                                        if isCurrent then
                                                                            theTheme.ActiveDotStyle
                                                                        else
                                                                            theTheme.InactiveDotStyle

                                                                    RX.View(
                                                                        styles = [|
                                                                            Styles.dot theTheme isCurrent
                                                                            yield!
                                                                                maybeDotThemeStyle
                                                                                |> Option.map(fun style -> [|style|])
                                                                                |> Option.defaultValue [||]
                                                                        |]
                                                                    )
                                                                )
                                                            )
                                                    )
                                                }
                                        )

                                        RX.View(
                                            styles =
                                                [|
                                                    Styles.side
                                                    Styles.zeroRightOffset
                                                |],
                                            children =
                                                elements {
                                                    match maybeNextIndex with
                                                    | Some nextIndex ->
                                                        LC.IconButton(
                                                            styles = iconStyles,
                                                            theme  = Styles.iconButtonTheme theTheme,
                                                            icon   = Icon.ChevronRight,
                                                            state  =
                                                                ButtonHighLevelState.LowLevel (
                                                                    ButtonLowLevelState.Actionable (
                                                                        fun _ -> goToIndex nextIndex
                                                                    )
                                                                )
                                                        )
                                                    | None ->
                                                        noElement
                                                }
                                        )
                                |]
                            )
                        }
                )
        )
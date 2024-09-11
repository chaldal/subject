[<AutoOpen>]
module LibClient.Components.Scrim

open System

open Fable.React

open LibClient

open ReactXP.Styles
open ReactXP.Styles.Animation
open ReactXP.Components

[<RequireQualifiedAccess>]
module private Styles =
    let gestureView =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
        }

    let scrimInner (value: AnimatableValue) =
        makeAnimatableViewStyles {
            Position.Absolute
            top 0
            right 0
            bottom 0
            left 0
            backgroundColor (Color.Rgba(0, 0, 0, 0.5))
            animatedOpacity value
        }

[<RequireQualifiedAccess>]
module private Helpers =
    let animate
            (maybeOngoingAnimationHook: IRefValue<Option<Animation>>)
            (opacityAnimatedValue: AnimatedValue)
            (toOpacity: float)
            (onComplete: unit -> unit)
            : unit =
        maybeOngoingAnimationHook.current |> Option.sideEffect (fun ongoingAnimation ->
            // it's important to set to None before calling stop() — there are instances
            // where a hide/show in quick succession lets both animations succeed when
            // stop is called before maybeOngoingAnimation is reset (see completon
            // callback implementation below).
            maybeOngoingAnimationHook.current <- None
            ongoingAnimation.Stop()
        )

        let animation = Animation.Timing(opacityAnimatedValue, toOpacity, TimeSpan.FromMilliseconds(500))
        maybeOngoingAnimationHook.current <- Some animation
        animation.Start (fun () ->
            if maybeOngoingAnimationHook.current = Some animation then
                maybeOngoingAnimationHook.current <- None
                onComplete ()
        )

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Scrim(
            isVisible: bool,
            ?onPress: ReactEvent.Action -> unit,
            ?onPanVertical: ReactXP.Components.GestureView.PanGestureState -> unit,
            ?onPanHorizontal: ReactXP.Components.GestureView.PanGestureState -> unit,
            ?styles: array<ViewStyles>,
            ?key: string) : ReactElement =
        key |> ignore

        let isMountedHook = Hooks.useState isVisible
        let opacityAnimatableValue = Hooks.useRef (AnimatedValue.Create(if isVisible then 1.0 else 0.0))
        let maybeOngoingAnimationHook = Hooks.useRef<Option<Animation>> None

        Hooks.useEffect(
            (fun () ->
                if isVisible then
                    isMountedHook.update true

                    // using runLater here is important, otherwise the animation gets sporadically botched
                    LibClient.JsInterop.runLater (System.TimeSpan.FromMilliseconds 10.) (fun () ->
                        Helpers.animate maybeOngoingAnimationHook opacityAnimatableValue.current 1.0 NoopFn
                    )
                else
                    // using runLater here is important, otherwise the animation gets sporadically botched
                    LibClient.JsInterop.runLater (System.TimeSpan.FromMilliseconds 10.) (fun () ->
                        Helpers.animate maybeOngoingAnimationHook opacityAnimatableValue.current 0.0 (fun () ->
                            isMountedHook.update false
                        )
                    )
            ),
            [| isVisible |]
        )

        if isMountedHook.current then
            RX.View(
                styles = (styles |> Option.defaultValue [||]),
                children =
                    elements {
                        RX.AnimatableView (
                            styles = [| Styles.scrimInner (AnimatableValue.Value opacityAnimatableValue.current) |],
                            children = [||]
                        )

                        let child =
                            match onPress with
                            | Some onPress ->
                                LC.TapCapture(
                                    onPress = onPress
                                )
                            | None ->
                                noElement

                        let isPanEnabled = onPanHorizontal.IsSome || onPanVertical.IsSome

                        if isPanEnabled then
                            RX.GestureView(
                                styles = [| Styles.gestureView |],
                                ?onPanHorizontal = onPanHorizontal,
                                ?onPanVertical = onPanVertical,
                                children =
                                    elements {
                                        child
                                    }
                            )
                        else
                            child
                    }
            )
        else
            noElement
[<AutoOpen>]
module LibClient.Components.TapCapture

open Fable.React

open LibClient

open ReactXP.Styles
open ReactXP.Components

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
        }

    let debug =
        makeViewStyles {
            backgroundColor (Color.Rgba(255, 0, 0, 0.3))
        }

    let button =
        makeViewStyles {
            flex 1
        }

type private Props =
    {
        OnPress: ReactEvent.Action -> unit
        MaybeOnHoverStart: Option<Browser.Types.PointerEvent -> unit>
        MaybeOnHoverEnd: Option<Browser.Types.PointerEvent -> unit>
        MaybeOnPressIn: Option<Browser.Types.PointerEvent -> unit>
        MaybeOnPressOut: Option<Browser.Types.PointerEvent -> unit>
        MaybePointerState: Option<LC.Pointer.State.PointerState>
        MaybeStyles: Option<array<ViewStyles>>
        key: Option<string>
    }

// Using a class-based component rather than a function-based one so that we have access to the component instance via the this pointer.
// See https://github.com/fable-compiler/fable-react/issues/240
type private TapCaptureComponent(initialProps: Props) =
    inherit Fable.React.PureStatelessComponent<Props>(initialProps)

    let mutable maybeTimeoutReference: Option<int> = None
    let mutable maybePressInCoords: Option<float * float> = None

    let onPress (e: Browser.Types.PointerEvent) : unit =
        // since we handle everything in the OnPressOut event, we stop propagation
        // for the regular OnPress
        e.stopPropagation()

    let onPressIn
            (maybeOnPressIn: Option<Browser.Types.PointerEvent -> unit>)
            (maybePointerState: Option<LC.Pointer.State.PointerState>)
            (e: Browser.Types.PointerEvent) : unit =
        maybePressInCoords <- e.CrossPlatformPageXY

        maybePointerState
        |> Option.map (fun ps -> ps.SetIsDepressed true)
        |> Option.orElse maybeOnPressIn
        |> Option.sideEffect (fun fn -> fn e)

    let onPressOut
            (this: ReactElement)
            (onPress: ReactEvent.Action -> unit)
            (maybeOnPressOut: Option<Browser.Types.PointerEvent -> unit>)
            (maybePointerState: Option<LC.Pointer.State.PointerState>)
            (e: Browser.Types.PointerEvent) : unit =
        let isDrag =
            match (maybePressInCoords, e.CrossPlatformPageXY) with
            | (Some (pressInX, pressInY), Some (x, y)) ->
                let deltaX = x - pressInX
                let deltaY = y - pressInY
                let distance = deltaX * deltaX + deltaY * deltaY |> sqrt
                distance > 5. // allow for fat fingering
            | _ -> true

        maybePressInCoords <- None

        maybePointerState
        |> Option.map (fun ps -> ps.SetIsDepressed false)
        |> Option.orElse maybeOnPressOut
        |> Option.sideEffect (fun fn -> fn e)

        if not isDrag then
            ReactXP.UserInterface.dismissKeyboard()

            // To prevent conflict with scrolling on web, only call props.onPress if the event is cancelable
            // until this PR is merged https://github.com/microsoft/reactxp/pull/1252
            if (e.cancelable && ReactXP.Runtime.isWeb()) || ReactXP.Runtime.isNative() then
                e.stopPropagation();

                // hover on pointer state does not seem to be turned off correctly with just the ReactXP
                // onHover related props, so we'll try to turn it off manually here
                maybePointerState
                |> Option.sideEffect (fun pointerState ->
                        pointerState.SetIsHovered false e
                        maybeTimeoutReference <- Some (Fable.Core.JS.setTimeout (fun _ -> pointerState.SetIsDepressed false e) 1500)
                   )

                onPress ((ReactEvent.Pointer.OfBrowserEvent e).WithSource this |> ReactEvent.Action.Make)

    override this.render() =
        let props = this.props

        RX.View (
            styles =
                [|
                    Styles.view

                    if LibClient.Components.TapCaptureDebugVisibility.isVisibleForDebug then
                        Styles.debug

                    yield! (props.MaybeStyles |> Option.getOrElse Array.empty)
                |],
            children =
                elements {
                    RX.Button(
                        styles = [| Styles.button |],
                        onPress = onPress,
                        onPressIn = onPressIn props.MaybeOnPressIn props.MaybePointerState,
                        onPressOut = onPressOut this props.OnPress props.MaybeOnPressOut props.MaybePointerState,
                        ?onHoverStart = (props.MaybePointerState |> Option.map (fun ps -> ps.SetIsHovered true) |> Option.orElse props.MaybeOnHoverStart),
                        ?onHoverEnd = (props.MaybePointerState |> Option.map (fun ps -> ps.SetIsHovered false) |> Option.orElse props.MaybeOnHoverEnd)
                    )
                },
            // added to mark the element as native element. This is hack to fix a bug on react native android. More info -
            //   https://github.com/facebook/react-native/issues/9382
            //   https://github.com/microsoft/reactxp/pull/780
            onLayout = ignore
        )

    override _.componentWillUnmount () =
        maybeTimeoutReference
        |> Option.sideEffect (fun timeoutReference -> (Fable.Core.JS.clearTimeout timeoutReference))

type LibClient.Components.Constructors.LC with
    static member TapCapture(
            onPress: ReactEvent.Action -> unit,
            ?onHoverStart: Browser.Types.PointerEvent -> unit,
            ?onHoverEnd: Browser.Types.PointerEvent -> unit,
            ?onPressIn: Browser.Types.PointerEvent -> unit,
            ?onPressOut: Browser.Types.PointerEvent -> unit,
            // NOTE: this prop overrides the preceding four. We typically don't like doing this kind of
            // modelling, but in this case it seems to lead to a nicer user experience than having
            // two separate components with different names.
            ?pointerState: LC.Pointer.State.PointerState,
            ?styles: array<ViewStyles>,
            ?key: string) : ReactElement =
        let props =
            {
                OnPress = onPress
                MaybeOnHoverStart = onHoverStart
                MaybeOnHoverEnd = onHoverEnd
                MaybeOnPressIn = onPressIn
                MaybeOnPressOut = onPressOut
                MaybePointerState = pointerState
                MaybeStyles = styles
                key = key
            }
        Fable.React.Helpers.ofType<TapCaptureComponent,_,_> props Seq.empty

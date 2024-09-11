module LibClient.Components.Draggable

open System
open LibClient
open Fable.Core.JsInterop
open ReactXP.Components
open ReactXP.Styles.Animation
open ReactXP.Styles

[<RequireQualifiedAccess>]
type Position =
| Left
| Right
| Up
| Down
| Base

type IDraggableRef =
    abstract member SetPosition:     Position -> bool;
    abstract member OnPanHorizontal: ReactXP.Components.GestureView.PanGestureState -> unit;
    abstract member OnPanVertical:   ReactXP.Components.GestureView.PanGestureState -> unit;

type DragTarget = {|
    ForwardThreshold:  int
    Offset:            int
    BackwardThreshold: int
|}

type PositionChangeReason =
| AnimationFinished
| ManualDrag

type Change =
| DragInducedAnimationStarted  of Target: Position
| PositionChanged of Target: Position * PositionChangeReason

type Props = (* GenerateMakeFunction *) {
    Left:            DragTarget     option // defaultWithAutoWrap None
    Right:           DragTarget     option // defaultWithAutoWrap None
    Up:              DragTarget     option // defaultWithAutoWrap None
    Down:            DragTarget     option // defaultWithAutoWrap None
    BaseOffset:      int * int             // default (0, 0)

    OnChange:        (Change -> unit) option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
    ref: (LibClient.JsInterop.JsNullable<IDraggableRef> -> unit) option // defaultWithAutoWrap JsUndefined
}
with
    static member private Range (maybeNegative: Option<DragTarget>) (maybePositive: Option<DragTarget>) : int * int =
        match (maybeNegative, maybePositive) with
        | (None,                None)                -> (0, 0)
        | (Some negativeTarget, None)                -> (-negativeTarget.Offset, 0)
        | (None,                Some positiveTarget) -> (0, positiveTarget.Offset)
        | (Some negativeTarget, Some positiveTarget) -> (-negativeTarget.Offset, positiveTarget.Offset)

    member this.RangeX : int * int =
        Props.Range this.Left this.Right

    member this.RangeY : int * int =
        Props.Range this.Up this.Down

    member this.BaseOffsetX : int = fst this.BaseOffset

    member this.BaseOffsetY : int = snd this.BaseOffset

type Direction =
| Horizontal
| Vertical

type Estate = {
    RangeX:           int * int
    RangeY:           int * int
    LastRestX:        int
    LastRestY:        int
    LastRestPosition: Position
    AniValueX:        AnimatedValue
    AniValueY:        AnimatedValue
}

[<RequireQualifiedAccess>]
module private Styles =
    let contents (estate: Estate) =
        makeAnimatableViewStyles {
            Overflow.Visible

            animatedTransform [
                [ animatedTranslateX (AnimatableValue.Value estate.AniValueX) ]
                [ animatedTranslateY (AnimatableValue.Value estate.AniValueY) ]
            ]
        }

let contentsView (child: ReactElement) (props: Props) (estate: Estate) : ReactElement =
    let maybePropsStyles: Option<List<ReactXP.LegacyStyles.RuntimeStyles>> = props?__style

    element {
        RX.AnimatableView (
            styles =
                [|
                    // HACK: only needed until this component is converted to F# Dialect.
                    yield!
                        match maybePropsStyles with
                        | Some propStyles ->
                            // It's critical we lie here and convert for View rather than AniView, since the latter does not convert as expected.
                            ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent<array<AnimatableViewStyles>> "ReactXP.Components.View" propStyles
                        | None ->
                            Array.empty

                    Styles.contents estate
                |],
            children = [| child |]
        )
    }


let private limitToRange (range: int * int) (value: int) : int =
    value
    |> min (snd range)
    |> max (fst range)

type ReactXP.Components.GestureView.PanGestureState with
    member this.DeltaX : int =
        this.pageX - this.initialPageX |> int

    member this.DeltaY : int =
        this.pageY - this.initialPageY |> int

type Draggable(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Draggable>("LibClient.Components.Draggable", _initialProps, Actions, hasStyles = true)

    let mutable maybeLastGestureState: Option<ReactXP.Components.GestureView.PanGestureState> = None

    // There is a bug, I think in the GestureView implementation, which manifests in the
    // browser's device emulation. After receiving a series of GestureStates that have
    // properly set pageX/Y values, the final GestureState, with isComplete = true,
    // has pageX/Y values as undefined. This makes it impossible to calculate the full
    // gesture's delta. So we hack, but using the last gesture's values.
    let currentOrLastDatafulGestureState (current: ReactXP.Components.GestureView.PanGestureState) : ReactXP.Components.GestureView.PanGestureState =
        if current.isComplete && current.isTouch && isNullOrUndefined current.pageX then
            match maybeLastGestureState with
            | Some last ->
                if last.initialPageX = current.initialPageX && last.initialPageY = current.initialPageY then
                    LibClient.JsInterop.extendRecord
                        [
                            "pageX" ==> last.pageX
                            "pageY" ==> last.pageY
                        ]
                        current
                else
                    current
            | None -> current
        else
            maybeLastGestureState <- Some current
            current


    let mutable maybeOngoingAnimation: Option<Animation> = None

    let animate (setRestP: int -> unit) (value: AnimatedValue) (baseOffset: int) (maybeFromP: Option<int>) (toP: int) (maybeOnComplete: Option<unit -> unit>) : unit =
        maybeOngoingAnimation |> Option.sideEffect (fun ongoingAnimation ->
            maybeOngoingAnimation <- None
            ongoingAnimation.Stop()
        )

        maybeFromP |> Option.sideEffect (fun fromP -> value.SetValue (fromP + baseOffset))
        let animation =
            Animation.Timing(
                value,
                toValue = (toP + baseOffset |> double),
                duration = TimeSpan.FromMilliseconds 100
            )
        maybeOngoingAnimation <- Some animation
        animation.Start (fun () ->
            if maybeOngoingAnimation = Some animation then
                maybeOngoingAnimation <- None
                maybeOnComplete |> Option.sideEffect (fun fn -> fn ())
            setRestP toP
        )


    let onPan
        (maybeOnChange: Option<Change -> unit>)
        (baseOffset: int)
        (lastRestP: int)
        (lastRestPosition: Position)
        (range: int * int)
        (aniValue: ReactXP.Styles.Animation.AnimatedValue)
        (maybeNegativeDragTarget: Option<DragTarget> * Position)
        (maybePositiveDragTarget: Option<DragTarget> * Position)
        (setRestP: int -> unit)
        (setLastRestPosition: Position -> unit)
        (rawDeltaP: int)
        (isGestureComplete: bool)
        : unit =

        let nextP = lastRestP + rawDeltaP |> limitToRange range
        let deltaP = nextP - lastRestP

        match isGestureComplete with
        | false -> aniValue.SetValue (nextP + baseOffset)
        | true  ->
            match nextP with
            | 0 ->
                setRestP 0
                setLastRestPosition Position.Base
                maybeOnChange |> Option.sideEffect (fun fn ->
                    fn (PositionChanged (Position.Base, ManualDrag))
                )

            | _ ->
                let (targetRestP, targetRestPosition) =
                    match (lastRestP, deltaP > 0, fst maybeNegativeDragTarget, fst maybePositiveDragTarget) with
                    // from the center, moving negative, passed the open threshold -> open
                    | (0, false, Some negativeTarget, _) when -nextP > negativeTarget.ForwardThreshold -> (-negativeTarget.Offset, snd maybeNegativeDragTarget)
                    // from the center, moving negative, but not far enough, stay centred
                    | (0, false, Some negativeTarget, _) when -nextP <= negativeTarget.ForwardThreshold -> (0, Position.Base)
                    // from the center, moving positive, passed the open threshold -> open
                    | (0, true, _, Some positiveTarget) when nextP > positiveTarget.ForwardThreshold -> (positiveTarget.Offset, snd maybePositiveDragTarget)
                    // from the center, moving positive, but not far enough, stay centred
                    | (0, true, _, Some positiveTarget) when nextP <= positiveTarget.ForwardThreshold -> (0, Position.Base)

                    // from the negative open position, moving positive, passed the close threshold -> close
                    | (lastRestP, true, Some negativeTarget, _) when (lastRestP = -negativeTarget.Offset) && (deltaP > negativeTarget.BackwardThreshold) -> (0, Position.Base)
                    // from the negative open position, moving positive, but not far enough, stay negative
                    | (lastRestP, true, Some negativeTarget, _) when (lastRestP = -negativeTarget.Offset) && (deltaP <= negativeTarget.BackwardThreshold) -> (-negativeTarget.Offset, snd maybeNegativeDragTarget)

                    // from the positive open position, moving negative, passed the close threshold -> close
                    | (lastRestP, false, _, Some positiveTarget) when (lastRestP = positiveTarget.Offset) && (-deltaP > positiveTarget.BackwardThreshold) -> (0, Position.Base)
                    // from the positive open position, moving negative, but not far enough, stay positive
                    | (lastRestP, false, _, Some positiveTarget) when (lastRestP = positiveTarget.Offset) && (-deltaP <= positiveTarget.BackwardThreshold) -> (positiveTarget.Offset, snd maybePositiveDragTarget)

                    // we shouldn't make it here, since the handler won't be attached
                    | (_, _, None, None) -> (0, Position.Base)

                    | _ -> (lastRestP, lastRestPosition)

                maybeOnChange |> Option.sideEffect (fun fn -> fn (DragInducedAnimationStarted targetRestPosition))
                animate setRestP aniValue baseOffset (Some nextP) targetRestP (Some (fun () ->
                    maybeOnChange |> Option.sideEffect (fun fn ->
                        setLastRestPosition targetRestPosition
                        fn (PositionChanged (targetRestPosition, AnimationFinished))
                    )
                ))


    member this.SetLastRestPosition (value: Position) : unit =
       this.SetEstate (fun estate _ -> { estate with LastRestPosition = value })

    member this.SetLastRestX (x: int) : unit =
       this.SetEstate (fun estate _ -> { estate with LastRestX = x })

    member this.SetLastRestY (y: int) : unit =
       this.SetEstate (fun estate _ -> { estate with LastRestY = y })

    member this.OnPanX (rawDeltaX: int) (isGestureComplete: bool) =
        onPan
            this.props.OnChange
            this.props.BaseOffsetX
            this.state.LastRestX
            this.state.LastRestPosition
            this.state.RangeX
            this.state.AniValueX
            (this.props.Left,  Position.Left)
            (this.props.Right, Position.Right)
            this.SetLastRestX
            this.SetLastRestPosition
            rawDeltaX
            isGestureComplete

    member this.OnPanY (rawDeltaY: int) (isGestureComplete: bool) =
        onPan
            this.props.OnChange
            this.props.BaseOffsetY
            this.state.LastRestY
            this.state.LastRestPosition
            this.state.RangeY
            this.state.AniValueY
            (this.props.Up,   Position.Up)
            (this.props.Down, Position.Down)
            this.SetLastRestY
            this.SetLastRestPosition
            rawDeltaY
            isGestureComplete

    interface IDraggableRef with
        member this.SetPosition (newPosition: Position) : bool =
            // NOTE we basically do not support diagonal transitions — if somebody is trying
            // to transition diagonally, they'll exprience a jump along one of the axis, and
            // a smooth transition along the other. Implementing it properly can be done,
            // but I'm sure we'll never need this in practice, so didn't bother.
            let maybeTarget =
                match (newPosition, this.props.Left, this.props.Right, this.props.Up, this.props.Down) with
                | (Position.Left, Some target,           _,           _,           _) -> (this.SetLastRestX, this.state.AniValueX, this.props.BaseOffsetX, -target.Offset, fun () -> this.state.AniValueY.SetValue this.props.BaseOffsetY) |> Some
                | (Position.Right,          _, Some target,           _,           _) -> (this.SetLastRestX, this.state.AniValueX, this.props.BaseOffsetX,  target.Offset, fun () -> this.state.AniValueY.SetValue this.props.BaseOffsetY) |> Some
                | (Position.Up,             _,           _, Some target,           _) -> (this.SetLastRestY, this.state.AniValueY, this.props.BaseOffsetY, -target.Offset, fun () -> this.state.AniValueX.SetValue this.props.BaseOffsetX) |> Some
                | (Position.Down,           _,           _,           _, Some target) -> (this.SetLastRestY, this.state.AniValueY, this.props.BaseOffsetY,  target.Offset, fun () -> this.state.AniValueX.SetValue this.props.BaseOffsetX) |> Some
                | (Position.Base,           _,           _,           _,           _) ->
                    match this.state.LastRestPosition with
                    | Position.Right | Position.Left -> (this.SetLastRestX, this.state.AniValueX, this.props.BaseOffsetX, 0, fun () -> this.state.AniValueY.SetValue this.props.BaseOffsetY) |> Some
                    | Position.Up    | Position.Down -> (this.SetLastRestY, this.state.AniValueY, this.props.BaseOffsetY, 0, fun () -> this.state.AniValueX.SetValue this.props.BaseOffsetX) |> Some
                    // NOTE this case is a hack, it disregards the possibility that we're moving back to base from maybe
                    // a vertical position. This case used to be just None, but when you double click the
                    // trigger button, the second click can fall on the scrim, which immediately requests
                    // a SetPosition Base, when LastRestPosition is still Base (irrespective of whether
                    // we set it at the start or end of the animation — even at the start, the second click
                    // gets in before the state update)
                    | Position.Base                  -> (this.SetLastRestX, this.state.AniValueX, this.props.BaseOffsetX, 0, fun () -> this.state.AniValueY.SetValue this.props.BaseOffsetY) |> Some
                | _ -> None

            maybeTarget |> Option.sideEffect (fun (setRestP, value, baseOffset, targetP, resetNonAnimatingAxis) ->
                resetNonAnimatingAxis ()
                animate setRestP value baseOffset None targetP (Some (fun () -> this.SetLastRestPosition newPosition))
            )

            maybeTarget.IsSome

        member this.OnPanHorizontal (rawGestureState: ReactXP.Components.GestureView.PanGestureState) : unit =
            this.OnPanHorizontal rawGestureState

        member this.OnPanVertical (rawGestureState: ReactXP.Components.GestureView.PanGestureState) : unit =
            this.OnPanVertical rawGestureState


    override _.GetInitialEstate(initialProps: Props) : Estate =
        let animationValueX = initialProps.BaseOffset |> fst |> double |> AnimatedValue.Create
        let animationValueY = initialProps.BaseOffset |> snd |> double |> AnimatedValue.Create

        {
            LastRestX        = 0
            LastRestY        = 0
            LastRestPosition = Position.Base
            AniValueX        = animationValueX
            AniValueY        = animationValueY
            RangeX           = initialProps.RangeX
            RangeY           = initialProps.RangeY
        }

    override this.ComponentWillReceiveProps (nextProps: Props) : unit =
        if this.props.Left <> nextProps.Left || this.props.Right <> nextProps.Right then
            this.SetEstate (fun estate _ -> { estate with RangeX = nextProps.RangeX })

        if this.props.Up <> nextProps.Up || this.props.Down <> nextProps.Down then
            this.SetEstate (fun estate _ -> { estate with RangeY = nextProps.RangeY })

        if this.props.BaseOffset <> nextProps.BaseOffset then
            if this.state.LastRestX = 0 && this.state.LastRestY = 0 then
                this.state.AniValueX.SetValue nextProps.BaseOffsetX
                this.state.AniValueY.SetValue nextProps.BaseOffsetY

    member this.OnPanHorizontal (rawGestureState: ReactXP.Components.GestureView.PanGestureState) : unit =
        let gestureState = currentOrLastDatafulGestureState rawGestureState
        this.OnPanX gestureState.DeltaX gestureState.isComplete

    member this.OnPanVertical (rawGestureState: ReactXP.Components.GestureView.PanGestureState) : unit =
        let gestureState = currentOrLastDatafulGestureState rawGestureState
        this.OnPanY gestureState.DeltaY gestureState.isComplete


and Actions(this: Draggable) =
    member _.OnPanHorizontal (rawGestureState: ReactXP.Components.GestureView.PanGestureState) : unit =
        this.OnPanHorizontal rawGestureState

    member _.OnPanVertical (rawGestureState: ReactXP.Components.GestureView.PanGestureState) : unit =
        this.OnPanVertical rawGestureState

let Make = makeConstructor<Draggable, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
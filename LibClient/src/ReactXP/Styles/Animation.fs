module ReactXP.Styles.Animation

open System
open Fable.Core.JsInterop
open LibClient

[<RequireQualifiedAccess>]
type Easing =
| Linear
| Out
| In
| InOut
| InBack
| OutBack
| InOutBack
| StepStart
| StepEnd
| Steps of Intervals: int * MaybeEnd: Option<bool>
| CubicBezier of Coords1: (double * double) * Coords2: (double * double)
with
    member this.ToReactXP : obj =
        match this with
        | Linear -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?Linear()
        | Out -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?Out()
        | In -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?In()
        | InOut -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?InOut()
        | InBack -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?InBack()
        | OutBack -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?OutBack()
        | InOutBack -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?InOutBack()
        | StepStart -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?StepStart()
        | StepEnd -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?StepEnd()
        | Steps (intervals, maybeEnd) -> ReactXP.Helpers.ReactXPRaw?Animated?Easing?Steps(intervals, maybeEnd |> Option.map box |> Option.toObj)
        | CubicBezier ((x1, y1), (x2, y2)) ->
            // TODO: would be nice to restrict values to 0 >= v <= 1
            ReactXP.Helpers.ReactXPRaw?Animated?Easing?CubicBezier(x1, y1, x2, y2)

type InterpolationConfig internal(raw: obj) =
    static member Create(mappings: seq<double * double>) =
        createObj
            [
                ("inputRange", mappings |> Seq.map fst |> Seq.toArray |> box)
                ("outputRange", mappings |> Seq.map snd |> Seq.toArray |> box)
            ]
        |> InterpolationConfig

    static member Create(mappings: seq<double * Color>) =
        createObj
            [
                ("inputRange", mappings |> Seq.map fst |> Seq.toArray |> box)
                ("outputRange", mappings |> Seq.map snd |> Seq.map (fun color -> color.ToReactXPString) |> Seq.toArray |> box)
            ]
        |> InterpolationConfig

    member internal _.Raw = raw

type RawInterpolatedValue =
    abstract member interpolate: obj -> RawInterpolatedValue

type InterpolatedValue internal(raw: RawInterpolatedValue) =
    member _.Interpolate(config: InterpolationConfig): InterpolatedValue =
        raw.interpolate(config)
        |> InterpolatedValue

    member internal _.Raw = raw

type RawAnimatedValue =
    abstract member setValue: double -> unit
    abstract member interpolate: obj -> RawInterpolatedValue

type AnimatedValue internal(raw: RawAnimatedValue) =
    static member Create (value: double) : AnimatedValue =
        value
        |> ReactXP.Helpers.ReactXPRaw?Animated?createValue
        |> AnimatedValue

    member internal _.Raw = raw

    member _.SetValue (value: double): unit =
        raw.setValue value

    member _.SetValue (value: int): unit =
        raw.setValue (double value)

    member _.Interpolate (config: InterpolationConfig): InterpolatedValue =
        raw.interpolate(config.Raw)
        |> InterpolatedValue

[<RequireQualifiedAccess>]
type AnimatableValue =
| Value of AnimatedValue
| Interpolated of InterpolatedValue
with
    member this.Raw =
        match this with
        | Value value -> box value.Raw
        | Interpolated interpolated -> box interpolated.Raw

type RawAnimation =
    abstract member start: Option<unit -> unit> -> unit
    abstract member stop:  unit -> unit

type Animation internal(raw: RawAnimation) =
    static member Timing(
            animatedValue: AnimatedValue,
            toValue: double,
            duration: TimeSpan,
            ?delay: TimeSpan,
            ?easing: Easing,
            ?restartFrom: double)
            : Animation =
        let fields =
            createObj
                [|
                    ("toValue",  toValue :> obj)
                    ("duration", duration.TotalMilliseconds :> obj)
                    // Per the caveats listed at https://reactnative.dev/blog/2017/02/14/using-native-driver-for-animated#caveats, not all properties
                    // can be natively animated. Despite being on a fairly recent version of RN (0.70.8 at time of writing), I have found that even
                    // some basic things like text color cannot be animated with the native driver, so I've explicitly disabled it for now. In the
                    // future we should enable this if we can, or perhaps allow the caller to specify.
                    ("useNativeDriver", box false)

                    match delay with
                    | None -> Noop
                    | Some delay -> ("delay", delay.TotalMilliseconds :> obj)

                    match easing with
                    | None -> Noop
                    | Some easing -> ("easing", easing.ToReactXP)

                    match restartFrom with
                    | None -> Noop
                    | Some restartFrom -> ("loop", createObj [| ("restartFrom", restartFrom :> obj) |])
                |]

        ReactXP.Helpers.ReactXPRaw?Animated?timing(animatedValue.Raw, fields)
        |> Animation

    static member Parallel([<ParamArray>] animations: array<Animation>): Animation =
        ReactXP.Helpers.ReactXPRaw?Animated?("parallel")(animations |> Seq.map (fun animation -> animation.Raw) |> Seq.toArray)
        |> Animation

    static member Sequence([<ParamArray>] animations: array<Animation>): Animation =
        ReactXP.Helpers.ReactXPRaw?Animated?sequence(animations |> Seq.map (fun animation -> animation.Raw) |> Seq.toArray)
        |> Animation

    member internal _.Raw = raw

    member _.Start(?onComplete: unit -> unit) : unit =
        raw.start onComplete

    member _.Stop() : unit =
        raw.stop ()

// TODO: delete everything from here down once animation migration is complete
type GetOrCreateAnimatedValue      = (* key *) string -> (* initialValue *) double -> RawAnimatedValue
type AnimatedRulesConstructor      = GetOrCreateAnimatedValue -> ReactXPStyleRulesObject
type AnimatedAnimationsConstructor = GetOrCreateAnimatedValue -> RawAnimation

[<AutoOpen>]
module ReactXPAnimationExtensions =
    type RawAnimatedValue with
        static member Simple (key: string) (initialValue: double) : GetOrCreateAnimatedValue -> RawAnimatedValue =
            fun getOrCreateAnimatedValue -> getOrCreateAnimatedValue key initialValue

    type RawAnimation with
        static member Simple (toValue: double, durationMillis: int) : (RawAnimatedValue -> RawAnimation) =
            RawAnimation.Simple (toValue, durationMillis, ?easing = None)

        static member Simple (toValue: double, durationMillis: int, ?easing: Easing) : (RawAnimatedValue -> RawAnimation) =
            fun (value: RawAnimatedValue) ->
                let requiredFields = [
                    ("toValue",  toValue :> obj)
                    ("duration", durationMillis :> obj)
                    ("useNativeDriver", true)
                ]

                let fields =
                    match easing with
                    | None -> requiredFields
                    | Some easing -> ("easing", easing.ToReactXP) :: requiredFields

                ReactXP.Helpers.ReactXPRaw?Animated?timing(value, createObj fields)

        static member Parallel (a1: RawAnimatedValue -> RawAnimation, a2: RawAnimatedValue -> RawAnimation) =
            fun (v1: RawAnimatedValue) (v2: RawAnimatedValue) ->
                ReactXP.Helpers.ReactXPRaw?Animated?("parallel")([
                    a1 v1
                    a2 v2
                ] |> Array.ofList)

        static member Parallel (a1: RawAnimatedValue -> RawAnimation, a2: RawAnimatedValue -> RawAnimation, a3: RawAnimatedValue -> RawAnimation) =
            fun (v1: RawAnimatedValue) (v2: RawAnimatedValue) (v3: RawAnimatedValue) ->
                ReactXP.Helpers.ReactXPRaw?Animated?("parallel")([
                    a1 v1
                    a2 v2
                    a3 v3
                ] |> Array.ofList)

        static member Sequence (a1: RawAnimatedValue -> RawAnimation, a2: RawAnimatedValue -> RawAnimation) =
            fun (v1: RawAnimatedValue) (v2: RawAnimatedValue) ->
                ReactXP.Helpers.ReactXPRaw?Animated?("sequence")([
                    a1 v1
                    a2 v2
                ] |> Array.ofList)

        // TODO trivially add `sequence` and higher arrity versions

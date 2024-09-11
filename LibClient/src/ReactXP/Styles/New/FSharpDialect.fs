[<AutoOpen>]
module ReactXP.Styles.FSharpDialect

open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
type ScrollViewStyles =
| InternalRepresentationIsForJsRuntimeConsumptionOnly

[<RequireQualifiedAccess>]
type ViewStyles =
| InternalRepresentationIsForJsRuntimeConsumptionOnly

[<RequireQualifiedAccess>]
type AnimatableViewStyles =
| InternalRepresentationIsForJsRuntimeConsumptionOnly

[<RequireQualifiedAccess>]
type TextStyles =
| InternalRepresentationIsForJsRuntimeConsumptionOnly

[<RequireQualifiedAccess>]
type AnimatableTextStyles =
| InternalRepresentationIsForJsRuntimeConsumptionOnly

// TODO: we should also have TextInputStyles rather than re-using ViewStyles, since the latter does
// not permit some rules relevant to TextInput (fontSize and color)
[<RequireQualifiedAccess>]
type AnimatableTextInputStyles =
| InternalRepresentationIsForJsRuntimeConsumptionOnly

let legacyTheme (value: ReactXP.LegacyStyles.Styles) : List<ReactXP.LegacyStyles.RuntimeStyles> =
    value
    |> ReactXP.LegacyStyles.Designtime.processStyles
    |> List.ofOneItem

type ViewStylesBuilder() =
    member _.Zero () : seq<RawReactXPStyleRule> =
        Seq.empty

    member _.Yield (rule: RawReactXPViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexChildrenStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexChildrenStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPTransformStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPTransformStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Combine (a: seq<RawReactXPStyleRule>, b: seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a b

    member _.Combine (a: seq<RawReactXPStyleRule>, bf: unit -> seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a (bf())

    member _.Delay (expr: unit -> seq<RawReactXPStyleRule>) : unit -> seq<RawReactXPStyleRule> = expr

    member _.Run (f: unit -> seq<RawReactXPStyleRule>) : ViewStyles =
        !!(!!(f()) |> createObj |> ReactXP.Helpers.ReactXPRaw?Styles?createViewStyle)

let makeViewStyles = ViewStylesBuilder()

type AnimatableViewStylesBuilder() =
    member _.Zero () : seq<RawReactXPStyleRule> =
        Seq.empty

    member _.Yield (rule: RawReactXPViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexChildrenStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexChildrenStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedTransformStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedTransformStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Combine (a: seq<RawReactXPStyleRule>, b: seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a b

    member _.Combine (a: seq<RawReactXPStyleRule>, bf: unit -> seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a (bf())

    member _.Delay (expr: unit -> seq<RawReactXPStyleRule>) : unit -> seq<RawReactXPStyleRule> = expr

    member _.Run (f: unit -> seq<RawReactXPStyleRule>) : AnimatableViewStyles =
        !!(!!(f()) |> createObj |> ReactXP.Helpers.ReactXPRaw?Styles?createAnimatedViewStyle)

let makeAnimatableViewStyles = AnimatableViewStylesBuilder()

type ScrollViewStylesBuilder() =
    member _.Zero () : seq<RawReactXPStyleRule> =
        Seq.empty

    member _.Yield (rule: RawReactXPViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Combine (a: seq<RawReactXPStyleRule>, b: seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a b

    member _.Combine (a: seq<RawReactXPStyleRule>, bf: unit -> seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a (bf())

    member _.Delay (expr: unit -> seq<RawReactXPStyleRule>) : unit -> seq<RawReactXPStyleRule> = expr

    member _.Run (f: unit -> seq<RawReactXPStyleRule>) : ScrollViewStyles =
        !!(!!(f()) |> createObj |> ReactXP.Helpers.ReactXPRaw?Styles?createViewStyle)

let makeScrollViewStyles = ScrollViewStylesBuilder()

type TextStylesBuilder() =
    member _.Zero () : seq<RawReactXPStyleRule> =
        Seq.empty

    member _.Yield (rule: RawReactXPTextStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rule: RawReactXPViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexChildrenStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexChildrenStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPTransformStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPTransformStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Combine (a: seq<RawReactXPStyleRule>, b: seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a b

    member _.Combine (a: seq<RawReactXPStyleRule>, bf: unit -> seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a (bf())

    member _.Delay (expr: unit -> seq<RawReactXPStyleRule>) : unit -> seq<RawReactXPStyleRule> = expr

    member _.Run (f: unit -> seq<RawReactXPStyleRule>) : TextStyles =
        !!(!!(f()) |> createObj |> ReactXP.Helpers.ReactXPRaw?Styles?createTextStyle)

let makeTextStyles = TextStylesBuilder()

type AnimatableTextStylesBuilder() =
    member _.Zero () : seq<RawReactXPStyleRule> =
        Seq.empty

    member _.Yield (rule: RawReactXPTextStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPTextStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexChildrenStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexChildrenStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPTransformStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPTransformStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedTextStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedTextStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedTransformStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedTransformStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Combine (a: seq<RawReactXPStyleRule>, b: seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a b

    member _.Combine (a: seq<RawReactXPStyleRule>, bf: unit -> seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a (bf())

    member _.Delay (expr: unit -> seq<RawReactXPStyleRule>) : unit -> seq<RawReactXPStyleRule> = expr

    member _.Run (f: unit -> seq<RawReactXPStyleRule>) : AnimatableTextStyles =
        !!(!!(f()) |> createObj |> ReactXP.Helpers.ReactXPRaw?Styles?createAnimatedTextStyle)

let makeAnimatableTextStyles = AnimatableTextStylesBuilder()

type AnimatableTextInputStylesBuilder() =
    member _.Zero () : seq<RawReactXPStyleRule> =
        Seq.empty

    member _.Yield (rule: RawReactXPTextStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPTextStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedTextStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedTextStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPFlexChildrenStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPFlexChildrenStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedViewStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedViewStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedFlexStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedFlexStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Yield (rule: RawReactXPAnimatedTransformStyleRule) : seq<RawReactXPStyleRule> =
        Seq.ofOneItem !!rule

    member _.Yield (rules: array<RawReactXPAnimatedTransformStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.ofArray !!rules

    member _.Combine (a: seq<RawReactXPStyleRule>, b: seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a b

    member _.Combine (a: seq<RawReactXPStyleRule>, bf: unit -> seq<RawReactXPStyleRule>) : seq<RawReactXPStyleRule> =
        Seq.append a (bf())

    member _.Delay (expr: unit -> seq<RawReactXPStyleRule>) : unit -> seq<RawReactXPStyleRule> = expr

    member _.Run (f: unit -> seq<RawReactXPStyleRule>) : AnimatableTextInputStyles =
        !!(!!(f()) |> createObj |> ReactXP.Helpers.ReactXPRaw?Styles?createAnimatedTextInputStyle)

let makeAnimatableTextInputStyles = AnimatableTextInputStylesBuilder()

open LibClient.Memoize

type ViewStyles with
    static member Memoize (fn: 'a -> ViewStyles) : 'a -> ViewStyles =
        memoize fn

    static member Memoize (fn: 'a -> 'b -> ViewStyles) : 'a -> 'b -> ViewStyles =
        memoize2 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> ViewStyles) : 'a -> 'b -> 'c -> ViewStyles =
        memoize3 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> ViewStyles) : 'a -> 'b -> 'c -> 'd -> ViewStyles =
        memoize4 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> 'e -> ViewStyles) : 'a -> 'b -> 'c -> 'd -> 'e -> ViewStyles =
        memoize5 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> 'e -> 'f -> ViewStyles) : 'a -> 'b -> 'c -> 'd -> 'e -> 'f -> ViewStyles =
        memoize6 fn

type ScrollViewStyles with
    static member Memoize (fn: 'a -> ScrollViewStyles) : 'a -> ScrollViewStyles =
        memoize fn

    static member Memoize (fn: 'a -> 'b -> ScrollViewStyles) : 'a -> 'b -> ScrollViewStyles =
        memoize2 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> ScrollViewStyles) : 'a -> 'b -> 'c -> ScrollViewStyles =
        memoize3 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> ScrollViewStyles) : 'a -> 'b -> 'c -> 'd -> ScrollViewStyles =
        memoize4 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> 'e -> ScrollViewStyles) : 'a -> 'b -> 'c -> 'd -> 'e -> ScrollViewStyles =
        memoize5 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> 'e -> 'f -> ScrollViewStyles) : 'a -> 'b -> 'c -> 'd -> 'e -> 'f -> ScrollViewStyles =
        memoize6 fn

type TextStyles with
    static member Memoize (fn: 'a -> TextStyles) : 'a -> TextStyles =
        memoize fn

    static member Memoize (fn: 'a -> 'b -> TextStyles) : 'a -> 'b -> TextStyles =
        memoize2 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> TextStyles) : 'a -> 'b -> 'c -> TextStyles =
        memoize3 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> TextStyles) : 'a -> 'b -> 'c -> 'd -> TextStyles =
        memoize4 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> 'e -> TextStyles) : 'a -> 'b -> 'c -> 'd -> 'e -> TextStyles =
        memoize5 fn

    static member Memoize (fn: 'a -> 'b -> 'c -> 'd -> 'e -> 'f -> TextStyles) : 'a -> 'b -> 'c -> 'd -> 'e -> 'f -> TextStyles =
        memoize6 fn


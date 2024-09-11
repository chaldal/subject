[<AutoOpen>]
module AutoOpenModuleForOr

[<RequireQualifiedAccess>]
type Or<'T, 'U> =
| Left  of 'T
| Right of 'U
| Both  of 'T * 'U
with
    static member OfOptions (maybeLeft: Option<'T>) (maybeRight: Option<'U>) : Option<Or<'T, 'U>> =
        match (maybeLeft, maybeRight) with
        | Some left, Some right -> Some (Both (left, right))
        | Some left, None       -> Some (Left left)
        | None, Some right      -> Some (Right right)
        | None, None            -> None

    member this.LeftOption : Option<'T> =
        match this with
        | Left left
        | Both (left, _) -> Some left
        | _              -> None

    member this.RightOption : Option<'U> =
        match this with
        | Right right
        | Both (_, right) -> Some right
        | _               -> None

    member this.WithoutLeft : Option<Or<'T, 'U>> =
        match this with
        | Left _          -> None
        | Right right     -> Right right |> Some
        | Both (_, right) -> Right right |> Some

    member this.WithLeft (newLeft: 'T) : Or<'T, 'U> =
        match this with
        | Left _          -> Left newLeft
        | Right right     -> Both (newLeft, right)
        | Both (_, right) -> Both (newLeft, right)

    member this.WithoutRight : Option<Or<'T, 'U>> =
        match this with
        | Right _        -> None
        | Left left      -> Left left |> Some
        | Both (left, _) -> Left left |> Some

    member this.WithRight (newRight: 'U) : Or<'T, 'U> =
        match this with
        | Right _        -> Right newRight
        | Left left      -> Both (left, newRight)
        | Both (left, _) -> Both (left, newRight)

#if !FABLE_COMPILER
open CodecLib
#endif

module Or =
    let toTuple (source: Or<'T, 'U>) : Option<'T> * Option<'U> =
        (source.LeftOption, source.RightOption)

    let unsafeFromTuple (tuple: Option<'T> * Option<'U>) : Or<'T, 'U> =
        Or.OfOptions (fst tuple) (snd tuple) |> Option.get

#if !FABLE_COMPILER

    let codec valueLeftCodec valueRightCodec : Codec<_, Or<'t, 'u>> =
        Codec.create (Ok << unsafeFromTuple)  toTuple
        |> Codec.compose (Codecs.tuple2 (Codecs.option valueLeftCodec) (Codecs.option valueRightCodec))

type Or<'T, 'U> with
    static member inline get_Codec () : Codec<_, Or<'t, 'u>> =
        Codec.create (Ok << Or.unsafeFromTuple)  Or.toTuple
        |> Codec.compose (Codecs.tuple2 (Codecs.option codecFor<_, 't>) (Codecs.option codecFor<_, 'u>))

#endif
[<AutoOpen>]
module LibUiSubject.Types

open LibClient

type Subjects<'Id, 'Subject when 'Id : comparison> = seq<'Id * AsyncData<'Subject>>

type SubjectsWithTotalCount<'Id, 'Subject when 'Id : comparison> = {
    Subjects:   Subjects<'Id, 'Subject>
    TotalCount: uint64
}

module Subjects =
    let private availableHelper (shouldIncludeFetching: bool) (f: 'Id * 'Subject -> 'U) (source: Subjects<'Id, 'Subject>) : seq<'U> =
        source
        |> Seq.filterMap (fun (id, subjectAD) ->
            match (shouldIncludeFetching, subjectAD) with
            | (_,    Available subject)          -> Some (f (id, subject))
            | (true, Fetching (Some oldSubject)) -> Some (f (id, oldSubject))
            | _                                  -> None
        )

    let available (source: Subjects<'Id, 'Subject>) : seq<'Subject> =
        availableHelper false snd source

    let availableIncludingFetching (source: Subjects<'Id, 'Subject>) : seq<'Subject> =
        availableHelper true snd source

    let availableKeyed (source: Subjects<'Id, 'Subject>) : Map<'Id, 'Subject> =
        availableHelper false identity source
        |> Map.ofSeq

    let availableKeyedIncludingFetching (source: Subjects<'Id, 'Subject>) : Map<'Id, 'Subject> =
        availableHelper true identity source
        |> Map.ofSeq

    let availablePairs (source: Subjects<'Id, 'Subject>) : seq<'Id * 'Subject> =
        availableHelper false identity source

    let availableIds (source: Subjects<'Id, 'Subject>) : seq<'Id> =
        availableHelper false fst source

    let map (fn: 'Subject -> 'U) (source: Subjects<'Id, 'Subject>) : Subjects<'Id, 'U> =
        source
        |> Seq.map (fun (id, subjectAD) -> id, subjectAD |> AsyncData.map fn)

    let mapAvailable (source: AsyncData<Subjects<'Id, 'Subject>>) : AsyncData<seq<'Subject>> =
        source |> AsyncData.map available

    let ids (source: Subjects<'Id, 'Subject>) : seq<'Id> =
        source |> Seq.map fst

    let subjects (source: Subjects<'Id, 'Subject>) : seq<AsyncData<'Subject>> =
        source |> Seq.map snd

module SubjectsWithTotalCount =
    let ofOneItem (idAndItem: 'Id * AsyncData<'Subject>) : SubjectsWithTotalCount<'Id, 'Subject> = {
        Subjects   = Seq.ofOneItem idAndItem
        TotalCount = 1UL
    }

    let map (fn: 'Subject -> 'U) (source: SubjectsWithTotalCount<'Id, 'Subject>): SubjectsWithTotalCount<'Id, 'U> =
        {
            Subjects = source.Subjects |> Subjects.map fn
            TotalCount = source.TotalCount
        }

    let subjects (source: SubjectsWithTotalCount<'Id, 'Subject>): Subjects<'Id, 'Subject> =
        source.Subjects

    let totalCount (source: SubjectsWithTotalCount<'Id, 'Subject>): uint64 =
        source.TotalCount

[<RequireQualifiedAccess>]
type WithSubjects<'Id, 'Subject when 'Id : comparison> =
| Raw           of (AsyncData<Subjects<'Id, 'Subject>> -> ReactElement)
| WhenAvailable of (Subjects<'Id, 'Subject> -> ReactElement)

[<RequireQualifiedAccess>]
type WithSubjectsWithCount<'Id, 'Subject when 'Id : comparison> =
| Raw           of (AsyncData<SubjectsWithTotalCount<'Id, 'Subject>> -> ReactElement)
| WhenAvailable of (SubjectsWithTotalCount<'Id, 'Subject> -> ReactElement)

[<RequireQualifiedAccess>]
type WithSubject<'Subject> =
| Raw           of (AsyncData<'Subject> -> ReactElement)
| WhenAvailable of ('Subject -> ReactElement)

type PropWithSubjectFactory =
    static member Make<'T> (whenAvailable: 'T -> ReactElement) =
        WithSubject.WhenAvailable whenAvailable

type PropWithSubjectsFactory =
    static member Make<'Id, 'Subject when 'Id : comparison> (whenAvailable: Subjects<'Id, 'Subject> -> ReactElement) =
        WithSubjects.WhenAvailable whenAvailable

type PropWithSubjectsWithCountFactory =
    static member Make<'Id, 'Subject when 'Id : comparison> (whenAvailable: SubjectsWithTotalCount<'Id, 'Subject> -> ReactElement) =
        WithSubjectsWithCount.WhenAvailable whenAvailable

type UseCache =
| No
| IfNewerThan of System.TimeSpan
| IfReasonablyFresh
| IfAvailable

module AsyncData =
    type AsyncData<'T> with
        member this.SideEffectIfNotNetworkFailure (fn: AsyncData<'T> -> unit) : unit =
            match this with
            | Failed NetworkFailure -> Noop
            | _                     -> fn this

    let sideEffectIfNotNetworkFailure (fn: AsyncData<'T> -> unit) (value: AsyncData<'T>) : unit =
        value.SideEffectIfNotNetworkFailure fn
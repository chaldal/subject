[<AutoOpen; CodecLib.CodecAutoGenerate>]
module SuiteT__EC__T.Types.T__LC__T

open System

type T__LC__TId = T__LC__TId of NonemptyString
with
    interface SubjectId with
        member this.IdString =
            let (T__LC__TId name) = this
            name.Value


type T__LC__T = {
    Name:      NonemptyString
    CreatedOn: DateTimeOffset
    Counter:   int
}
with
    interface Subject<T__LC__TId> with
        member this.SubjectCreatedOn =
            this.CreatedOn

        member this.SubjectId =
            T__LC__TId this.Name

[<RequireQualifiedAccess>]
type T__LC__TAction =
| Add of Value: int
| ResetIfNonZero
with interface LifeAction

[<RequireQualifiedAccess>]
type T__LC__TOpError =
| CounterIsZero
with interface OpError

[<RequireQualifiedAccess>]
type T__LC__TConstructor =
| New of Name: NonemptyString * Value: int
with interface Constructor

type T__LC__TLifeEvent = private NoLifeEvent of unit with interface LifeEvent

type T__LC__TNumericIndex = NoNumericIndex<T__LC__TOpError>
type T__LC__TStringIndex = NoStringIndex<T__LC__TOpError>
type T__LC__TSearchIndex = NoSearchIndex
type T__LC__TGeographyIndex = NoGeographyIndex

type T__LC__TIndex() = inherit SubjectIndex<T__LC__TIndex, T__LC__TNumericIndex, T__LC__TStringIndex, T__LC__TSearchIndex, T__LC__TGeographyIndex, T__LC__TOpError>()


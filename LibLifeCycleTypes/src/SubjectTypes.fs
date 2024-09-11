[<AutoOpen>]
module
#if FABLE_COMPILER
    // See comment in LibLifeCycleTypes/AssemblyInfo.fs
    LibLifeCycleTypes_SubjectTypes
#else
    LibLifeCycleTypes.SubjectTypes
#endif


open System

[<ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never); AutoOpen>]
module Helpers =

    let inline uncheckedUnbox<'destType> (x: obj) : 'destType =
    #if !FABLE_COMPILER
        Unchecked.unbox x
    #else
        unbox<'destType> x
    #endif

type SubjectId =
    inherit IComparable
    inherit CodecLib.IInterfaceCodec<SubjectId>
    abstract member IdString: string

// A Subject is something we care about independently modeling and persisting
type Subject =
    // Ideally we want this "untyped" Subject to be able to return an "untyped" SubjectId
    // This will be possible in F# 5, which will support members on interfaces
    // abstract member SubjectId: SubjectId
    abstract member SubjectCreatedOn: DateTimeOffset

// Interface that requires all implementing types to be Unions
// Needs to be implemented as a compiler extension
// Would be nice to have default interface methods (supported in C#, but F# refuses to support) to get case info
type Union =
    interface end

// Interface that requires all implementing types to be Records
// Needs to be implemented as a compiler extension
// Would be nice to have default interface methods (supported in C#, but F# refuses to support) to get field info
type Record =
    interface end

type SubjectProjection<'SubjectId when 'SubjectId :> SubjectId and 'SubjectId : comparison> =
    abstract member SubjectId: 'SubjectId

type Subject<'SubjectId when 'SubjectId :> SubjectId and 'SubjectId : comparison> =
    inherit Subject
    inherit SubjectProjection<'SubjectId>
    inherit CodecLib.IInterfaceCodec<Subject<'SubjectId>>


// will transition in response to actions
type LifeAction =
    inherit CodecLib.IInterfaceCodec<LifeAction>
    inherit Union

// and can produce errors
type OpError =
    inherit CodecLib.IInterfaceCodec<OpError>

// and zero or more events ...
type LifeEvent =
    inherit IComparable
    inherit CodecLib.IInterfaceCodec<LifeEvent>

// and one or more constructors
type Constructor =
    inherit CodecLib.IInterfaceCodec<Constructor>
    inherit Union

// which decompose to primitives (for now to facilitate storage ... in the future this might get auto-generated)
type IndexedPrimitiveNumber<'OpError when 'OpError :> OpError> =
| IndexedNumber       of int64
| UniqueIndexedNumber of int64 * ErrorToRaiseOnDuplicate: 'OpError
with
    member this.Value =
        match this with
        | IndexedNumber i -> i
        | UniqueIndexedNumber (i, _) -> i

type IndexedPrimitiveString<'OpError when 'OpError :> OpError> =
| IndexedString        of string
| UniqueIndexedString  of string * ErrorToRaiseOnDuplicate: 'OpError
with
    member this.Value =
        match this with
        | IndexedString i -> i
        | UniqueIndexedString (i, _) -> i

type IndexedPrimitiveSearchableText =
| IndexedPrimitiveSearchableText of string
with
    member this.Value =
        let (IndexedPrimitiveSearchableText value) = this in value

type IndexedPrimitiveGeography =
| IndexedPrimitiveGeography of GeographyIndexValue
with
    member this.Value =
        let (IndexedPrimitiveGeography value) = this in value

[<CodecLib.SkipCodecAutoGenerate>]
type SubjectNumericIndex<'OpError when 'OpError :> OpError> =
    inherit Union
    abstract member Primitive: IndexedPrimitiveNumber<'OpError>

[<CodecLib.SkipCodecAutoGenerate>]
type SubjectStringIndex<'OpError when 'OpError :> OpError> =
    inherit Union
    abstract member Primitive: IndexedPrimitiveString<'OpError>

[<CodecLib.SkipCodecAutoGenerate>]
type SubjectSearchIndex =
    inherit Union
    abstract member Primitive: IndexedPrimitiveSearchableText

[<CodecLib.SkipCodecAutoGenerate>]
type SubjectGeographyIndex =
    inherit Union
    abstract member Primitive: IndexedPrimitiveGeography

type NoNumericIndex<'OpError when 'OpError :> OpError> = private NoIndex of unit
with
    interface SubjectNumericIndex<'OpError> with
        member this.Primitive = shouldNotReachHereBecause "This is a no-index type, and calls should be preempted by the framework"
type NoStringIndex<'OpError when 'OpError :> OpError> = private NoIndex of unit
with
    interface SubjectStringIndex<'OpError> with
        member this.Primitive = shouldNotReachHereBecause "This is a no-index type, and calls should be preempted by the framework"

type NoSearchIndex = private NoIndex of unit
with
    interface SubjectSearchIndex with
        member this.Primitive = shouldNotReachHereBecause "This is a no-index type, and calls should be preempted by the framework"

type NoGeographyIndex = private NoIndex of unit
with
    interface SubjectGeographyIndex with
        member this.Primitive = shouldNotReachHereBecause "This is a no-index type, and calls should be preempted by the framework"


type IndexKey =
| Numeric   of KeyName: string
| String    of KeyName: string
| Search    of KeyName: string
| Geography of KeyName: string
with
    member this.KeyName =
        match this with
        | Numeric keyName
        | String  keyName
        | Search  keyName
        | Geography keyName ->
            keyName

type SubjectIndex<'OpError when 'OpError :> OpError> =
    abstract member MaybeKeyAndPrimitiveNumber:         Option<string * IndexedPrimitiveNumber<'OpError>>
    abstract member MaybeKeyAndPrimitiveString:         Option<string * IndexedPrimitiveString<'OpError>>
    abstract member MaybeKeyAndPrimitiveSearchableText: Option<string * IndexedPrimitiveSearchableText>
    abstract member MaybeKeyAndPrimitiveGeography:      Option<string * IndexedPrimitiveGeography>
#if !FABLE_COMPILER
    static abstract member IndexKeys : Set<IndexKey>
     // only required by backend
    static abstract member SubjectNumericIndexType:     Type
    static abstract member SubjectStringIndexType:      Type
    static abstract member SubjectSearchIndexType:      Type
    static abstract member SubjectGeographyIndexType:   Type
#endif

[<RequireQualifiedAccess>]
type SubjectAuditOperation<'LifeAction, 'Constructor
                                when 'LifeAction :> LifeAction
                                and  'Constructor :> Constructor> =
| Act       of 'LifeAction
| Construct of 'Constructor

[<RequireQualifiedAccess>]
type GetSnapshotOfVersion =
| Latest
| Specific of uint64

// Using tuple rather than a record to force stability of relational comparison without the need for custom comparison/equality implementations.
// We store ticks rather than DateTimeOffset because otherwise accuracy is lost when round-tripping with the client, since JS dates are only accurate
// to nearest millisecond. Since the client doesn't actually need it as a date/time, we store as int to retain as much accuracy as possible.
type ComparableVersion = (* AsOfTicks *) int64 * (* Version *) uint64

[<RequireQualifiedAccess>]
module ComparableVersion =
    let MinValue = (Int64.MinValue, UInt64.MinValue)

    let MaxValue = (Int64.MaxValue, UInt64.MaxValue)

type VersionedSubject<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and 'SubjectId : comparison> = {
    Subject:   'Subject
    AsOf:      DateTimeOffset
    Version:   uint64
}

[<RequireQualifiedAccess>]
module VersionedSubject =
    let subject versionedSubject =
        versionedSubject.Subject

type TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId
                       when 'Subject :> Subject<'SubjectId>
                       and  'LifeAction :> LifeAction
                       and  'Constructor :> Constructor
                       and  'SubjectId :> SubjectId
                       and 'SubjectId : comparison> = {
    Subject:   'Subject
    AsOf:      DateTimeOffset
    Version:   uint64
    Operation: Result<SubjectAuditOperation<'LifeAction, 'Constructor>, string>
    By:        string
}

type SubjectAuditData<'LifeAction, 'Constructor
                       when 'LifeAction :> LifeAction
                       and  'Constructor :> Constructor> = {
    AsOf:      DateTimeOffset
    Version:   uint64
    Operation: Result<SubjectAuditOperation<'LifeAction, 'Constructor>, string>
    By:        string
}

type UntypedSubjectAuditData = {
    AsOf:         DateTimeOffset
    Version:      uint64
    OperationStr: string
    By:           string
}

let getId (subject: Subject<'SubjectId>) : 'SubjectId =
    subject.SubjectId

let getIdString id =
    (id :> SubjectId).IdString

// To be implemented by 'LifeAction and 'Constructor implementations that contain
// large or sensitive data that should be redacted in the audit log and AppInsights telemetry.
type IRedactable =
        abstract member Redact: unit -> obj

let indexedStringf (format: Printf.StringFormat<'T, IndexedPrimitiveString<'OpError>>) =
    Printf.ksprintf (fun str -> IndexedString str) format

let uniqueIndexedStringf (opError: 'OpError) (format: Printf.StringFormat<'T, IndexedPrimitiveString<'OpError>>) =
    Printf.ksprintf (fun str -> UniqueIndexedString(str, opError)) format

type GetMaybeConstruct<'SubjectId, 'Constructor
        when 'SubjectId :> SubjectId
        and  'Constructor :> Constructor> = {
    Id:          'SubjectId
    Constructor: 'Constructor
}

type ActMaybeConstruct<'LifeAction, 'Constructor
                        when 'LifeAction  :> LifeAction
                        and  'Constructor :> Constructor> = {
    Action:      'LifeAction
    Constructor: 'Constructor
}

type ActAndWaitOnLifeEvent<'LifeAction, 'LifeEvent
                            when 'LifeAction :> LifeAction
                            and  'LifeEvent  :> LifeEvent> = {
    Action:    'LifeAction
    LifeEvent: 'LifeEvent
}

type ActMaybeConstructAndWaitOnLifeEvent<'LifeAction, 'Constructor, 'LifeEvent
                                          when 'LifeAction :> LifeAction
                                          and  'Constructor :> Constructor
                                          and  'LifeEvent  :> LifeEvent> = {
    Action:      'LifeAction
    Constructor: 'Constructor
    LifeEvent:   'LifeEvent
}

type ConstructAndWaitOnLifeEvent<'Constructor, 'LifeEvent
                                  when 'Constructor :> Constructor
                                  and  'LifeEvent  :> LifeEvent> = {
    Constructor: 'Constructor
    LifeEvent:   'LifeEvent
}

type ActOrConstructAndWaitOnLifeEventResult<'Subject, 'SubjectId, 'LifeEvent
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId
        and 'SubjectId : comparison
        and  'LifeEvent :> LifeEvent> =
| LifeEventTriggered      of FinalValueAfterEvent: VersionedSubject<'Subject, 'SubjectId> * TriggeredEvent: 'LifeEvent
| WaitOnLifeEventTimedOut of InitialValueAfterActionOrConstruction: VersionedSubject<'Subject, 'SubjectId>
with
    member this.VersionedSubject: VersionedSubject<'Subject, 'SubjectId> =
        match this with
        | LifeEventTriggered (versionedSubject, _)
        | WaitOnLifeEventTimedOut versionedSubject -> versionedSubject

type ListWithTotalCount<'T> = {
    Data: list<'T>
    TotalCount: uint64
}

[<RequireQualifiedAccess>]
module ListWithTotalCount =
    let map (fn: 'T -> 'U) (values: ListWithTotalCount<'T>) : ListWithTotalCount<'U> =
        {
            Data       = List.map fn values.Data
            TotalCount = values.TotalCount
        }

// NOTE please let me put this here for now while I'm figuring out this
// pattern of mine for handling spinners.
type HumanActionId = HumanActionId of Guid
with
    static member New : HumanActionId =
        HumanActionId (Guid.NewGuid())

type LocalSubjectPKeyReference = {
    LifeCycleName: string
    SubjectIdStr : string
}

type LifeCycleKey =
| LifeCycleKey of LifeCycleName: string * EcosystemName: string
// TODO: remove obsolete local key only when :
// - all previously deployed ecosystems upgraded
// - OurSubscriptions re-encoded (can take long time!)
// - persisted side effects pumped (should quick)
// TODO: can we do better? e.g. inject default ecosystem name on host startup so old stuff can be converted to new format upon decoding
| OBSOLETE_LocalLifeCycleKey of LifeCycleName: string
with
    member this.LocalLifeCycleName =
        match this with
        | LifeCycleKey (name, _) ->
            name
        | OBSOLETE_LocalLifeCycleKey _ ->
            failwith "unexpected obsolete local LC key"

    member this.EcosystemName =
        match this with
        | LifeCycleKey (_, ecosystemName) ->
            ecosystemName
        | OBSOLETE_LocalLifeCycleKey _ ->
            failwith "unexpected obsolete local LC key"

[<RequireQualifiedAccess>]
type SubjectPKeyReference = {
    LifeCycleKey: LifeCycleKey
    SubjectIdStr: string
}

[<RequireQualifiedAccess>]
type SubjectReference = {
    LifeCycleKey: LifeCycleKey
    SubjectId:    SubjectId
}
with
    member this.SubjectPKeyReference : SubjectPKeyReference =
        { SubjectIdStr = this.SubjectId.IdString; LifeCycleKey = this.LifeCycleKey }


type TimeSeriesKey =
| TimeSeriesKey of TimeSeriesName: string * TimeSeriesEcosystemName: string
with
    member this.LocalTimeSeriesName =
        match this with
        | TimeSeriesKey (name, _) ->
            name
    member this.EcosystemName =
        match this with
        | TimeSeriesKey (_, ecosystemName) ->
            ecosystemName

type TimeSeriesIndex<'TimeSeriesIndex when 'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> =
    inherit Union
    abstract member PrimitiveValue: string
#if !FABLE_COMPILER
    static abstract member TryParse: primitiveKey: string -> primitiveValue: string -> Option<'TimeSeriesIndex>
#endif

// it was considered to tag BlobId<> with generic 'Subject type parameter but it deemed impractical:
// - if BlobId passed to downstream life cycles, they need to know the type of the owner Subject which creates a nasty type cycle
// - because of how transition CE works, blobId of correct type would be impossible to generate in a nested transition of different type
#if FABLE_COMPILER
type BlobId = {
#else
type BlobId = internal {
#endif
    Id_:       Guid
    // revision incremented with every append to blob
    Revision_: UInt32
    // owner is the subject that created the blob, only this subject can delete or append to it
    // it helps to think of blob as just a regular subject property, only large & with manual lifetime management
    // TODO: implement blob garbage collector to avoid manual memory management. Reflection or codecs based
    Owner_:    LocalSubjectPKeyReference
}
with
    member this.Id       = this.Id_
    member this.Revision = this.Revision_
    member this.Owner    = this.Owner_
    member this.Url      = sprintf "/%s/blob/%s/%s" this.Owner_.LifeCycleName this.Owner_.SubjectIdStr (this.Id_.ToTinyUuid())

type BlobData = {
    Data:     byte[]
    MimeType: Option<MimeType>
}

// TODO: could make this private were it not for Fable: https://github.com/fable-compiler/Fable/issues/2069
[<RequireQualifiedAccess>]
module
#if !FABLE_COMPILER
    internal
#endif
    UnionCase =
    open System.Reflection
    open Microsoft.FSharp.Reflection

    // This is used when callers provide a union case value directly, either a union case that has no fields or a union case that
    // has fields that have already been "filled in" by the caller.
    type UnionCaseWithValue = {
        Type: Type
        Value: obj
    }

    // This is used when callers provide a union case that has fields, but those fields haven't been "filled in" by the caller.
    // i.e. they have provided a function from fields to union case value. In this scenario, we construct a default value for the
    // fields so that the union case itself can be constructed via the function. However, we do not wish to memoize based on that
    // default value, since it won't necessarily be a valid F# object instance and may trigger an exception when its GetHashCode
    // member is invoked (which is required for memoization).
    //
    // Thus, this custom record ensures only the union case type is used in hashing and equality checks, since the default value
    // is irrelevant to the memoization.
    //
    // Note that Fable doesn't do memoization and cannot access Type.Equals, so we disable accordingly.
#if !FABLE_COMPILER
    [<CustomEquality>]
    [<NoComparison>]
#endif
    type UnionCaseWithDefaultValue = {
        Type: Type
        DefaultValue: obj
    }
#if !FABLE_COMPILER
    with
        override this.GetHashCode() = this.Type.GetHashCode()
        override this.Equals(other) =
            match other with
            | :? UnionCaseWithDefaultValue as typedOther -> this.Type.Equals(typedOther.Type)
            | _ -> false
#endif


    let getDefault (ty: Type) : obj =
#if !FABLE_COMPILER
        let getSingleTypeDefault (ty: Type) =
            if ty.IsValueType then
                Activator.CreateInstance ty
            else
#else
        let getSingleTypeDefault (_ty: Type) =
#endif
                null

        let bindingFlags = BindingFlags.Public ||| BindingFlags.NonPublic

        match ty with
        | ty when FSharpType.IsTuple ty ->
            let fields = ty.GenericTypeArguments |> Array.map getSingleTypeDefault
            FSharpValue.MakeTuple(fields, ty)
        | ty when FSharpType.IsUnion(ty, bindingFlags) ->
            let unionCaseInfos = FSharpType.GetUnionCases(ty, bindingFlags)
            let firstUnionCaseInfo = unionCaseInfos |> Array.head
            let fieldValues =
                firstUnionCaseInfo.GetFields()
                |> Array.map (fun pi -> pi.PropertyType)
                |> Array.map getSingleTypeDefault
            FSharpValue.MakeUnion(firstUnionCaseInfo, fieldValues, bindingFlags)
        | ty when FSharpType.IsRecord(ty, bindingFlags) ->
            let fieldValues =
                FSharpType.GetRecordFields(ty, bindingFlags)
                |> Array.map (fun pi -> pi.PropertyType)
                |> Array.map getSingleTypeDefault
            FSharpValue.MakeRecord(ty, fieldValues, bindingFlags)
        | _ ->
            getSingleTypeDefault ty

    let getAllCaseNames (ty: Type) : list<string> =
        FSharpType.GetUnionCases(ty, true)
        |> Seq.map (fun uc -> uc.Name)
        |> Seq.toList

    let getTagAndName (unionCaseWithValue: UnionCaseWithValue) : int * string =
        let uc = (FSharpValue.GetUnionFields (unionCaseWithValue.Value, unionCaseWithValue.Type) |> fst)
        (uc.Tag, uc.Name)

    let getTagAndNameForDefaultValue (unionCaseWithDefaultValue: UnionCaseWithDefaultValue) : int * string =
        let uc = (FSharpValue.GetUnionFields (unionCaseWithDefaultValue.DefaultValue, unionCaseWithDefaultValue.Type) |> fst)
        (uc.Tag, uc.Name)

#if FABLE_COMPILER
    let memoizeN = id
#else
    open FSharpPlus
#endif
    let getDefaultMemoized : (Type -> obj) = memoizeN getDefault
    let getTagAndNameMemoized : (UnionCaseWithValue -> int * string) = memoizeN getTagAndName
    let getTagAndNameForDefaultValueMemoized : (UnionCaseWithDefaultValue -> int * string) = memoizeN getTagAndNameForDefaultValue
    let getAllCaseNamesMemoized : (Type -> list<string>) = memoizeN getAllCaseNames

type UnionCase<'T when 'T :> Union> =
#if !FABLE_COMPILER
    private
#endif
        UnionCase_ of _TagNumber: int * _CaseName: string with

    member this.TagNumber : int =
        let (UnionCase_ (x, _)) = this in x

    member this.CaseName : string =
        let (UnionCase_ (_, x)) = this in x

#if FABLE_COMPILER
    static member inline ofCase (unionCase : 'T) : UnionCase<'T> =
#else
    static member        ofCase (unionCase : 'T) : UnionCase<'T> =
#endif
        let unionCaseWithValue: UnionCase.UnionCaseWithValue = {
            Type = typeof<'T>
            Value = unionCase
        }
        UnionCase_ (UnionCase.getTagAndNameMemoized unionCaseWithValue)

#if FABLE_COMPILER
    static member inline ofCase (unionCase : 'TupleOrSingleValue -> 'T) : UnionCase<'T> =
#else
    static member        ofCase (unionCase : 'TupleOrSingleValue -> 'T) : UnionCase<'T> =
#endif
        let defaultFieldValue = UnionCase.getDefaultMemoized typeof<'TupleOrSingleValue> |> uncheckedUnbox
        let unionCaseValue = unionCase defaultFieldValue
        let unionCaseWithDefaultValue: UnionCase.UnionCaseWithDefaultValue = {
            Type = unionCaseValue.GetType()
            DefaultValue = unionCaseValue
        }
        let tagAndName = UnionCase.getTagAndNameForDefaultValueMemoized unionCaseWithDefaultValue
        UnionCase_ tagAndName

#if FABLE_COMPILER
    static member inline nameOfCase (unionCase: 'T) : string =
#else
    static member nameOfCase (unionCase: 'T) : string =
#endif
        (UnionCase.ofCase unionCase).CaseName

#if !FABLE_COMPILER
    static member allCaseNames : list<string> =
        UnionCase.getAllCaseNamesMemoized typeof<'T>
#endif

// Used to query indices
type IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
        and  'OpError             :> OpError
        and  'SubjectSearchIndex  :> SubjectSearchIndex
        and  'SubjectGeographyIndex :> SubjectGeographyIndex> =
// Numeric operators
| EqualToNumeric              of 'SubjectNumericIndex
| GreaterThanNumeric          of 'SubjectNumericIndex
| GreaterThanOrEqualToNumeric of 'SubjectNumericIndex
| LessThanNumeric             of 'SubjectNumericIndex
| LessThanOrEqualToNumeric    of 'SubjectNumericIndex

// String operators
| EqualToString               of 'SubjectStringIndex
| GreaterThanString           of 'SubjectStringIndex
| GreaterThanOrEqualToString  of 'SubjectStringIndex
| LessThanString              of 'SubjectStringIndex
| LessThanOrEqualToString     of 'SubjectStringIndex
| StartsWith                  of UnionCase<'SubjectStringIndex> * StartsWith: string

// Full text search
| Matches                     of UnionCase<'SubjectSearchIndex> * Keywords: string
| MatchesExact                of UnionCase<'SubjectSearchIndex> * Keywords: string
| MatchesPrefix               of UnionCase<'SubjectSearchIndex> * StartsWith: string

// Geography
| IntersectsGeography         of UnionCase<'SubjectGeographyIndex> * IntersectsWhat: GeographyIndexValue

// Logical operators
| And                         of IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError> * IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
| Or                          of IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError> * IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
| Diff                        of IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError> * IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>

module IndexPredicate =
    let andList (indices: NonemptyList<IndexPredicate<_, _, _, _, _>>) =
        List.fold (fun acc next -> And (acc, next)) indices.Head indices.Tail

    let orList (indices: NonemptyList<IndexPredicate<_, _, _, _, _>>) =
        List.fold (fun acc next -> Or (acc, next)) indices.Head indices.Tail


[<RequireQualifiedAccess>]
type UntypedPredicate =
| EqualToNumeric              of Key: string * Value: int64
| GreaterThanNumeric          of Key: string * Value: int64
| GreaterThanOrEqualToNumeric of Key: string * Value: int64
| LessThanNumeric             of Key: string * Value: int64
| LessThanOrEqualToNumeric    of Key: string * Value: int64

// String operators
| EqualToString               of Key: string * Value: string
| GreaterThanString           of Key: string * Value: string
| GreaterThanOrEqualToString  of Key: string * Value: string
| LessThanString              of Key: string * Value: string
| LessThanOrEqualToString     of Key: string * Value: string
| StartsWith                  of Key: string * StartsWith: string

// Full text
| Matches                     of Key: string * Keywords: string
| MatchesExact                of Key: string * Keywords: string
| MatchesPrefix               of Key: string * StartsWith: string

// Geography
| IntersectsGeography         of Key: string * GeographyIndexValue

// Logical operators
| And                         of UntypedPredicate * UntypedPredicate
| Or                          of UntypedPredicate * UntypedPredicate
| Diff                        of UntypedPredicate * UntypedPredicate

type ResultPage = {
    Size:   uint16
    Offset: uint64
}

[<RequireQualifiedAccess>]
type ResultSetOptions<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'OpError             :> OpError> = {
    Page:    ResultPage
    OrderBy: OrderBy<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>
}

and [<RequireQualifiedAccess>] OrderBy<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
                                when 'SubjectNumericIndex  :> SubjectNumericIndex<'OpError>
                                and  'SubjectStringIndex   :> SubjectStringIndex<'OpError>
                                and  'OpError              :> OpError> =

// FIXME - in long term we should remodel whole search ranking e.g. to allow ordering by multiple
// ranks with different weights. For now let's keep this awful yet accurate name.
| FastestOrSingleSearchScoreIfAvailable
| SubjectId         of OrderDirection
| Random
| NumericIndexEntry of UnionCase<'SubjectNumericIndex> * OrderDirection
| StringIndexEntry  of UnionCase<'SubjectStringIndex> * OrderDirection

and [<RequireQualifiedAccess>] OrderDirection =
| Ascending
| Descending

module ResultSetOptions =
    let atMostOne: ResultSetOptions<_, _, _> = {
        Page = {
            Size   = 1us
            Offset = 0UL
        }
        OrderBy = OrderBy<_, _, _>.FastestOrSingleSearchScoreIfAvailable
    }

    let dangerousAll (orderBy: OrderBy<_, _, _>): ResultSetOptions<_, _, _> = {
        Page = {
            Size   = System.UInt16.MaxValue
            Offset = 0UL
        }
        OrderBy = orderBy
    }

[<RequireQualifiedAccess>]
type UntypedOrderBy =
| FastestOrSingleSearchScoreIfAvailable
| SubjectId  of OrderDirection
| Random
| NumericIndexEntry of Key: string * OrderDirection
| StringIndexEntry  of Key: string * OrderDirection

[<RequireQualifiedAccess>]
type UntypedResultSetOptions = {
    Page:    ResultPage
    OrderBy: UntypedOrderBy
}

type ResultSetOptions<'SubjectIndex> =
#if !FABLE_COMPILER
    private
#endif
        ResultSetOptions_ of UntypedResultSetOptions
with
    member this.Options =
        let (ResultSetOptions_ (resultSetOptions)) = this in resultSetOptions

    static member OrderByFastestWithPage (page: ResultPage) =
        let options : UntypedResultSetOptions = {
            Page    = page
            OrderBy = UntypedOrderBy.FastestOrSingleSearchScoreIfAvailable
        }
        ResultSetOptions_ options

type IndexQuery<'SubjectIndex> =
#if !FABLE_COMPILER
    private
#endif
        IndexQuery of UntypedPredicate * UntypedResultSetOptions
with
    member this.Predicate =
        let (IndexQuery (predicate, _)) = this in predicate

    member this.ResultSetOptions =
        let (IndexQuery (_, resultSetOptions)) = this in resultSetOptions

type PreparedIndexPredicate<'SubjectIndex> =
#if !FABLE_COMPILER
    private
#endif
        PreparedIndexPredicate of UntypedPredicate
with
    member this.Predicate =
        let (PreparedIndexPredicate predicate) = this in predicate

    member this.PrepareQuery options =
        let (PreparedIndexPredicate predicate) = this
        IndexQuery(predicate, options)

#if FABLE_COMPILER
let rec (* want private but need for inline *) toUntypedPredicate (caseNameNumeric) (caseNameString) typedPredicate =
#else
let rec private toUntypedPredicate caseNameNumeric caseNameString typedPredicate =
#endif
    match typedPredicate with
    | EqualToNumeric              numIndex -> UntypedPredicate.EqualToNumeric              (caseNameNumeric numIndex, numIndex.Primitive.Value)
    | GreaterThanNumeric          numIndex -> UntypedPredicate.GreaterThanNumeric          (caseNameNumeric numIndex, numIndex.Primitive.Value)
    | GreaterThanOrEqualToNumeric numIndex -> UntypedPredicate.GreaterThanOrEqualToNumeric (caseNameNumeric numIndex, numIndex.Primitive.Value)
    | LessThanNumeric             numIndex -> UntypedPredicate.LessThanNumeric             (caseNameNumeric numIndex, numIndex.Primitive.Value)
    | LessThanOrEqualToNumeric    numIndex -> UntypedPredicate.LessThanOrEqualToNumeric    (caseNameNumeric numIndex, numIndex.Primitive.Value)

    // String operators
    | EqualToString               strIndex -> UntypedPredicate.EqualToString               (caseNameString strIndex, strIndex.Primitive.Value)
    | GreaterThanString           strIndex -> UntypedPredicate.GreaterThanString           (caseNameString strIndex, strIndex.Primitive.Value)
    | GreaterThanOrEqualToString  strIndex -> UntypedPredicate.GreaterThanOrEqualToString  (caseNameString strIndex, strIndex.Primitive.Value)
    | LessThanString              strIndex -> UntypedPredicate.LessThanString              (caseNameString strIndex, strIndex.Primitive.Value)
    | LessThanOrEqualToString     strIndex -> UntypedPredicate.LessThanOrEqualToString     (caseNameString strIndex, strIndex.Primitive.Value)
    | StartsWith(case, str)                -> UntypedPredicate.StartsWith(case.CaseName, str)

    // Full text search
    //   Fail fast on empty or whitespace full text string - or it will crash in sql.
    //   Can we do bettter? NonemptyString still can have whitespace so won't work.
    | Matches(_, keywords) when String.IsNullOrWhiteSpace keywords
                                           -> failwith "Search text can't be empty or whitespace"
    | MatchesExact(_, keywords) when String.IsNullOrWhiteSpace keywords
                                           -> failwith "Search text can't be empty or whitespace"
    | MatchesPrefix(_, startsWith) when String.IsNullOrWhiteSpace startsWith
                                           -> failwith "Search text can't be empty or whitespace"
    | Matches(case, keywords)              -> UntypedPredicate.Matches(case.CaseName, keywords)
    | MatchesExact(case, keywords)         -> UntypedPredicate.MatchesExact(case.CaseName, keywords)
    | MatchesPrefix(case, startsWith)      -> UntypedPredicate.MatchesPrefix(case.CaseName, startsWith)

    | IntersectsGeography (case, geoValue) -> UntypedPredicate.IntersectsGeography (case.CaseName, geoValue)

    // Logical operators
    | And  (one, two)                      -> UntypedPredicate.And  (toUntypedPredicate caseNameNumeric caseNameString one, toUntypedPredicate caseNameNumeric caseNameString two)
    | Or   (one, two)                      -> UntypedPredicate.Or   (toUntypedPredicate caseNameNumeric caseNameString one, toUntypedPredicate caseNameNumeric caseNameString two)
    | Diff (one, two)                      -> UntypedPredicate.Diff (toUntypedPredicate caseNameNumeric caseNameString one, toUntypedPredicate caseNameNumeric caseNameString two)

#if FABLE_COMPILER
let rec (* want private but need for inline *) toUntypedResultSetOptions (resultSetOptions: ResultSetOptions<_, _, _>) =
#else
let rec private toUntypedResultSetOptions (resultSetOptions: ResultSetOptions<_, _, _>) =
#endif
    {
        UntypedResultSetOptions.Page    = resultSetOptions.Page
        UntypedResultSetOptions.OrderBy =
            match resultSetOptions.OrderBy with
            | OrderBy.FastestOrSingleSearchScoreIfAvailable -> UntypedOrderBy.FastestOrSingleSearchScoreIfAvailable
            | OrderBy.SubjectId direction                   -> UntypedOrderBy.SubjectId direction
            | OrderBy.Random                                -> UntypedOrderBy.Random
            | OrderBy.NumericIndexEntry (case, direction)   -> UntypedOrderBy.NumericIndexEntry(case.CaseName, direction)
            | OrderBy.StringIndexEntry (case, direction)    -> UntypedOrderBy.StringIndexEntry(case.CaseName, direction)
    }

// Has no abstract members but needs to be abstract to enforce inheritance instead of type aliases
// Inheritance is required to make sure that full name of concrete subject index type remains the same after breaking changes to base type.
[<AbstractClass; CodecLib.SkipCodecAutoGenerate>]
type SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
       when 'OpError             :> OpError
       and  'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
       and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
       and  'SubjectSearchIndex  :> SubjectSearchIndex
       and  'SubjectGeographyIndex :> SubjectGeographyIndex
       and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
#if !FABLE_COMPILER // instantiation required only in backend
       and  'SubjectIndex : (new: unit -> 'SubjectIndex)
#endif
       > () =

    // I did try to avoid the private mutable but nothing worked:
    // - static abstract member New to implement in inheritor? Can't do that in F#
    // - constructor constraint on 'SubjectIndex parameter? Can constraint only to default new() constructor
    let mutable value: Choice<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex> = Unchecked.defaultof<_>

    #if !FABLE_COMPILER

    member private _.SetValue valueToSet =
        value <- valueToSet

    static member New (value: Choice<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex>) : 'SubjectIndex =
        let idx = new 'SubjectIndex()
        idx.SetValue value
        idx

    #endif

    #if FABLE_COMPILER
    static member inline PreparePredicate
    #else
    static member PreparePredicate
    #endif
        (predicate: IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>)
        : PreparedIndexPredicate<'SubjectIndex> =
            PreparedIndexPredicate(toUntypedPredicate UnionCase.nameOfCase UnionCase.nameOfCase predicate)

    #if FABLE_COMPILER
    static member inline PrepareQuery
    #else
    static member PrepareQuery
    #endif
        (resultSetOptions: ResultSetOptions<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>)
        (predicate: IndexPredicate<'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>)
        : IndexQuery<'SubjectIndex> =
            IndexQuery((toUntypedPredicate UnionCase.nameOfCase UnionCase.nameOfCase predicate), (toUntypedResultSetOptions resultSetOptions))

    static member PrepareQueryFromPreparedPredicate
        (predicate: PreparedIndexPredicate<'SubjectIndex>)
        (resultSetOptions: ResultSetOptions<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>)
        : IndexQuery<'SubjectIndex> =
            IndexQuery(predicate.Predicate, (toUntypedResultSetOptions resultSetOptions))

    static member WithResultSetOptions
        (resultSetOptions: ResultSetOptions<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>)
        : ResultSetOptions<'SubjectIndex> =
            ResultSetOptions_((toUntypedResultSetOptions resultSetOptions))

    interface SubjectIndex<'OpError> with

#if !FABLE_COMPILER
        static member IndexKeys : Set<IndexKey> =
            seq {
                yield!
                    UnionCase<'SubjectNumericIndex>.allCaseNames
                    |> Seq.map Numeric

                yield!
                    UnionCase<'SubjectStringIndex>.allCaseNames
                    |> Seq.map String

                yield!
                    UnionCase<'SubjectSearchIndex>.allCaseNames
                    |> Seq.map Search

                yield!
                    UnionCase<'SubjectGeographyIndex>.allCaseNames
                    |> Seq.map Geography
            }
            |> Set.ofSeq

        static member SubjectNumericIndexType   = typeof<'SubjectNumericIndex>
        static member SubjectStringIndexType    = typeof<'SubjectStringIndex>
        static member SubjectSearchIndexType    = typeof<'SubjectSearchIndex>
        static member SubjectGeographyIndexType = typeof<'SubjectGeographyIndex>
#endif

        member _.MaybeKeyAndPrimitiveNumber =
            match value with
            | Choice1Of4 (numIndex: 'SubjectNumericIndex) ->
                let caseName: string =
                    #if FABLE_COMPILER
                    failwith "Unsupported in Fable"
                    #else
                    (UnionCase.ofCase numIndex).CaseName
                    #endif
                if (caseName.Length > 80) then
                    failwithf "Index Key cannot be longer than 80 chars: %s" caseName
                (caseName, numIndex.Primitive) |> Some
            | _ -> None

        member _.MaybeKeyAndPrimitiveString =
            match value with
            | Choice2Of4 (strIndex: 'SubjectStringIndex) ->
                let caseName: string =
                    #if FABLE_COMPILER
                    failwith "Unsupported in Fable"
                    #else
                    (UnionCase.ofCase strIndex).CaseName
                    #endif
                let primitive = strIndex.Primitive
                if (caseName.Length > 80) then
                    failwithf "Index Key cannot be longer than 80 chars: %s" caseName
                if (isNull primitive.Value) then
                    failwith "Index ValueStr cannot be null"
                if (primitive.Value.Length > 500) then
                    failwithf "Index ValueStr cannot be longer than 500 chars: %s" primitive.Value
                (caseName, primitive) |> Some
            | _ -> None

        member _.MaybeKeyAndPrimitiveSearchableText =
            match value with
            | Choice3Of4 (searchIndex: 'SubjectSearchIndex) ->
                let caseName: string =
                    #if FABLE_COMPILER
                    failwith "Unsupported in Fable"
                    #else
                    (UnionCase.ofCase searchIndex).CaseName
                    #endif
                if (caseName.Length > 80) then
                    failwithf "Index Key cannot be longer than 80 chars: %s" caseName
                (caseName, searchIndex.Primitive) |> Some
            | _ -> None

        member _.MaybeKeyAndPrimitiveGeography =
            match value with
            | Choice4Of4 (geographyIndex: 'SubjectGeographyIndex) ->
                let caseName: string =
                    #if FABLE_COMPILER
                    failwith "Unsupported in Fable"
                    #else
                    (UnionCase.ofCase geographyIndex).CaseName
                    #endif
                if (caseName.Length > 80) then
                    failwithf "Index Key cannot be longer than 80 chars: %s" caseName
                (caseName, geographyIndex.Primitive) |> Some
            | _ -> None

let noIndices<'Subject, 'OpError, 'SubjectIndex when 'OpError :> OpError> (_subject: 'Subject) : seq<'SubjectIndex> = Seq.empty


// SubjectTransactionId is a well-known type

type SubjectTransactionId = SubjectTransactionId of Guid
with
    interface SubjectId with
        member this.IdString =
            let (SubjectTransactionId transactionId) = this
            transactionId.ToTinyUuid()


type TimeSeriesId<'TimeSeriesId when 'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>> =
    abstract member IdString: string
    // parameterized the TimeSeries interface with implementing 'TimeSeriesId so we can easily add static Codec member later (not needed so far)
    // static abstract member Codec<'Encoding when 'Encoding :> Fleece.IEncoding and 'Encoding : (new: unit -> 'Encoding)> : unit -> Fleece.Codec<'Encoding, 'TimeSeriesId>

type TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure
    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
    and 'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>> =
    abstract member Id: 'TimeSeriesId
    abstract member TimeIndex: DateTimeOffset
    abstract member Value: float<'UnitOfMeasure>
#if !FABLE_COMPILER
    static abstract member Codec<'Encoding when 'Encoding :> CodecLib.IEncoding and 'Encoding : (new: unit -> 'Encoding)> : unit -> CodecLib.Codec<'Encoding, 'TimeSeriesDataPoint>
#endif
type TimeIntervalEndpoint =
| IncludeEndpoint of DateTimeOffset
| ExcludeEndpoint of DateTimeOffset

type TimePart =
| Day
| Week
| Month
| Quarter
| Year
| Hour
| Minute
| Second
| Millisecond

type TimeBucket = {
    Part: TimePart
    Width: PositiveInteger
    Origin: Option<DateTimeOffset>
}

type TimeBucketValueAggregate =
| FirstValue
| LastValue
| MinimumValue
| MaximumValue
| AverageValue
| MedianValue
| PercentileValue of PositivePercentage
| SumValue

type TimeBucketPointAggregate =
| FirstPoint
| LastPoint

type TimeSeriesInterval<'TimeSeriesId, 'TimeSeriesIndex
    when 'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
    and 'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> = {
    Id: 'TimeSeriesId
    Index: Option<'TimeSeriesIndex>
    // why no Option<TimeIntervalEndpoint> for open range? to discourage wide ranges, if client really needs to it can specify extreme start/end time
    Start: TimeIntervalEndpoint
    End: TimeIntervalEndpoint
}

type TimeSeriesGroupInterval<'TimeSeriesId, 'TimeSeriesIndex
    when 'TimeSeriesId :> TimeSeriesId<'TimeSeriesId>
    and 'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>> = {
    Id: 'TimeSeriesId
    // why no Option<TimeIntervalEndpoint> for open range? to discourage wide ranges, if client really needs to it can specify extreme start/end time
    Start: TimeIntervalEndpoint
    End: TimeIntervalEndpoint
    GroupBy: UnionCase<'TimeSeriesIndex>
}

// Codecs & casts

#if !FABLE_COMPILER

open CodecLib

type SubjectAuditOperation<'LifeAction, 'Constructor
                                when 'LifeAction :> LifeAction
                                and  'Constructor :> Constructor>
with
    static member inline get_Codec () =
        function
        | Act _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "Act" (function (Act x) -> Some x | _ -> None)
                return Act payload
            }
        | Construct _ ->
            codec {
                let! payload = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'constructor> "Construct" (function (Construct x) -> Some x | _ -> None)
                return Construct payload
            }
        |> mergeUnionCases
        |> ofObjCodec

    static member CastUnsafe (op: SubjectAuditOperation<#LifeAction, #Constructor>) : SubjectAuditOperation<'LifeAction, 'Constructor> =
        match op with
        | Act x ->
            Act (x |> box :?> 'LifeAction)
        | Construct x ->
            Construct (x |> box :?> 'Constructor)


type GetSnapshotOfVersion
with
    static member get_Codec () =
        function
        | Latest ->
            codec {
                let! _ = reqWith Codecs.unit "Latest" (function Latest -> Some () | _ -> None)
                return Latest
            }
        | Specific _ ->
            codec {
                let! payload = reqWith Codecs.uint64 "Specific" (function (Specific x) -> Some x | _ -> None)
                return Specific payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type VersionedSubject<'Subject, 'SubjectId
        when 'Subject   :> Subject<'SubjectId>
        and  'SubjectId :> SubjectId>
with
    static member inline get_Codec () : Codec<'RawEncoding, VersionedSubject<Subject<'SubjectId>, 'SubjectId>> =
        codec {
            let! subject = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Subject<'SubjectId>> "Subject" (fun x -> Some x.Subject)
            and! asOf = reqWith Codecs.dateTimeOffset "AsOf" (fun x -> Some x.AsOf)
            and! version = reqWith Codecs.uint64 "Version" (fun x -> Some x.Version)
            return { Subject = subject; AsOf = asOf; Version = version }
        }
        |> ofObjCodec

    static member CastUnsafe (versionedSubject: VersionedSubject<#Subject<'SubjectId>, 'SubjectId>) : VersionedSubject<'Subject, 'SubjectId> =
        {
            Subject   = versionedSubject.Subject |> box :?> 'Subject
            AsOf      = versionedSubject.AsOf
            Version   = versionedSubject.Version
        }

type TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId
                       when 'Subject :> Subject<'SubjectId>
                       and  'LifeAction :> LifeAction
                       and  'Constructor :> Constructor>
with
    static member inline get_Codec () : Codec<_, TemporalSnapshot<Subject<'SubjectId>, 'lifeAction, 'constructor, 'SubjectId>> = ofObjCodec <| codec {
        let! asOf = reqWith Codecs.dateTimeOffset "AsOf" (fun x -> Some x.AsOf)
        and! subject = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, Subject<'SubjectId>> "Subject" (fun x -> Some x.Subject)
        and! operation = reqWith (Codecs.result (SubjectAuditOperation<_, _>.get_Codec ()) Codecs.string) "Operation" (fun x -> Some x.Operation)
        and! by = reqWith Codecs.string "By" (fun x -> Some x.By)
        and! version = reqWith Codecs.uint64 "Version" (fun x -> Some x.Version)
        return { AsOf = asOf; Subject = subject; Operation = operation; By = by; Version = version }}

    static member CastUnsafe (snapshot: TemporalSnapshot<#Subject<'SubjectId>, #LifeAction, #Constructor, 'SubjectId>) : TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId> =
        {
            AsOf      = snapshot.AsOf
            Subject   = snapshot.Subject |> box :?> 'Subject
            Operation = snapshot.Operation |> Result.map SubjectAuditOperation<'LifeAction, 'Constructor>.CastUnsafe
            By        = snapshot.By
            Version   = snapshot.Version
        }

type SubjectAuditData<'LifeAction, 'Constructor
                       when 'LifeAction :> LifeAction
                       and  'Constructor :> Constructor>
with
    static member inline get_Codec () : Codec<_, SubjectAuditData<'lifeAction,'constructor>> = ofObjCodec <| codec {
        let! asOf = reqWith Codecs.dateTimeOffset "AsOf" (fun x -> Some x.AsOf)
        and! operation = reqWith (Codecs.result (SubjectAuditOperation<'lifeAction, 'constructor>.get_Codec ()) Codecs.string) "Operation" (fun x -> Some x.Operation)
        and! by = reqWith Codecs.string "By" (fun x -> Some x.By)
        and! version = reqWith Codecs.uint64 "Version" (fun x -> Some x.Version)
        return { AsOf = asOf; Operation = operation; By = by; Version = version }}

    static member CastUnsafe (snapshot: SubjectAuditData<#LifeAction, #Constructor>) : SubjectAuditData<'LifeAction, 'Constructor> =
        {
            AsOf      = snapshot.AsOf
            Operation = snapshot.Operation |> Result.map SubjectAuditOperation<'LifeAction, 'Constructor>.CastUnsafe
            By        = snapshot.By
            Version   = snapshot.Version
        }

type LocalSubjectPKeyReference with
    static member get_Codec () = ofObjCodec <| codec {
        let! lifeCycleName = reqWith Codecs.string "LifeCycleName"(fun x -> Some x.LifeCycleName)
        and! subjectIdStr  = reqWith Codecs.string "SubjectIdStr" (fun x -> Some x.SubjectIdStr)
        return { LifeCycleName = lifeCycleName; SubjectIdStr = subjectIdStr } }

type LifeCycleKey with
    static member get_Codec () =
        function
        | LifeCycleKey _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "Key" (function | LifeCycleKey (x1, x2) -> Some (x1, x2) | _ -> None)
                return LifeCycleKey payload
            }
        | OBSOLETE_LocalLifeCycleKey _ ->
            codec {
                let! payload = reqWith Codecs.string "Local" (function | OBSOLETE_LocalLifeCycleKey x -> Some x | _ -> None)
                return OBSOLETE_LocalLifeCycleKey payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type SubjectPKeyReference with
    static member get_ObjDecoder_V1 () = decoder {
        let! lifeCycleName = reqDecodeWithCodec Codecs.string "LifeCycleName"
        and! subjectIdStr  = reqDecodeWithCodec Codecs.string "SubjectIdStr"
        return { LifeCycleKey = OBSOLETE_LocalLifeCycleKey lifeCycleName; SubjectIdStr = subjectIdStr } }

    static member get_ObjCodec_V2 () = codec {
        let! lifeCycleKey = reqWith codecFor<_, LifeCycleKey> "LifeCycleKey" (fun x -> Some x.LifeCycleKey)
        and! subjectIdStr  = reqWith Codecs.string "SubjectIdStr" (fun x -> Some x.SubjectIdStr)
        return { LifeCycleKey = lifeCycleKey; SubjectIdStr = subjectIdStr } }

    static member get_Codec () =
        SubjectPKeyReference.get_ObjCodec_V2 ()
        |> withDecoders [SubjectPKeyReference.get_ObjDecoder_V1 ()]
        |> ofObjCodec

type SubjectReference with
    static member get_ObjDecoder_V1 () = decoder {
      let! lifeCycleName = reqDecodeWithCodec Codecs.string "LifeCycleName"
      and! subjectId     = reqDecodeWithCodec defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, SubjectId> "SubjectId"
      return { LifeCycleKey = OBSOLETE_LocalLifeCycleKey lifeCycleName; SubjectId = subjectId } }

    static member get_ObjCodec_V2 () = codec {
        let! lifeCycleKey = reqWith codecFor<_, LifeCycleKey> "LifeCycleKey" (fun x -> Some x.LifeCycleKey)
        and! subjectIdStr  = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, SubjectId> "SubjectId" (fun x -> Some x.SubjectId)
        return { LifeCycleKey = lifeCycleKey; SubjectId = subjectIdStr } }

    static member get_Codec () =
        SubjectReference.get_ObjCodec_V2 ()
        |> withDecoders [SubjectReference.get_ObjDecoder_V1 ()]
        |> ofObjCodec

type TimeSeriesKey with
    static member get_Codec () =
        function
        | TimeSeriesKey _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "TSK" (function | TimeSeriesKey (x1, x2) -> Some (x1, x2))
                return TimeSeriesKey payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type BlobId
with
    static member get_Codec () = ofObjCodec <| codec {
        let! id       = reqWith Codecs.guid "Id" (fun x -> Some x.Id_)
        and! revision = reqWith Codecs.uint32 "Revision" (fun x -> Some x.Revision_)
        and! owner    = reqWith (LocalSubjectPKeyReference.get_Codec ()) "Owner" (fun x -> Some x.Owner_)
        return
            { Id_ = id
              Revision_   = revision
              Owner_      = owner } }

type BlobData
with
    static member get_Codec () = ofObjCodec <| codec {
        let! data     = reqWith Codecs.base64Bytes "Data" (fun x -> Some x.Data)
        and! mimeType = optWith codecFor<_, MimeType> "MimeType" (fun x -> x.MimeType)
        return
            { Data = data
              MimeType = mimeType } }

type OrderDirection
with
    static member get_Codec () =
        function
        | Ascending ->
            codec {
                let! _ = reqWith Codecs.unit "Ascending" (function Ascending -> Some () | _ -> None)
                return Ascending
            }
        | Descending ->
            codec {
                let! _ = reqWith Codecs.unit "Descending" (function Descending -> Some () | _ -> None)
                return Descending
            }
        |> mergeUnionCases
        |> ofObjCodec

type ResultPage
with
    static member get_Codec () = ofObjCodec <| codec {
        let! size = reqWith Codecs.uint16 "Size" (fun x -> Some x.Size)
        and! offset = reqWith Codecs.uint64 "Offset" (fun x -> Some x.Offset)
        return { Size = size; Offset = offset } }

type UntypedOrderBy
with
    static member get_Codec () =
        function
        | FastestOrSingleSearchScoreIfAvailable ->
            codec {
                let! _ = reqWith Codecs.unit "Fastest" (function FastestOrSingleSearchScoreIfAvailable -> Some () | _ -> None)
                return FastestOrSingleSearchScoreIfAvailable
            }
        | SubjectId _ ->
            codec {
                let! payload = reqWith (OrderDirection.get_Codec ()) "SubjectId" (function (SubjectId x) -> Some x  | _ -> None)
                return SubjectId payload
            }
        | Random ->
            codec {
                let! _ = reqWith Codecs.unit "Random" (function Random -> Some () | _ -> None)
                return Random
            }
        | NumericIndexEntry _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string (OrderDirection.get_Codec ())) "NumericIndexEntry" (function (NumericIndexEntry (x1, x2)) -> Some (x1, x2)  | _ -> None)
                return NumericIndexEntry payload
            }
        | StringIndexEntry _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string (OrderDirection.get_Codec ())) "StringIndexEntry" (function (StringIndexEntry (x1, x2)) -> Some (x1, x2)  | _ -> None)
                return StringIndexEntry payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type UntypedResultSetOptions
with
    static member get_Codec () = ofObjCodec <| codec {
        let! page = reqWith (ResultPage.get_Codec ()) "Page" (fun x -> Some x.Page)
        and! orderBy = reqWith (UntypedOrderBy.get_Codec ()) "OrderBy" (fun x -> Some x.OrderBy)
        return { Page = page; OrderBy = orderBy } }

type UntypedPredicate
with
    static member get_Codec () =
        function
        | EqualToNumeric _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.int64) "EqualToNumeric" (function (EqualToNumeric (x1, x2)) -> Some (x1, x2) | _ -> None)
                return EqualToNumeric payload
            }
        | GreaterThanNumeric _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.int64) "GreaterThanNumeric" (function (GreaterThanNumeric (x1, x2)) -> Some (x1, x2) | _ -> None)
                return GreaterThanNumeric payload
            }
        | GreaterThanOrEqualToNumeric _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.int64) "GreaterThanOrEqualToNumeric" (function (GreaterThanOrEqualToNumeric (x1, x2)) -> Some (x1, x2) | _ -> None)
                return GreaterThanOrEqualToNumeric payload
            }
        | LessThanNumeric _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.int64) "LessThanNumeric" (function (LessThanNumeric (x1, x2)) -> Some (x1, x2) | _ -> None)
                return LessThanNumeric payload
            }
        | LessThanOrEqualToNumeric _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.int64) "LessThanOrEqualToNumeric" (function (LessThanOrEqualToNumeric (x1, x2)) -> Some (x1, x2) | _ -> None)
                return LessThanOrEqualToNumeric payload
            }
        | EqualToString _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "EqualToString" (function (EqualToString (x1, x2)) -> Some (x1, x2) | _ -> None)
                return EqualToString payload
            }
        | GreaterThanString _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "GreaterThanString" (function (GreaterThanString (x1, x2)) -> Some (x1, x2) | _ -> None)
                return GreaterThanString payload
            }
        | GreaterThanOrEqualToString _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "GreaterThanOrEqualToString" (function (GreaterThanOrEqualToString (x1, x2)) -> Some (x1, x2) | _ -> None)
                return GreaterThanOrEqualToString payload
            }
        | LessThanString _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "LessThanString" (function (LessThanString (x1, x2)) -> Some (x1, x2) | _ -> None)
                return LessThanString payload
            }
        | LessThanOrEqualToString _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "LessThanOrEqualToString" (function (LessThanOrEqualToString (x1, x2)) -> Some (x1, x2) | _ -> None)
                return LessThanOrEqualToString payload
            }
        | StartsWith _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "StartsWith" (function (StartsWith (x1, x2)) -> Some (x1, x2) | _ -> None)
                return StartsWith payload
            }
        | Matches _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "Matches" (function (Matches (x1, x2)) -> Some (x1, x2) | _ -> None)
                return Matches payload
            }
        | MatchesExact _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "MatchesExact" (function (MatchesExact (x1, x2)) -> Some (x1, x2) | _ -> None)
                return MatchesExact payload
            }
        | MatchesPrefix _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.string) "MatchesPrefix" (function (MatchesPrefix (x1, x2)) -> Some (x1, x2) | _ -> None)
                return MatchesPrefix payload
            }
        | IntersectsGeography _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string codecFor<_, GeographyIndexValue>) "IntersectsGeography" (function (IntersectsGeography (x1, x2)) -> Some (x1, x2) | _ -> None)
                return IntersectsGeography payload
            }
        | And _ ->
            codec {
                let! payload = reqWithLazy (fun () -> Codecs.tuple2 (UntypedPredicate.get_Codec ()) (UntypedPredicate.get_Codec ())) "And" (function (And (x1, x2)) -> Some (x1, x2) | _ -> None)
                return And payload
            }
        | Or _ ->
            codec {
                let! payload = reqWithLazy (fun () -> Codecs.tuple2 (UntypedPredicate.get_Codec ()) (UntypedPredicate.get_Codec ())) "Or" (function (Or (x1, x2)) -> Some (x1, x2) | _ -> None)
                return Or payload
            }
        | Diff _ ->
            codec {
                let! payload = reqWithLazy (fun () -> Codecs.tuple2 (UntypedPredicate.get_Codec ()) (UntypedPredicate.get_Codec ())) "Diff" (function (Diff (x1, x2)) -> Some (x1, x2) | _ -> None)
                return Diff payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type SubjectTransactionId
with
    static member TypeLabel () = "SubjectTransactionId"
    static member get_ObjCodec () =
        function
        | SubjectTransactionId _ ->
            codec {
                let! payload = reqWith Codecs.guid "SubjectTransactionId" (fun (SubjectTransactionId x) -> Some x)
                return SubjectTransactionId payload
            }
        |> mergeUnionCases

    static member get_Codec () = ofObjCodec <| SubjectTransactionId.get_ObjCodec ()

    // Init needs generic 'op because ecosystem-specific transaction life cycles
    // are implemented via generic SubjectTransactionLifeCycle<>

    static member Init (typeLabel: string, _typeParams: _) =
        initializeInterfaceImplementation<SubjectId, SubjectTransactionId> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| SubjectTransactionId.get_ObjCodec ())

type PreparedIndexPredicate<'SubjectIndex>
with
    static member get_Codec () =
        function
        | PreparedIndexPredicate _ ->
            codec {
                let! payload = reqWith (UntypedPredicate.get_Codec ()) "PreparedIndexPredicate" (fun (PreparedIndexPredicate x) -> Some x)
                return PreparedIndexPredicate payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type ResultSetOptions<'SubjectIndex>
with
    static member get_Codec () =
        function
        | ResultSetOptions_ _ ->
            codec {
                let! payload = reqWith (UntypedResultSetOptions.get_Codec ()) "ResultSetOptions_" (function (ResultSetOptions_ x) -> Some x)
                return ResultSetOptions_ payload
            }
        |> mergeUnionCases
        |> ofObjCodec


type IndexQuery<'SubjectIndex>
with
    static member get_Codec () =
        function
        | IndexQuery _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 (UntypedPredicate.get_Codec ()) (UntypedResultSetOptions.get_Codec ())) "IndexQuery" (function (IndexQuery (x1, x2)) -> Some (x1, x2))
                return IndexQuery payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type IndexKey
with
    static member get_Codec () =
        function
        | Numeric _ ->
            codec {
                let! payload = reqWith Codecs.string "Numeric" (function Numeric x -> Some x | _ -> None)
                return Numeric payload
            }
        | String _ ->
            codec {
                let! payload = reqWith Codecs.string "String" (function String x -> Some x | _ -> None)
                return String payload
            }
        | Search _ ->
            codec {
                let! payload = reqWith Codecs.string "Search" (function Search x -> Some x | _ -> None)
                return Search payload
            }
        | Geography _ ->
            codec {
                let! payload = reqWith Codecs.string "Geography" (function Geography x -> Some x | _ -> None)
                return Geography payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type GetMaybeConstruct<'SubjectId, 'Constructor
        when 'SubjectId :> SubjectId
        and  'Constructor :> Constructor> with
    static member inline get_Codec () =
        codec {
            let! id = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'id> "Id" (fun x -> Some x.Id)
            and! constructor = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'constructor> "Constructor" (fun x -> Some x.Constructor)
            return { Id = id; Constructor = constructor }
        }
        |> ofObjCodec

    member this.AsUntyped = { Id = this.Id :> SubjectId; Constructor = this.Constructor :> Constructor }

type ActMaybeConstruct<'LifeAction, 'Constructor
                        when 'LifeAction  :> LifeAction
                        and  'Constructor :> Constructor> with
    static member inline get_Codec () =
        codec {
            let! action = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "Action" (fun x -> Some x.Action)
            and! constructor = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'constructor> "Constructor" (fun x -> Some x.Constructor)
            return { Action = action; Constructor = constructor }
        }
        |> ofObjCodec

    member this.AsUntyped = { Action = this.Action :> LifeAction; Constructor = this.Constructor :> Constructor }

type ActAndWaitOnLifeEvent<'LifeAction, 'LifeEvent
                            when 'LifeAction :> LifeAction
                            and  'LifeEvent  :> LifeEvent> with
    static member inline get_Codec () =
        codec {
            let! id = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "Action" (fun x -> Some x.Action)
            and! lifeEvent = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeEvent> "LifeEvent" (fun x -> Some x.LifeEvent)
            return { Action = id; LifeEvent = lifeEvent }
        }
        |> ofObjCodec

    member this.AsUntyped = { Action = this.Action :> LifeAction; LifeEvent = this.LifeEvent :> LifeEvent }

type ActMaybeConstructAndWaitOnLifeEvent<'LifeAction, 'Constructor, 'LifeEvent
                                          when 'LifeAction :> LifeAction
                                          and  'Constructor :> Constructor
                                          and  'LifeEvent  :> LifeEvent> with
    static member inline get_Codec () =
        codec {
            let! id = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeAction> "Action" (fun x -> Some x.Action)
            and! constructor = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'constructor> "Constructor" (fun x -> Some x.Constructor)
            and! lifeEvent = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeEvent> "LifeEvent" (fun x -> Some x.LifeEvent)
            return { Action = id; Constructor = constructor; LifeEvent = lifeEvent }
        }
        |> ofObjCodec

    member this.AsUntyped = { Action = this.Action :> LifeAction; Constructor = this.Constructor :> Constructor; LifeEvent = this.LifeEvent :> LifeEvent }

type ConstructAndWaitOnLifeEvent<'Constructor, 'LifeEvent
                                  when 'Constructor :> Constructor
                                  and  'LifeEvent  :> LifeEvent> with
    static member inline get_Codec () =
        codec {
            let! constructor = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'constructor> "Constructor" (fun x -> Some x.Constructor)
            and! lifeEvent = reqWith defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, 'lifeEvent> "LifeEvent" (fun x -> Some x.LifeEvent)
            return { Constructor = constructor; LifeEvent = lifeEvent }
        }
        |> ofObjCodec

    member this.AsUntyped = { Constructor = this.Constructor :> Constructor; LifeEvent = this.LifeEvent :> LifeEvent }

#endif


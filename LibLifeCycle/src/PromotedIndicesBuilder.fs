[<AutoOpen>]
module LibLifeCycle.PromotedIndicesWorkflow

open System

type PromotedKey = PromotedKey of string
type PromotedValue = PromotedValue of string
type BaseKey = BaseKey of string
type BaseValue = BaseValue of string
type BaseSeparator = BaseSeparator of String

[<RequireQualifiedAccess>]
type PromotedIndex<'NumericIndex, 'StringIndex>
    when 'StringIndex :> Union
    and 'NumericIndex :> Union =
| Numeric of UnionCase<'NumericIndex>
| String of UnionCase<'StringIndex>
    static member PromoteIndex (caseFactory: 'TupleOrSingleValue -> 'NumericIndex) =
        UnionCase.ofCase caseFactory |> PromotedIndex<'NumericIndex, 'StringIndex>.Numeric
    static member PromoteIndex (caseFactory: 'TupleOrSingleValue -> 'StringIndex) =
        UnionCase.ofCase caseFactory |> PromotedIndex<'NumericIndex, 'StringIndex>.String
    static member PromoteIndex (index: 'NumericIndex) =
        UnionCase.ofCase index |> PromotedIndex<'NumericIndex, 'StringIndex>.Numeric
    static member PromoteIndex (index: 'StringIndex) =
        UnionCase.ofCase index |> PromotedIndex<'NumericIndex, 'StringIndex>.String
    member this.CaseName =
        match this with
        | Numeric case -> case.CaseName
        | String case -> case.CaseName

type PromotedIndicesConfig =
    private PromotedIndicesConfig of Option<(* 'SubjectNumericIndexType *) Type * (* 'SubjectStringIndexType *) Type * Map<PromotedKey, NonemptyList<Choice<BaseKey, BaseSeparator>>>>
with
    member this.Mappings =
        let (PromotedIndicesConfig maybeConfig) = this
        match maybeConfig with
        | Some (_, _, mappings) -> mappings
        | None -> Map.empty

    member internal this.SubjectNumericAndStringIndexTypes =
        let (PromotedIndicesConfig maybeConfig) = this
        maybeConfig |> Option.map (fun (subjectNumericIndexType, subjectStringIndexType, _) -> subjectNumericIndexType, subjectStringIndexType)

    static member Empty = PromotedIndicesConfig None

type PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError> =
    private PromotedIndicesBuilderState of Map<PromotedKey, NonemptyList<Choice<BaseKey, BaseSeparator>>>

type PromotedIndicesBuilder () =

    member _.Zero<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError>
            ()
            : PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError> =

        PromotedIndicesBuilderState Map.empty

    member _.Yield<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError>
            (idx: PromotedIndex<'SubjectNumericIndex, 'SubjectStringIndex>)
            : PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError> =

        PromotedIndicesBuilderState (Map.ofOneItem (PromotedKey idx.CaseName, nel { Choice1Of2(BaseKey idx.CaseName) }))

    member _.Yield<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError>
            (compoundIdx: PromotedIndex<'SubjectNumericIndex, 'SubjectStringIndex> * string * PromotedIndex<'SubjectNumericIndex, 'SubjectStringIndex>)
            : PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError> =

        let idx1, sep, idx2 = compoundIdx
        if idx1 = idx2 then
            failwith $"Promoted index definition is invalid. Duplicate names found:\n{idx1}\n{idx2}"

        PromotedIndicesBuilderState (Map.ofOneItem (PromotedKey $"{idx1.CaseName}_{idx2.CaseName}", nel { Choice1Of2 (BaseKey idx1.CaseName); Choice2Of2 (BaseSeparator sep); Choice1Of2(BaseKey idx2.CaseName) }))

    member _.Yield<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError>
            (compoundIdx: PromotedIndex<'SubjectNumericIndex, 'SubjectStringIndex> * string * PromotedIndex<'SubjectNumericIndex, 'SubjectStringIndex> * string * PromotedIndex<'SubjectNumericIndex, 'SubjectStringIndex>)
            : PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError> =

        let idx1, sep1, idx2, sep2, idx3 = compoundIdx
        if (List.distinct [idx1; idx2; idx3]).Length < 3 then
            failwith $"Promoted index definition is invalid. Duplicate names found:\n{idx1}\n{idx2}\n{idx3}"

        PromotedIndicesBuilderState (Map.ofOneItem (PromotedKey $"{idx1.CaseName}_{idx2.CaseName}_{idx3.CaseName}", nel { Choice1Of2 (BaseKey idx1.CaseName); Choice2Of2 (BaseSeparator sep1); Choice1Of2 (BaseKey idx2.CaseName); Choice2Of2 (BaseSeparator sep2); Choice1Of2 (BaseKey idx3.CaseName) }))

    member _.Combine<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError>
            (config: PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>, res: unit -> PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>)
            : PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError> =

        let (PromotedIndicesBuilderState map1) = config
        let (PromotedIndicesBuilderState map2) = res()

        if (map1 |> Map.keys |> Set.filterMap map2.TryFind).IsNonempty || (map2 |> Map.keys |> Set.filterMap map1.TryFind).IsNonempty then
            failwith $"Promoted index definition is invalid. Overlapping promoted index table names:\n{map1.Keys}\n{map2.Keys}"

        PromotedIndicesBuilderState (Map.merge map1 map2)

    member _.Delay<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError>
            (value: unit -> PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>)
            : unit -> PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError> =

        value

    member _.Run<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError
            when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
            and 'SubjectStringIndex :> SubjectStringIndex<'OpError>
            and 'OpError :> OpError>
            (value: unit -> PromotedIndicesBuilderState<'SubjectNumericIndex, 'SubjectStringIndex, 'OpError>)
            : PromotedIndicesConfig =

        let (PromotedIndicesBuilderState.PromotedIndicesBuilderState config) = value ()
        PromotedIndicesConfig (Some (typeof<'SubjectNumericIndex>, typeof<'SubjectStringIndex>, config))

let promotedIndices = PromotedIndicesBuilder ()

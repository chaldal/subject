[<AutoOpen>]
module LibLifeCycleHost.ValueSummarizer

open LibLifeCycle

#nowarn "0686"  // Allow exceptionally here to have explicit parameters for codec related functions

// Provides a succinct summary of a value, optimized for high-perf production use-cases, such as logging

open System
open System.Reflection
open System.Collections.Generic
open FSharp.Reflection
open FSharpPlus

type private ValueSummarizer  = obj -> string
type ValueSummarizers = private ValueSummarizers of IReadOnlyDictionary<Type, ValueSummarizer> * Default: (Type -> ValueSummarizer)
with
    member this.FormatValue (value: obj) : string =
        let (ValueSummarizers(summarizers, defaultSummarizer)) = this
        if isNull value then
            "<NULL>"
        else
            let typ = value.GetType()
            match IReadOnlyDictionary.tryGetValue typ summarizers with
            | Some summarizer ->
                summarizer value
            | None ->
                defaultSummarizer typ value

let redactSummarizer (summarizer: ValueSummarizer) =
    fun value ->
        match (box value) with
        | :? IRedactable as redactable -> redactable.Redact()
        | (v: obj) -> v
        |> summarizer

////////////////////////////////////////////////////////////
// Fast summarizers (super fast but not too informative)

let private getTypeNameAsSummary (typ: Type) =
    let genericDeclaration =
        if typ.IsGenericType then
            typ.GetGenericArguments()
            |> Seq.map (fun gTyp -> gTyp.Name)
            |> fun gTypNames -> String.Join(", ", gTypNames)
            |> sprintf "<%s>"
        else
            ""

    sprintf "{%s%s}" typ.Name genericDeclaration

// ToString could potentially be expensive (esp for F# types, which calls a "%A" sprintf)
let private plainToStringSummarizers : IReadOnlyDictionary<Type, ValueSummarizer> =
    [
        typeof<int>; typeof<int16>; typeof<int64>
        typeof<uint32>; typeof<uint16>; typeof<uint64>
        typeof<byte>; typeof<sbyte>
        typeof<string>; typeof<bool>; typeof<Guid>;
        typeof<float>; typeof<single>; typeof<decimal>;
        typeof<DateTime>; typeof<DateTimeOffset>; typeof<TimeSpan>
        typeof<NonemptyString>
    ]
    |> Seq.map (fun i -> (i, (fun (o: obj) -> if isNull o then "<NULL>" else o.ToString())))
    |> readOnlyDict

type private InnerSummarizers =
| SomeValueBased of list<(obj -> string)>
| AllConstants   of list<string>

let private getInnerSummarizer (fieldTypes: seq<Type>) (valueToFields: obj -> obj[]) =
    let fieldTypesArr = fieldTypes |> Seq.toList

    // Avoid reading valueToFields if its not needed

    let innerSummarizers =
        fieldTypesArr
        |> Seq.truncate 3
        |> Seq.fold (fun accInnerSummarizers typ ->
            if plainToStringSummarizers.ContainsKey typ then
                let innerSummarizer = fun (o: obj) -> if isNull o then "<NULL>" else o.ToString()
                match accInnerSummarizers with
                | SomeValueBased summarizers ->
                    SomeValueBased(innerSummarizer::summarizers)
                | AllConstants constants ->
                    let summarizers = constants |> List.map (fun str -> (fun _ -> str))
                    SomeValueBased(innerSummarizer::summarizers)
            else
                let summary = getTypeNameAsSummary typ
                match accInnerSummarizers with
                | SomeValueBased summarizers ->
                    let innerSummarizer = fun _ -> summary
                    SomeValueBased(innerSummarizer::summarizers)
                | AllConstants constants ->
                    AllConstants(summary::constants)
        ) (AllConstants [])

    let wasTruncated = fieldTypesArr.Length > 3

    match innerSummarizers with
    | SomeValueBased valueBasedSummarizers ->
        let summarizersArr =
            valueBasedSummarizers
            |> Seq.rev
            |> Seq.toArray

        fun (obj: obj) -> seq {
            yield!
                valueToFields obj
                |> Seq.truncate 3
                |> Seq.zip summarizersArr
                |> Seq.map (fun (a,b) -> a b)

            if wasTruncated then
                yield ".."
        }
    | AllConstants constantSummarizers ->
        let constantSummarizersArr = constantSummarizers |> Seq.rev |> Seq.toArray |> Array.toSeq
        fun _ -> constantSummarizersArr

let private getTupleSummarizer (typ: Type) : ValueSummarizer =
    let fieldReader = FSharpValue.PreComputeTupleReader typ
    let fields = FSharpType.GetTupleElements typ
    let innerSummarizer = getInnerSummarizer fields fieldReader

    fun obj ->
        innerSummarizer obj
        |> fun vals -> String.Join(", ", vals)
        |> sprintf "(%s)"

let private getRecordSummarizer (typ: Type) : ValueSummarizer =
    let fieldReader = FSharpValue.PreComputeRecordReader(typ, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
    let fields = FSharpType.GetRecordFields(typ, true) |> Seq.map (fun fieldInfo -> fieldInfo.PropertyType)
    let innerSummarizer = getInnerSummarizer fields fieldReader

    fun obj ->
        innerSummarizer obj
        |> fun vals -> String.Join("; ", vals)
        |> sprintf "{%s}"

let private getUnionSummarizer (typ: Type) : ValueSummarizer =
    let tagReader = FSharpValue.PreComputeUnionTagReader(typ, true)
    let tagsToCaseNameAndInnerSummarizer =
        FSharpType.GetUnionCases typ
        |> Seq.map (fun unionCase ->
            let fieldReader = FSharpValue.PreComputeUnionReader(unionCase, true)
            let fields = unionCase.GetFields() |> Seq.map (fun fieldInfo -> fieldInfo.PropertyType)
            let innerSummarizer = getInnerSummarizer fields fieldReader

            (unionCase.Tag, (unionCase.Name, innerSummarizer))
        )
        |> readOnlyDict

    fun obj ->
        let tag = tagReader obj
        let (caseName, innerSummarizer) = tagsToCaseNameAndInnerSummarizer.[tag]
        let inners =
            innerSummarizer obj
            |> fun vals -> String.Join(", ", vals)
            |> fun str -> if str = "" then "" else sprintf "(%s)" str

        sprintf "%s%s" caseName inners

let private tryGetFSharpTypeSummarizerOrPlainToStringSummarizer (typ: Type) =
    if FSharpType.IsUnion typ then
        getUnionSummarizer typ |> Some
    elif FSharpType.IsTuple typ then
        getTupleSummarizer typ |> Some
    elif FSharpType.IsRecord typ then
        getRecordSummarizer typ |> Some
    else
        match plainToStringSummarizers.TryGetValue typ with
        | true, summarizer ->
            Some summarizer
        | false, _ ->
            None

let getFastestButNotInformativeDefaultSummarizer (typ: Type) =
    let typNam = getTypeNameAsSummary typ
    fun _ -> typNam

let private getFastestButNotInformativeSummarizersForTypes (types: seq<Type>) : seq<Type * ValueSummarizer> =
    types
    |> Seq.choose (fun typ -> tryGetFSharpTypeSummarizerOrPlainToStringSummarizer typ |> Option.map (fun summarizer -> typ, summarizer))

////////////////////////////////////////////////////////////
// Codec summarizers (good balance of fast & informative)

let private getSomewhatFastCodecSummarizers (lifeCycleDefs: list<LifeCycleDef>) : seq<Type * ValueSummarizer> =
    lifeCycleDefs
    |> Seq.collect LibLifeCycleCore.SummaryEncoders.getSummaryEncodersForLifeCycle
    |> Seq.map (fun (typ, summarizer) -> typ, redactSummarizer summarizer)

////////////////////////////////////////////////////////////
// Combined codec (where available) & fastest summarizers

let mixedCodecBasedAndFastSummarizersForEcosystem (ecosystem: Ecosystem) (overrideDefaultSummarizer: Option<Type -> ValueSummarizer>) : ValueSummarizers =
        Seq.concat
            [
                getFastestButNotInformativeSummarizersForTypes
                     // connector request types don't have codecs. Should we add them?
                     (ecosystem.Connectors |> Map.values
                      |> Seq.map (fun connector ->
                          connector.Invoke
                              { new FullyTypedConnectorFunction<_> with
                                  member _.Invoke(_: Connector<'Request, _>) = typeof<'Request> })
                      )

                plainToStringSummarizers |> Seq.map (fun kvp -> kvp.Key, kvp.Value)
                // if any type summarizer is ambiguous, second sequence i.e. codec version wins
                (getSomewhatFastCodecSummarizers ecosystem.LifeCycleDefs)
            ]
        |> readOnlyDict
        |> fun typeToSummarizer ->
            typeToSummarizer
            |> Seq.map (fun kvp -> kvp.Key, kvp.Value)
            |> Seq.append (
                typeToSummarizer
                |> Seq.map (fun kvp -> kvp.Key)
                |> Seq.toList
                |> LibLifeCycleCore.OrleansEx.Serialization.getCaseInstanceTypesForUnionTypes
                |> List.map (fun (unionType, caseType) -> caseType, typeToSummarizer[unionType]))
        |> readOnlyDict
        |> fun summarizers ->
            ValueSummarizers (summarizers, overrideDefaultSummarizer |> Option.defaultValue getFastestButNotInformativeDefaultSummarizer)

// TODO: delete this func if codec summarizers proven reliable. Otherwise use it to quickly revert to old summarizers
[<Obsolete>]
let fastestButNotInformativeSummarizersForEcosystem (ecosystem: Ecosystem) =
    // piggyback on code-based summarizers only to discover types
    let (ValueSummarizers (summarizers, _)) = mixedCodecBasedAndFastSummarizersForEcosystem ecosystem None
    summarizers.Keys
    |> getFastestButNotInformativeSummarizersForTypes
    |> readOnlyDict
    |> fun summarizers ->
        ValueSummarizers (summarizers, getFastestButNotInformativeDefaultSummarizer)

////////////////////////////////////////////////////////////
// Debug summarizers (informative but slow)

let defaultSlowSummarizer : (Type -> ValueSummarizer) = (fun _ -> sprintf "%A")

let slowDebugValueSummarizers : ValueSummarizers =
    // No need to register any summarizers; just default to using "%A"
    // Warning: %A is really slow; do not use in production
    ValueSummarizers((readOnlyDict Seq.empty), defaultSlowSummarizer)

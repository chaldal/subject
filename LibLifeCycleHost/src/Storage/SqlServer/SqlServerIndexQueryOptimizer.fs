[<AutoOpen>]
module LibLifeCycleHost.Storage.SqlServer.SqlServerPromotedIndexQueryOptimizer

open LibLifeCycle
open LibLifeCycleHost.Storage.SqlServer
open LibLifeCycleTypes

// Applies 3 optimization techniques to index queries:
// * Index promotion & trimming of redundant query nodes after promotion
// * Reduce multiple inequality nodes for the same key to a single Between node
// * Reduce multiple OR'd equality nodes into single In node

[<RequireQualifiedAccess>]
type PredicateBound<'T> =
| Unbounded
| Exclusive of Value: 'T
| Inclusive of Value: 'T

[<RequireQualifiedAccess>]
type OptimizedUntypedPredicate =
| BetweenNumeric              of Key: string * LowerBound: PredicateBound<int64>  * UpperBound: PredicateBound<int64>  * Option<PromotedKey * PromotedValue>
| BetweenString               of Key: string * LowerBound: PredicateBound<string> * UpperBound: PredicateBound<string> * Option<PromotedKey * PromotedValue>
| InNumeric                   of Key: string * Values: List<int64>                                                     * Option<PromotedKey * PromotedValue>
| InString                    of Key: string * Values: List<string>                                                    * Option<PromotedKey * PromotedValue>
| StartsWith                  of Key: string * StartsWith: string                                                      * Option<PromotedKey * PromotedValue>
| And                         of OptimizedUntypedPredicate * OptimizedUntypedPredicate
| Or                          of OptimizedUntypedPredicate * OptimizedUntypedPredicate
| Diff                        of OptimizedUntypedPredicate * OptimizedUntypedPredicate
| Matches                     of Key: string * Keywords: string
| MatchesExact                of Key: string * Keywords: string
| MatchesPrefix               of Key: string * KeywordsPrefix: string
| IntersectsGeography         of Key: string * GeographyIndexValue

[<RequireQualifiedAccess>]
type private AlgoUntypedPredicate =
| BetweenNumeric              of Key: string * LowerBound: PredicateBound<int64>  * UpperBound: PredicateBound<int64>  * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| BetweenString               of Key: string * LowerBound: PredicateBound<string> * UpperBound: PredicateBound<string> * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| InNumeric                   of Key: string * Values: List<int64>                                                     * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| InString                    of Key: string * Values: List<string>                                                    * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| StartsWith                  of Key: string * StartsWith: string                                                      * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| AndGroup                    of Set<AlgoUntypedPredicate>                                                             * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| OrGroup                     of Set<AlgoUntypedPredicate>                                                             * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| Diff                        of AlgoUntypedPredicate * AlgoUntypedPredicate                                           * Option<(PromotedKey * PromotedValue) * Set<BaseKey>>
| Matches                     of Key: string * Keywords: string
| MatchesExact                of Key: string * Keywords: string
| MatchesPrefix               of Key: string * KeywordsPrefix: string
| IntersectsGeography         of Key: string * GeographyIndexValue

let optimizeQueryWithPromotedIndices (promotedIndicesConfig: Map<PromotedKey, NonemptyList<Choice<BaseKey, BaseSeparator>>>) (query: UntypedPredicate) : OptimizedUntypedPredicate =
    // all promoted base index keys, either promoted as singular promoted index or as part of compound promoted index
    let allPromotedBaseKeys =
        promotedIndicesConfig
        |> Map.values
        |> Seq.map (NonemptyList.toList >> Set.ofList)
        |> Set.unionMany
        |> Seq.choose (function | Choice1Of2 baseKey -> Some baseKey | Choice2Of2 _sep -> None)
        |> Set.ofSeq

    // chooses widest promoted key from set of possible base (key * value)
    // reasoning: widest key is most specific so will match fewest rows and produce most efficient query
    let bestPromotion (promotableBaseIndices: Map<BaseKey, BaseValue>) : Option<(PromotedKey * PromotedValue) * Set<BaseKey>> =
        promotedIndicesConfig
        |> Map.toSeq
        |> Seq.choose (
            fun (promotedKey, ofBaseKeysAndSeparators) ->
                let baseValuesAndSeparators =
                    ofBaseKeysAndSeparators.ToList
                    |> List.map (function | Choice1Of2 ofBaseKey -> Map.tryFind ofBaseKey promotableBaseIndices |> Option.map Choice1Of2 | Choice2Of2 sep -> Some (Choice2Of2 sep))
                    |> Option.flattenList

                if baseValuesAndSeparators.Length = ofBaseKeysAndSeparators.Length then
                    // found baseValue for each baseKey in promoted index
                    let promotedValue =
                        baseValuesAndSeparators
                        |> List.map (function | Choice1Of2 (BaseValue baseValue) -> baseValue | Choice2Of2 (BaseSeparator sep) -> sep)
                        |> String.concat ""
                        |> PromotedValue
                    Some (
                        (promotedKey, promotedValue),
                        ofBaseKeysAndSeparators.ToList |> List.choose (function | Choice1Of2 ofBaseKey -> Some ofBaseKey | Choice2Of2 _sep -> None) |> Set.ofList)
                else
                    None
            )
        |> Seq.sortByDescending (snd >> Set.count) // best promoted is made of highest number of baseKeys
        |> Seq.tryHead

    let reduceBetweensInAndGroup group =
        let numericBetweens =
            group
            |> Set.filterMap (
                function
                | AlgoUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, maybePromotion) -> Some (key, (lowerBound, upperBound, maybePromotion))
                | AlgoUntypedPredicate.BetweenString _
                | AlgoUntypedPredicate.InNumeric _
                | AlgoUntypedPredicate.InString _
                | AlgoUntypedPredicate.Diff _
                | AlgoUntypedPredicate.AndGroup _
                | AlgoUntypedPredicate.OrGroup _
                | AlgoUntypedPredicate.BetweenNumeric _
                | AlgoUntypedPredicate.BetweenString _
                | AlgoUntypedPredicate.StartsWith _
                | AlgoUntypedPredicate.Matches _
                | AlgoUntypedPredicate.MatchesExact _
                | AlgoUntypedPredicate.MatchesPrefix _
                | AlgoUntypedPredicate.IntersectsGeography _ -> None)

        let stringBetweens =
            group
            |> Set.filterMap (
                function
                | AlgoUntypedPredicate.BetweenString (key, lowerBound, upperBound, maybePromotion) -> Some (key, (lowerBound, upperBound, maybePromotion))
                | AlgoUntypedPredicate.BetweenNumeric _
                | AlgoUntypedPredicate.InNumeric _
                | AlgoUntypedPredicate.InString _
                | AlgoUntypedPredicate.Diff _
                | AlgoUntypedPredicate.AndGroup _
                | AlgoUntypedPredicate.OrGroup _
                | AlgoUntypedPredicate.BetweenNumeric _
                | AlgoUntypedPredicate.BetweenString _
                | AlgoUntypedPredicate.StartsWith _
                | AlgoUntypedPredicate.Matches _
                | AlgoUntypedPredicate.MatchesExact _
                | AlgoUntypedPredicate.MatchesPrefix _
                | AlgoUntypedPredicate.IntersectsGeography _ -> None)

        let reduceBetweens betweens =
            betweens
            |> Set.toList
            |> List.groupBy fst
            |> List.map (
                fun (key, betweens) ->
                    betweens
                    |> List.reduce (
                        fun (_, (curLowerBound, curUpperBound, _)) (_, (newLowerBound, newUpperBound, _)) ->
                            let lowerBound =
                                match curLowerBound, newLowerBound with
                                | PredicateBound.Unbounded,          PredicateBound.Unbounded          -> PredicateBound.Unbounded
                                | PredicateBound.Unbounded,          PredicateBound.Inclusive newValue -> PredicateBound.Inclusive newValue
                                | PredicateBound.Unbounded,          PredicateBound.Exclusive newValue -> PredicateBound.Exclusive newValue
                                | PredicateBound.Exclusive curValue, PredicateBound.Unbounded          -> PredicateBound.Exclusive curValue
                                | PredicateBound.Inclusive curValue, PredicateBound.Unbounded          -> PredicateBound.Inclusive curValue
                                | PredicateBound.Inclusive curValue, PredicateBound.Inclusive newValue -> PredicateBound.Inclusive (if newValue > curValue then newValue else curValue)
                                | PredicateBound.Exclusive curValue, PredicateBound.Exclusive newValue -> PredicateBound.Exclusive (if newValue > curValue then newValue else curValue)
                                | PredicateBound.Inclusive curValue, PredicateBound.Exclusive newValue -> if newValue >= curValue then PredicateBound.Exclusive newValue else PredicateBound.Inclusive curValue
                                | PredicateBound.Exclusive curValue, PredicateBound.Inclusive newValue -> if newValue >  curValue then PredicateBound.Inclusive newValue else PredicateBound.Exclusive curValue

                            let upperBound =
                                match curUpperBound, newUpperBound with
                                | PredicateBound.Unbounded,          PredicateBound.Unbounded          -> PredicateBound.Unbounded
                                | PredicateBound.Unbounded,          PredicateBound.Inclusive newValue -> PredicateBound.Inclusive newValue
                                | PredicateBound.Unbounded,          PredicateBound.Exclusive newValue -> PredicateBound.Exclusive newValue
                                | PredicateBound.Exclusive curValue, PredicateBound.Unbounded          -> PredicateBound.Exclusive curValue
                                | PredicateBound.Inclusive curValue, PredicateBound.Unbounded          -> PredicateBound.Inclusive curValue
                                | PredicateBound.Inclusive curValue, PredicateBound.Inclusive newValue -> PredicateBound.Inclusive (if newValue < curValue then newValue else curValue)
                                | PredicateBound.Exclusive curValue, PredicateBound.Exclusive newValue -> PredicateBound.Exclusive (if newValue < curValue then newValue else curValue)
                                | PredicateBound.Inclusive curValue, PredicateBound.Exclusive newValue -> if newValue <= curValue then PredicateBound.Exclusive newValue else PredicateBound.Inclusive curValue
                                | PredicateBound.Exclusive curValue, PredicateBound.Inclusive newValue -> if newValue <  curValue then PredicateBound.Inclusive newValue else PredicateBound.Exclusive curValue

                            (key, (lowerBound, upperBound, None))))

        // we don't keep record of any potential promotions here, this reduction must occur BEFORE promotions are chosen!!
        let reducedBounds =
            Set.union
                (reduceBetweens numericBetweens |> List.map (fun (key, (lowerBound, upperBound, _)) -> AlgoUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, None)) |> Set.ofList)
                (reduceBetweens stringBetweens  |> List.map (fun (key, (lowerBound, upperBound, _)) -> AlgoUntypedPredicate.BetweenString  (key, lowerBound, upperBound, None)) |> Set.ofList)

        group
        |> Set.filter (
            function
            | AlgoUntypedPredicate.BetweenNumeric _
            | AlgoUntypedPredicate.BetweenString _ -> false
            | AlgoUntypedPredicate.InNumeric _
            | AlgoUntypedPredicate.InString _
            | AlgoUntypedPredicate.Diff _
            | AlgoUntypedPredicate.AndGroup _
            | AlgoUntypedPredicate.OrGroup _
            | AlgoUntypedPredicate.BetweenNumeric _
            | AlgoUntypedPredicate.BetweenString _
            | AlgoUntypedPredicate.StartsWith _
            | AlgoUntypedPredicate.Matches _
            | AlgoUntypedPredicate.MatchesExact _
            | AlgoUntypedPredicate.MatchesPrefix _
            | AlgoUntypedPredicate.IntersectsGeography _ -> true)
        |> Set.union reducedBounds

    let reduceInsInOrGroup group =
        let numericEquals =
            group
            |> Set.filterMap (
                function
                | AlgoUntypedPredicate.InNumeric (key, values, _) -> Some (key, Set.ofList values)
                | AlgoUntypedPredicate.BetweenNumeric _
                | AlgoUntypedPredicate.BetweenString _
                | AlgoUntypedPredicate.InString _
                | AlgoUntypedPredicate.Diff _
                | AlgoUntypedPredicate.AndGroup _
                | AlgoUntypedPredicate.OrGroup _
                | AlgoUntypedPredicate.BetweenNumeric _
                | AlgoUntypedPredicate.BetweenString _
                | AlgoUntypedPredicate.StartsWith _
                | AlgoUntypedPredicate.Matches _
                | AlgoUntypedPredicate.MatchesExact _
                | AlgoUntypedPredicate.MatchesPrefix _
                | AlgoUntypedPredicate.IntersectsGeography _ -> None)

        let stringEquals =
            group
            |> Set.filterMap (
                function
                | AlgoUntypedPredicate.InString (key, values, _) -> Some (key, Set.ofList values)
                | AlgoUntypedPredicate.BetweenNumeric _
                | AlgoUntypedPredicate.BetweenString _
                | AlgoUntypedPredicate.InNumeric _
                | AlgoUntypedPredicate.Diff _
                | AlgoUntypedPredicate.AndGroup _
                | AlgoUntypedPredicate.OrGroup _
                | AlgoUntypedPredicate.BetweenNumeric _
                | AlgoUntypedPredicate.BetweenString _
                | AlgoUntypedPredicate.StartsWith _
                | AlgoUntypedPredicate.Matches _
                | AlgoUntypedPredicate.MatchesExact _
                | AlgoUntypedPredicate.MatchesPrefix _
                | AlgoUntypedPredicate.IntersectsGeography _ -> None)

        let equalsToIns (equals: Set<string * Set<'Value>>) inPredicate =
            equals
            |> Set.toList
            |> List.groupBy fst
            |> List.map (fun (key, keyValues) -> key, keyValues |> List.map snd |> Set.unionMany)
            |> List.map (fun (key, values) -> (inPredicate (key, Set.toList values, None)))
            |> Set.ofList

        // we don't keep record of any potential promotions here, this reduction must occur BEFORE promotions are chosen!!
        let ins =
            Set.union
                (equalsToIns numericEquals AlgoUntypedPredicate.InNumeric)
                (equalsToIns stringEquals  AlgoUntypedPredicate.InString)

        group
        |> Set.filter (
            function
            | AlgoUntypedPredicate.InNumeric _
            | AlgoUntypedPredicate.InString _ -> false
            | AlgoUntypedPredicate.BetweenNumeric _
            | AlgoUntypedPredicate.BetweenString _
            | AlgoUntypedPredicate.Diff _
            | AlgoUntypedPredicate.AndGroup _
            | AlgoUntypedPredicate.OrGroup _
            | AlgoUntypedPredicate.BetweenNumeric _
            | AlgoUntypedPredicate.BetweenString _
            | AlgoUntypedPredicate.StartsWith _
            | AlgoUntypedPredicate.Matches _
            | AlgoUntypedPredicate.MatchesExact _
            | AlgoUntypedPredicate.MatchesPrefix _
            | AlgoUntypedPredicate.IntersectsGeography _ -> true)
        |> Set.union ins

    let rec queryToAlgoInput root =
        match root with
        | UntypedPredicate.EqualToNumeric              (key, value) -> AlgoUntypedPredicate.InNumeric      (key, [value], None)
        | UntypedPredicate.EqualToString               (key, value) -> AlgoUntypedPredicate.InString        (key, [value], None)
        | UntypedPredicate.GreaterThanNumeric          (key, value) -> AlgoUntypedPredicate.BetweenNumeric (key, PredicateBound.Exclusive value, PredicateBound.Unbounded,       None) // >
        | UntypedPredicate.GreaterThanOrEqualToNumeric (key, value) -> AlgoUntypedPredicate.BetweenNumeric (key, PredicateBound.Inclusive value, PredicateBound.Unbounded,       None) // >=
        | UntypedPredicate.LessThanNumeric             (key, value) -> AlgoUntypedPredicate.BetweenNumeric (key, PredicateBound.Unbounded,       PredicateBound.Exclusive value, None) // <
        | UntypedPredicate.LessThanOrEqualToNumeric    (key, value) -> AlgoUntypedPredicate.BetweenNumeric (key, PredicateBound.Unbounded,       PredicateBound.Inclusive value, None) // <=
        | UntypedPredicate.GreaterThanString           (key, value) -> AlgoUntypedPredicate.BetweenString  (key, PredicateBound.Exclusive value, PredicateBound.Unbounded,       None) // >
        | UntypedPredicate.GreaterThanOrEqualToString  (key, value) -> AlgoUntypedPredicate.BetweenString  (key, PredicateBound.Inclusive value, PredicateBound.Unbounded,       None) // >=
        | UntypedPredicate.LessThanString              (key, value) -> AlgoUntypedPredicate.BetweenString  (key, PredicateBound.Unbounded,       PredicateBound.Exclusive value, None) // <
        | UntypedPredicate.LessThanOrEqualToString     (key, value) -> AlgoUntypedPredicate.BetweenString  (key, PredicateBound.Unbounded,       PredicateBound.Inclusive value, None) // <=
        | UntypedPredicate.StartsWith             (key, startsWith) -> AlgoUntypedPredicate.StartsWith     (key, startsWith, None)
        | UntypedPredicate.Matches                (key, keywords)       -> AlgoUntypedPredicate.Matches        (key, keywords)
        | UntypedPredicate.MatchesExact           (key, keywords)       -> AlgoUntypedPredicate.MatchesExact   (key, keywords)
        | UntypedPredicate.MatchesPrefix          (key, keywordsPrefix) -> AlgoUntypedPredicate.MatchesPrefix  (key, keywordsPrefix)
        | UntypedPredicate.IntersectsGeography (key, value) -> AlgoUntypedPredicate.IntersectsGeography  (key, value)
        | UntypedPredicate.Diff (left, right) -> AlgoUntypedPredicate.Diff (queryToAlgoInput left, queryToAlgoInput right, None)
        | UntypedPredicate.And  (left, right) ->
            let expandAndGroup node =
                match queryToAlgoInput node with
                | AlgoUntypedPredicate.AndGroup (group, _) ->
                    group
                | other ->
                    Set.ofOneItem other
            AlgoUntypedPredicate.AndGroup (Set.union (expandAndGroup left) (expandAndGroup right) |> reduceBetweensInAndGroup, None)
        | UntypedPredicate.Or   (left, right) ->
            let expandOrGroup node =
                match queryToAlgoInput node with
                    | AlgoUntypedPredicate.OrGroup (group, _) ->
                        group
                    | other ->
                        Set.ofOneItem other
            AlgoUntypedPredicate.OrGroup (Set.union (expandOrGroup left) (expandOrGroup right) |> reduceInsInOrGroup, None)

    let rec algoOutputToOptimizedQuery root =
        match root with
        | AlgoUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, maybePromotion) -> OptimizedUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, maybePromotion |> Option.map fst)
        | AlgoUntypedPredicate.BetweenString  (key, lowerBound, upperBound, maybePromotion) -> OptimizedUntypedPredicate.BetweenString  (key, lowerBound, upperBound, maybePromotion |> Option.map fst)
        | AlgoUntypedPredicate.InNumeric      (key, values, maybePromotion)                 -> OptimizedUntypedPredicate.InNumeric      (key, values,                 maybePromotion |> Option.map fst)
        | AlgoUntypedPredicate.InString       (key, values, maybePromotion)                 -> OptimizedUntypedPredicate.InString       (key, values,                 maybePromotion |> Option.map fst)
        | AlgoUntypedPredicate.StartsWith     (key, startsWith, maybePromotion)             -> OptimizedUntypedPredicate.StartsWith     (key, startsWith,             maybePromotion |> Option.map fst)
        | AlgoUntypedPredicate.Matches        (key, keywords)                               -> OptimizedUntypedPredicate.Matches        (key, keywords)
        | AlgoUntypedPredicate.MatchesExact   (key, keywords)                               -> OptimizedUntypedPredicate.MatchesExact   (key, keywords)
        | AlgoUntypedPredicate.MatchesPrefix  (key, keywordsPrefix)                         -> OptimizedUntypedPredicate.MatchesPrefix  (key, keywordsPrefix)
        | AlgoUntypedPredicate.IntersectsGeography (key, value)                             -> OptimizedUntypedPredicate.IntersectsGeography     (key, value)
        | AlgoUntypedPredicate.Diff (left, right, _) -> OptimizedUntypedPredicate.Diff (algoOutputToOptimizedQuery left, algoOutputToOptimizedQuery right)
        | AlgoUntypedPredicate.AndGroup (group, _) ->
            match group |> Set.toList with
            | [] ->
                failwith "should be unreachable"
            | [singleNode] ->
                algoOutputToOptimizedQuery singleNode
            | node :: tail ->
                OptimizedUntypedPredicate.And(algoOutputToOptimizedQuery node, algoOutputToOptimizedQuery <| AlgoUntypedPredicate.AndGroup (tail |> Set.ofList, None))
        | AlgoUntypedPredicate.OrGroup (group, _) ->
                match group |> Set.toList with
                | [] ->
                    failwith "should be unreachable"
                | [singleNode] ->
                    algoOutputToOptimizedQuery singleNode
                | node :: tail ->
                    OptimizedUntypedPredicate.Or(algoOutputToOptimizedQuery node, algoOutputToOptimizedQuery <| AlgoUntypedPredicate.OrGroup (tail |> Set.ofList, None))

    let rec getPotentialPromotables root =
        match root with
        | AlgoUntypedPredicate.InNumeric (key, [value], _) -> if allPromotedBaseKeys.Contains (BaseKey key) then Set.ofOneItem (BaseKey key, BaseValue (string value)) else Set.empty
        | AlgoUntypedPredicate.InString  (key, [value], _) -> if allPromotedBaseKeys.Contains (BaseKey key) then Set.ofOneItem (BaseKey key, BaseValue value)          else Set.empty
        | AlgoUntypedPredicate.Diff      (left, _right, _) -> getPotentialPromotables left
        | AlgoUntypedPredicate.AndGroup         (group, _) -> group |> Set.map getPotentialPromotables |> Set.unionMany
        | AlgoUntypedPredicate.OrGroup          (group, _) -> group |> Set.map getPotentialPromotables |> Set.intersectMany
        | AlgoUntypedPredicate.BetweenNumeric _
        | AlgoUntypedPredicate.BetweenString _
        | AlgoUntypedPredicate.InNumeric _
        | AlgoUntypedPredicate.InString _
        | AlgoUntypedPredicate.StartsWith _
        | AlgoUntypedPredicate.Matches _
        | AlgoUntypedPredicate.MatchesExact _
        | AlgoUntypedPredicate.MatchesPrefix _
        | AlgoUntypedPredicate.IntersectsGeography _ -> Set.empty

    // trim any node from an andGroup where the node is EqualTo, it's key has been promoted, and there is a sibling node that will generate SQL that uses the promotion
    // avoid edge case where only sibling node of trimmable node is an Or, or a text search Matches, or other node that can't use any promoted conditions
    // simplicity of this trimming code is only reason And's are grouped in the first place - original nested structure makes it very difficult to determine if node can be safely trimmed or not
    // TODO this is not optimal and may fail to trim in some cases (query still correct just sub-optimal), needs a different trimming algo but not high priority
    let trimAndGroup chosenPromotion group =
        let promotedBaseKeys = chosenPromotion |> Option.mapOrElse Set.empty snd

        let trim group promotedBaseKey =
            let maybeTrimmable =
                group
                |> Set.filter (
                    function
                    | AlgoUntypedPredicate.InNumeric (key, [_], _)
                    | AlgoUntypedPredicate.InString (key, [_], _) when BaseKey key = promotedBaseKey -> true | _ -> false)
                |> Set.toSeq
                |> Seq.tryHead

            match maybeTrimmable with
            | None ->
                group

            | Some trimmable ->
                let rec canUsePromotion node =
                    match node with
                    | AlgoUntypedPredicate.Diff     (left, _right, _) -> canUsePromotion left
                    | AlgoUntypedPredicate.AndGroup (group, _)        -> group |> Seq.map canUsePromotion |> Seq.reduce (||)
                    | AlgoUntypedPredicate.OrGroup  (group, _)        -> group |> Seq.map canUsePromotion |> Seq.reduce (&&)
                    | AlgoUntypedPredicate.BetweenNumeric _
                    | AlgoUntypedPredicate.BetweenString _
                    | AlgoUntypedPredicate.InNumeric _
                    | AlgoUntypedPredicate.InString _
                    | AlgoUntypedPredicate.StartsWith _ -> true
                    | AlgoUntypedPredicate.Matches _
                    | AlgoUntypedPredicate.MatchesPrefix _
                    | AlgoUntypedPredicate.MatchesExact _
                    | AlgoUntypedPredicate.IntersectsGeography _ -> false

                let otherGroupNodesThatUsePromotion =
                    group
                    |> Set.remove trimmable
                    |> Set.filter canUsePromotion

                if otherGroupNodesThatUsePromotion.IsNonempty then
                    group |> Set.remove trimmable
                else
                    group

        promotedBaseKeys
        |> Set.fold trim group

    let rec choosePromotions maybeParentChosenPromotion root =
        let chosenPromotion =
            match maybeParentChosenPromotion with
            | Some parentChosenPromotion -> Some parentChosenPromotion
            | None -> root |> getPotentialPromotables |> Set.toSeq |> Map.ofSeq |> bestPromotion

        match root with
        | AlgoUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, None) -> AlgoUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, chosenPromotion)
        | AlgoUntypedPredicate.BetweenString  (key, lowerBound, upperBound, None) -> AlgoUntypedPredicate.BetweenString  (key, lowerBound, upperBound, chosenPromotion)
        | AlgoUntypedPredicate.InNumeric      (key, values,                 None) -> AlgoUntypedPredicate.InNumeric      (key, values,                 chosenPromotion)
        | AlgoUntypedPredicate.InString       (key, values,                 None) -> AlgoUntypedPredicate.InString       (key, values,                 chosenPromotion)
        | AlgoUntypedPredicate.StartsWith     (key, keywords,               None) -> AlgoUntypedPredicate.StartsWith     (key, keywords,               chosenPromotion)

        | AlgoUntypedPredicate.Diff (left, right, None) -> AlgoUntypedPredicate.Diff (choosePromotions chosenPromotion left, choosePromotions chosenPromotion right, chosenPromotion)

        | AlgoUntypedPredicate.AndGroup (group, None) -> AlgoUntypedPredicate.AndGroup (group |> Set.map (choosePromotions chosenPromotion) |> trimAndGroup chosenPromotion, chosenPromotion)
        | AlgoUntypedPredicate.OrGroup  (group, None) -> AlgoUntypedPredicate.OrGroup  (group |> Set.map (choosePromotions chosenPromotion),                                 chosenPromotion)

        // node already has chosen promotion, or we don't choose promotions for that node type
        | AlgoUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, Some promotion) -> AlgoUntypedPredicate.BetweenNumeric (key, lowerBound, upperBound, Some promotion)
        | AlgoUntypedPredicate.BetweenString  (key, lowerBound, upperBound, Some promotion) -> AlgoUntypedPredicate.BetweenString  (key, lowerBound, upperBound, Some promotion)
        | AlgoUntypedPredicate.InNumeric      (key, values,                 Some promotion) -> AlgoUntypedPredicate.InNumeric      (key, values,                 Some promotion)
        | AlgoUntypedPredicate.InString       (key, values,                 Some promotion) -> AlgoUntypedPredicate.InString       (key, values,                 Some promotion)
        | AlgoUntypedPredicate.StartsWith     (key, keywords,               Some promotion) -> AlgoUntypedPredicate.StartsWith     (key, keywords,               Some promotion)
        | AlgoUntypedPredicate.AndGroup       (group,                       Some promotion) -> AlgoUntypedPredicate.AndGroup       (group,                       Some promotion)
        | AlgoUntypedPredicate.OrGroup        (group,                       Some promotion) -> AlgoUntypedPredicate.OrGroup        (group,                       Some promotion)
        | AlgoUntypedPredicate.Diff           (left, right,                 Some promotion) -> AlgoUntypedPredicate.Diff           (left, right,                 Some promotion)
        | AlgoUntypedPredicate.Matches       (key, keywords)       -> AlgoUntypedPredicate.Matches       (key, keywords)
        | AlgoUntypedPredicate.MatchesExact  (key, keywords)       -> AlgoUntypedPredicate.MatchesExact  (key, keywords)
        | AlgoUntypedPredicate.MatchesPrefix (key, keywordsPrefix) -> AlgoUntypedPredicate.MatchesPrefix (key, keywordsPrefix)
        | AlgoUntypedPredicate.IntersectsGeography (key, value) -> AlgoUntypedPredicate.IntersectsGeography (key, value)

    query
    |> queryToAlgoInput
    |> choosePromotions None
    |> algoOutputToOptimizedQuery

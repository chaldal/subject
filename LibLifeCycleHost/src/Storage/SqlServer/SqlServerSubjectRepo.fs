namespace LibLifeCycleHost.Storage.SqlServer

open System
open LibLifeCycle.LifeCycles.Meta
open LibLifeCycleTypes

#nowarn "0686"

open System.Threading.Tasks
open Microsoft.Data.SqlClient
open FSharp.Control
open LibLifeCycle
open LibLifeCycleHost
open System.Data
open Microsoft.SqlServer.Server

module private Compression =
    let inline ofCompressedJsonText (bytes: byte[]) = DataEncode.ofCompressedJsonText bytes

    let inline ofCompressedJsonTextWithContextInfoInError (lifeCycleName: string) (pKey: string) (bytes: byte[]) =
        match DataEncode.ofCompressedJsonText bytes with
        | Ok x -> x
        | Error err ->
            InvalidOperationException (sprintf "Unable to decode subject: %s, %s ; Error: %A" lifeCycleName pKey err)
            |> raise

[<RequireQualifiedAccess>]
type private ValueType =
    | Int
    | Str
    with
        member this.ColumnName =
            match this with
            | Int -> "ValueInt"
            | Str -> "ValueStr"
        member this.OtherColumnName =
            match this with
            | Int -> Str.ColumnName
            | Str -> Int.ColumnName

type SqlServerSubjectRepo<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId
                   when 'Subject              :> Subject<'SubjectId>
                   and  'LifeAction           :> LifeAction
                   and  'OpError              :> OpError
                   and  'Constructor          :> Constructor
                   and  'LifeEvent            :> LifeEvent
                   and  'LifeEvent            : comparison
                   and  'SubjectIndex         :> SubjectIndex<'OpError>
                   and  'SubjectId            :> SubjectId
                   and  'SubjectId            : comparison>
    (
        lifeCycleAdapter: HostedLifeCycleAdapter<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>,
        connStrings: SqlServerConnectionStrings
    ) as this =

    let ecosystemName = lifeCycleAdapter.LifeCycle.Def.LifeCycleKey.EcosystemName
    let sqlConnectionString = connStrings.ForEcosystem ecosystemName

    let generateInSql (valueType: ValueType) (valueParamsAndValues: List<string * obj>) =
        let inValues =
            valueParamsAndValues
            |> List.map fst
            |> String.concat ", "
        $"{valueType.ColumnName} IN ({inValues})"

    let generateSqlForSingleValue
        (key: string)
        (valueType: ValueType)
        (operator: string)
        (value: 'Value)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = $"@K{paramGroupSuffix}"
        let valueParam = $"@S{paramGroupSuffix}"

        let updatedParams = inputParameters.Add(keyParam, key).Add(valueParam, value)
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_Index]
                WHERE [Key] = {keyParam} AND SubjectId <> '' AND [{valueType.OtherColumnName}] IS NULL AND [{valueType.ColumnName}] {operator} {valueParam}"

        sql, updatedParams

    let generateSqlForBetweenValues
        (key: string)
        (valueType: ValueType)
        (lowerOp: string)
        (lowerValue: 'Value)
        (upperOp: string)
        (upperValue: 'Value)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = $"@K{paramGroupSuffix}"
        let lowerParam = $"@SL{paramGroupSuffix}"
        let upperParam = $"@SU{paramGroupSuffix}"

        let updatedParams = inputParameters.Add(keyParam, key).Add(lowerParam, lowerValue).Add(upperParam, upperValue)
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_Index]
                WHERE [Key] = {keyParam} AND SubjectId <> '' AND [{valueType.OtherColumnName}] IS NULL AND [{valueType.ColumnName}] {lowerOp} {lowerParam} AND [{valueType.ColumnName}] {upperOp} {upperParam}"

        sql, updatedParams

    let generateSqlForInValues
        (key: string)
        (valueType: ValueType)
        (values: List<'Value>)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = $"@K{paramGroupSuffix}"
        let valueParamsAndValues =
            values
            |> List.mapi (fun i value -> $"@N{i}S{paramGroupSuffix}", box value)

        let updatedParams = inputParameters.Add(keyParam, key) |> Map.merge (Map.ofList valueParamsAndValues)

        let inSql = generateInSql valueType valueParamsAndValues

        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_Index]
                WHERE [Key] = {keyParam} AND SubjectId <> '' AND [{valueType.OtherColumnName}] IS NULL AND {inSql}"

        sql, updatedParams

    let generateSqlForSingleValueWithPromotion
        (key: string)
        (valueType: ValueType)
        (operator: string)
        (value: 'Value)
        (promotedKey: string)
        (promotedValue: string)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>) =

        let keyParam = $"@K{paramGroupSuffix}"
        let valueParam = $"@S{paramGroupSuffix}"
        let promotedValueParam = $"@PV{paramGroupSuffix}"

        let updatedParams = inputParameters.Add(keyParam, key).Add(valueParam, value).Add(promotedValueParam, promotedValue)
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_Index_{promotedKey}]
                WHERE [Key] = {keyParam} AND [{valueType.OtherColumnName}] IS NULL AND [{valueType.ColumnName}] {operator} {valueParam} AND [PromotedValue] = {promotedValueParam}"

        sql, updatedParams

    let generateSqlForBetweenValuesWithPromotion
        (key: string)
        (valueType: ValueType)
        (lowerOp: string)
        (lowerValue: 'Value)
        (upperOp: string)
        (upperValue: 'Value)
        (promotedKey: string)
        (promotedValue: string)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>) =

        let keyParam = $"@K{paramGroupSuffix}"
        let lowerValueParam = $"@SL{paramGroupSuffix}"
        let upperValueParam = $"@SU{paramGroupSuffix}"
        let promotedValueParam = $"@PV{paramGroupSuffix}"

        let updatedParams = inputParameters.Add(keyParam, key).Add(lowerValueParam, lowerValue).Add(upperValueParam, upperValue).Add(promotedValueParam, promotedValue)
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_Index_{promotedKey}]
                WHERE [Key] = {keyParam} AND [{valueType.OtherColumnName}] IS NULL AND [{valueType.ColumnName}] {lowerOp} {lowerValueParam} AND [{valueType.ColumnName}] {upperOp} {upperValueParam} AND [PromotedValue] = {promotedValueParam}"

        sql, updatedParams

    let generateSqlForInValuesWithPromotion
        (key: string)
        (valueType: ValueType)
        (values: List<'Value>)
        (promotedKey: string)
        (promotedValue: string)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = sprintf "@K%d" paramGroupSuffix
        let promotedValueParam = sprintf "@PV%d" paramGroupSuffix
        let valueParamsAndValues =
            values
            |> List.mapi (fun i value -> $"@N{i}S{paramGroupSuffix}", box value)

        let updatedParams = inputParameters.Add(keyParam, key).Add(promotedValueParam, promotedValue) |> Map.merge (Map.ofList valueParamsAndValues)

        let inSql = generateInSql valueType valueParamsAndValues

        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_Index_{promotedKey}]
                WHERE [Key] = {keyParam} AND [{valueType.OtherColumnName}] IS NULL AND {inSql} AND [PromotedValue] = {promotedValueParam}"

        sql, updatedParams

    let generateSqlForSearchValueFreeText
        (key: string)
        (value: string)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = sprintf "@K%d" paramGroupSuffix
        let strParam = sprintf "@S%d" paramGroupSuffix

        let updatedParams = inputParameters.Add(keyParam, key).Add(strParam, value)
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_SearchIndex]
                WHERE [Key] = {keyParam} AND FREETEXT([{ValueType.Str.ColumnName}], {strParam})
                "

        sql, updatedParams

    let generateSqlForSearchValueExact
        (key: string)
        (value: string)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = sprintf "@K%d" paramGroupSuffix
        let strParam = sprintf "@S%d" paramGroupSuffix

        // CONTAINS crashes with quotation marks inside, so it's removed. FULLTEXT doesn't index punctuation anyway.
        let sanitizedValue = value.Replace("\"", "")
        let updatedParams = inputParameters.Add(keyParam, key).Add(strParam, $"\"%s{sanitizedValue}\"")
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_SearchIndex]
                WHERE [Key] = {keyParam} AND CONTAINS([{ValueType.Str.ColumnName}], {strParam})
                "

        sql, updatedParams

    let generateSqlForSearchValuePrefix
        (key: string)
        (prefix: string)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = sprintf "@K%d" paramGroupSuffix
        let strParam = sprintf "@S%d" paramGroupSuffix

        // CONTAINS crashes with quotation marks inside, so it's removed. FULLTEXT doesn't index punctuation anyway.
        let sanitizedPrefix = prefix.Replace("\"", "")
        let updatedParams = inputParameters.Add(keyParam, key).Add(strParam, $"\"%s{sanitizedPrefix}*\"")
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_SearchIndex]
                WHERE [Key] = {keyParam} AND CONTAINS([{ValueType.Str.ColumnName}], {strParam})
                "

        sql, updatedParams

    let generateSqlForGeographyBoolMethod
        (key: string)
        (method: string)
        (value: GeographyIndexValue)
        (paramGroupSuffix: int)
        (inputParameters: Map<string, obj>)
        : (string * Map<string, obj>) =

        let keyParam = $"@K{paramGroupSuffix}"
        let valueParam = $"@S{paramGroupSuffix}"

        let updatedParams = inputParameters.Add(keyParam, key).Add(valueParam, value.ToWkt())
        let sql =
            $"SELECT DISTINCT SubjectId
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_GeographyIndex]
                WHERE (ValueGeography.{method}(CONVERT(geography, {valueParam}))) = 1 AND [Key] = {keyParam}"

        sql, updatedParams

    let rec generateSqlForIndexQueryNode
            (indexQuery: OptimizedUntypedPredicate)
            (excludeTopLevelSearchIndex: bool) // if already part of sortTable inner join, no need to search twice
            (isTopLevelAnd: bool)
            (paramGroupSuffix: int)
            (inputParameters: Map<string, obj>)
            : string * Map<string, obj> * int =

        let simpleQuery (key: string) (valueType: ValueType) (operator: string) (value: 'Value) =
            generateSqlForSingleValue key valueType operator value paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let betweenQuery (key: string) (valueType: ValueType) (lowerOp: string) (lowerValue: 'Value) (upperOp: string) (upperValue: 'Value) =
            generateSqlForBetweenValues key valueType lowerOp lowerValue upperOp upperValue paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let inQuery (key: string) (valueType: ValueType) (values: List<'Value>) =
            generateSqlForInValues key valueType values paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let simpleQueryWithPromotion (key: string) (valueType: ValueType) (operator: string) (value: 'Value) (PromotedKey promotedKey) (PromotedValue promotedValue) =
            generateSqlForSingleValueWithPromotion key valueType operator value promotedKey promotedValue paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let betweenQueryWithPromotion (key: string) (valueType: ValueType) (lowerOp: string) (lowerValue: 'Value) (upperOp: string) (upperValue: 'Value) (PromotedKey promotedKey) (PromotedValue promotedValue) =
            generateSqlForBetweenValuesWithPromotion key valueType lowerOp lowerValue upperOp upperValue promotedKey promotedValue paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let inQueryWithPromotion (key: string) (valueType: ValueType) (values: List<'Value>) (PromotedKey promotedKey) (PromotedValue promotedValue) =
            generateSqlForInValuesWithPromotion key valueType values promotedKey promotedValue paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let dummyAllSubjectsQuery () =
            let sql = sprintf "SELECT [Id] AS SubjectId FROM [%s].[%s]" ecosystemName lifeCycleAdapter.LifeCycle.Name
            sql, inputParameters, paramGroupSuffix

        let freeTextSearchQuery (key: string) (value: string) =
            generateSqlForSearchValueFreeText key value paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let exactSearchQuery (key: string) (value: string) =
            generateSqlForSearchValueExact key value paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let geographyBoolQuery (key: string) (method: string) (value: GeographyIndexValue) =
            generateSqlForGeographyBoolMethod key method value paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let prefixSearchQuery (key: string) (value: string) =
            generateSqlForSearchValuePrefix key value paramGroupSuffix inputParameters
            |> fun(sql, inputParameters) -> sql, inputParameters, (paramGroupSuffix + 1)

        let combineQueries (isTopLevelAnd: bool) (complexOperator: string) indexQuery1 indexQuery2 =
            let excludeTopLevelSearchIndex = excludeTopLevelSearchIndex && isTopLevelAnd

            let leftSql, leftParams, nextParamGroupSuffix =
                generateSqlForIndexQueryNode indexQuery1 excludeTopLevelSearchIndex isTopLevelAnd paramGroupSuffix inputParameters

            let rightSql, rightParams, nextParamGroupSuffix =
                generateSqlForIndexQueryNode indexQuery2 excludeTopLevelSearchIndex isTopLevelAnd nextParamGroupSuffix inputParameters

            let combinedSql = (sprintf "(%s) %s (%s)" leftSql complexOperator rightSql)
            (combinedSql, (Map.merge leftParams rightParams), nextParamGroupSuffix)

        match indexQuery with
        | OptimizedUntypedPredicate.StartsWith     (key, value, None) -> simpleQuery key ValueType.Str "LIKE" (value + "%")

        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Exclusive value,      PredicateBound.Unbounded,            None) -> simpleQuery key ValueType.Int ">"  value
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Inclusive value,      PredicateBound.Unbounded,            None) -> simpleQuery key ValueType.Int ">=" value
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Unbounded,            PredicateBound.Exclusive value,      None) -> simpleQuery key ValueType.Int "<"  value
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Unbounded,            PredicateBound.Inclusive value,      None) -> simpleQuery key ValueType.Int "<=" value
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Exclusive  value,     PredicateBound.Unbounded,            None) -> simpleQuery key ValueType.Str ">"  value
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Inclusive  value,     PredicateBound.Unbounded,            None) -> simpleQuery key ValueType.Str ">=" value
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Unbounded,            PredicateBound.Exclusive  value,     None) -> simpleQuery key ValueType.Str "<"  value
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Unbounded,            PredicateBound.Inclusive  value,     None) -> simpleQuery key ValueType.Str "<=" value

        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Exclusive lowerValue, PredicateBound.Exclusive upperValue, None) -> betweenQuery key ValueType.Int ">"  lowerValue "<"  upperValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Inclusive lowerValue, PredicateBound.Inclusive upperValue, None) -> betweenQuery key ValueType.Int ">=" lowerValue "<=" upperValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Inclusive lowerValue, PredicateBound.Exclusive upperValue, None) -> betweenQuery key ValueType.Int ">=" lowerValue "<"  upperValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Exclusive lowerValue, PredicateBound.Inclusive upperValue, None) -> betweenQuery key ValueType.Int ">"  lowerValue "<=" upperValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Exclusive lowerValue, PredicateBound.Exclusive upperValue, None) -> betweenQuery key ValueType.Str ">"  lowerValue "<"  upperValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Inclusive lowerValue, PredicateBound.Inclusive upperValue, None) -> betweenQuery key ValueType.Str ">=" lowerValue "<=" upperValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Inclusive lowerValue, PredicateBound.Exclusive upperValue, None) -> betweenQuery key ValueType.Str ">=" lowerValue "<"  upperValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Exclusive lowerValue, PredicateBound.Inclusive upperValue, None) -> betweenQuery key ValueType.Str ">"  lowerValue "<=" upperValue

        | OptimizedUntypedPredicate.InNumeric (key, values, None) -> inQuery key ValueType.Int values
        | OptimizedUntypedPredicate.InString  (key, values, None) -> inQuery key ValueType.Str values


        | OptimizedUntypedPredicate.StartsWith     (key, value, Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Str "LIKE" (value + "%") promotedKey promotedValue

        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Exclusive value,      PredicateBound.Unbounded,            Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Int ">"  value promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Inclusive value,      PredicateBound.Unbounded,            Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Int ">=" value promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Unbounded,            PredicateBound.Exclusive value,      Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Int "<"  value promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Unbounded,            PredicateBound.Inclusive value,      Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Int "<=" value promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Exclusive value,      PredicateBound.Unbounded,            Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Str ">"  value promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Inclusive value,      PredicateBound.Unbounded,            Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Str ">=" value promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Unbounded,            PredicateBound.Exclusive value,      Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Str "<"  value promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Unbounded,            PredicateBound.Inclusive value,      Some (promotedKey, promotedValue)) -> simpleQueryWithPromotion key ValueType.Str "<=" value promotedKey promotedValue

        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Exclusive lowerValue, PredicateBound.Exclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Int ">"  lowerValue "<"  upperValue promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Inclusive lowerValue, PredicateBound.Inclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Int ">=" lowerValue "<=" upperValue promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Inclusive lowerValue, PredicateBound.Exclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Int ">=" lowerValue "<"  upperValue promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenNumeric (key, PredicateBound.Exclusive lowerValue, PredicateBound.Inclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Int ">"  lowerValue "<=" upperValue promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Exclusive lowerValue, PredicateBound.Exclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Str ">"  lowerValue "<"  upperValue promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Inclusive lowerValue, PredicateBound.Inclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Str ">=" lowerValue "<=" upperValue promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Inclusive lowerValue, PredicateBound.Exclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Str ">=" lowerValue "<"  upperValue promotedKey promotedValue
        | OptimizedUntypedPredicate.BetweenString  (key, PredicateBound.Exclusive lowerValue, PredicateBound.Inclusive upperValue, Some (promotedKey, promotedValue)) -> betweenQueryWithPromotion key ValueType.Str ">"  lowerValue "<=" upperValue promotedKey promotedValue

        | OptimizedUntypedPredicate.InNumeric (key, values, Some (promotedKey, promotedValue)) -> inQueryWithPromotion key ValueType.Int values promotedKey promotedValue
        | OptimizedUntypedPredicate.InString  (key, values, Some (promotedKey, promotedValue)) -> inQueryWithPromotion key ValueType.Str values promotedKey promotedValue


        | OptimizedUntypedPredicate.BetweenNumeric (_, PredicateBound.Unbounded, PredicateBound.Unbounded, _)
        | OptimizedUntypedPredicate.BetweenString  (_, PredicateBound.Unbounded, PredicateBound.Unbounded, _) -> failwith "Bad query, unbounded Between are invalid"

        // special cases where top-level search scope should be skipped
        | OptimizedUntypedPredicate.Matches _
            when isTopLevelAnd && excludeTopLevelSearchIndex        -> dummyAllSubjectsQuery ()
        | OptimizedUntypedPredicate.And (OptimizedUntypedPredicate.Matches _, p)
        | OptimizedUntypedPredicate.And (p, OptimizedUntypedPredicate.Matches _)
            when isTopLevelAnd && excludeTopLevelSearchIndex        -> generateSqlForIndexQueryNode p excludeTopLevelSearchIndex isTopLevelAnd paramGroupSuffix inputParameters
        | OptimizedUntypedPredicate.Matches (key, value)                     -> freeTextSearchQuery key value
        | OptimizedUntypedPredicate.MatchesExact (key, value)                -> exactSearchQuery key value
        | OptimizedUntypedPredicate.MatchesPrefix (key, value)               -> prefixSearchQuery key value
        | OptimizedUntypedPredicate.IntersectsGeography (key, value)         -> geographyBoolQuery key "STIntersects" value
        | OptimizedUntypedPredicate.And (left, right)                        -> combineQueries isTopLevelAnd             "INTERSECT" left right
        | OptimizedUntypedPredicate.Or (left, right)                         -> combineQueries (* isTopLevelAnd *) false "UNION"     left right
        | OptimizedUntypedPredicate.Diff (left, right)                       -> combineQueries (* isTopLevelAnd *) false "EXCEPT"    left right

    let tryFindExactlyOneTopLevelFreeTextMatchArgs =
        let rec flatTopMatches p =
            seq {
                match p with
                | UntypedPredicate.Matches (key, keywords) ->
                    yield (key, keywords)
                | UntypedPredicate.And (pl, pr) ->
                    yield! flatTopMatches pl
                    yield! flatTopMatches pr
                | _ ->
                    ()
            }
        flatTopMatches >> Seq.tryExactlyOne

    let tryFindExactlyOneTopLevelContainsMatchArgs =
        let rec flatTopMatches p =
            seq {
                match p with
                | UntypedPredicate.MatchesExact (key, keywords) ->
                    yield (key, keywords)
                | UntypedPredicate.And (pl, pr) ->
                    yield! flatTopMatches pl
                    yield! flatTopMatches pr
                | _ ->
                    ()
            }
        flatTopMatches >> Seq.tryExactlyOne

    let tryFindExactlyOneTopLevelContainsMatchPrefixArgs =
        let rec flatTopMatchesPrefix p =
            seq {
                match p with
                | UntypedPredicate.MatchesPrefix (key, keywordsPrefix) ->
                    yield (key, keywordsPrefix)
                | UntypedPredicate.And (pl, pr) ->
                    yield! flatTopMatchesPrefix pl
                    yield! flatTopMatchesPrefix pr
                | _ ->
                    ()
            }
        flatTopMatchesPrefix >> Seq.tryExactlyOne

    let generateSortTableJoinOrderByAndParams
        (resultSetOptions: UntypedResultSetOptions)
        (maybeTopFreeTextParams: Option<string * string>)
        (maybeTopContainsParams: Option<string * string>)
        (maybeTopContainsPrefixParams: Option<string * string>) =

        // CONTAINSTABLE crashes with quotation marks inside, so it's removed. FULLTEXT doesn't index punctuation anyway.
        let sanitize (input: string) = input.Replace("\"", "")

        match resultSetOptions.OrderBy with
        | UntypedOrderBy.FastestOrSingleSearchScoreIfAvailable ->
            match maybeTopFreeTextParams, maybeTopContainsParams, maybeTopContainsPrefixParams with
            | None, None, None ->
                "",
                "stateTable.Id", // Ordering by ID has no added cost
                []

            | Some (freeTxtKey, freeTxtKeyword), None, None ->
                sprintf
                    // GROUP BY is needed because FREETEXTTABLE can return duplicate rows when
                    // underlying catalog becomes slightly corrupt due to hight update rate
                    "INNER JOIN (
                        SELECT searchIndexTbl.SubjectId, MAX(freeTxtResult.[RANK]) AS SearchRank
                        FROM FREETEXTTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreFreeTxtKeyword) freeTxtResult
                        JOIN [%s].[%s_SearchIndex] searchIndexTbl ON searchIndexTbl.Id = freeTxtResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreFreeTxtKey
                        GROUP BY searchIndexTbl.SubjectId) sortTable
                    ON stateTable.Id = sortTable.SubjectId"
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name,
                "sortTable.SearchRank DESC, stateTable.Id", // first by rank then by Id to make stable order
                [
                    ("@SearchScoreFreeTxtKeyword", freeTxtKeyword)
                    ("@SearchScoreFreeTxtKey", freeTxtKey)
                ]

            | None, Some (containsKey, containsValue), None ->
                let sanitizedValue = sanitize containsValue
                sprintf
                    // TODO does same duplicate rows issue apply to CONTAINSTABLE?
                    "INNER JOIN (
                        SELECT searchIndexTbl.SubjectId, MAX(containsResult.[RANK]) AS ContainsRank
                        FROM CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreContains) containsResult
                        JOIN [%s].[%s_SearchIndex] searchIndexTbl ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreContainsKey
                        GROUP BY searchIndexTbl.SubjectId) sortTable
                    ON stateTable.Id = sortTable.SubjectId"
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name,
                "sortTable.ContainsRank DESC, stateTable.Id", // first by rank then by Id to make stable order
                [
                    ("@SearchScoreContains", $"\"%s{sanitizedValue}\"")
                    ("@SearchScoreContainsKey", containsKey)
                ]

            | None, None, Some (prefixKey, prefixValue) ->
                let sanitizedPrefix = sanitize prefixValue
                sprintf
                    // TODO does same duplicate rows issue apply to CONTAINSTABLE?
                    "INNER JOIN (
                        SELECT searchIndexTbl.SubjectId, MAX(containsResult.[RANK]) AS ContainsRank
                        FROM CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScorePrefix) containsResult
                        JOIN [%s].[%s_SearchIndex] searchIndexTbl ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScorePrefixKey
                        GROUP BY searchIndexTbl.SubjectId) sortTable
                    ON stateTable.Id = sortTable.SubjectId"
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name,
                "sortTable.ContainsRank DESC, stateTable.Id", // first by rank then by Id to make stable order
                [
                    ("@SearchScorePrefix", $"\"%s{sanitizedPrefix}*\"")
                    ("@SearchScorePrefixKey", prefixKey)
                ]

            | Some (freeTxtKey, freeTxtKeyword), Some (containsKey, containsValue), None ->
                let sanitizedValue = sanitize containsValue
                sprintf
                    "INNER JOIN (
                        SELECT searchIndexTbl.SubjectId, MAX(freeTxtResult.[RANK]) AS FreeTxtRank, MAX(containsResult.[RANK]) AS ContainsRank
                        FROM [%s].[%s_SearchIndex] searchIndexTbl
                        JOIN FREETEXTTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreFreeTxtKeyword) freeTxtResult
                            ON searchIndexTbl.Id = freeTxtResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreFreeTxtKey
                        JOIN CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreContains) containsResult
                            ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreContainsKey
                        GROUP BY searchIndexTbl.SubjectId) sortTable
                    ON stateTable.Id = sortTable.SubjectId"
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name,
                "sortTable.FreeTxtRank DESC, sortTable.ContainsRank DESC, stateTable.Id", // first by ranks then by Id to make stable order
                [
                    ("@SearchScoreFreeTxtKeyword", freeTxtKeyword)
                    ("@SearchScoreFreeTxtKey", freeTxtKey)
                    ("@SearchScoreContains", $"\"%s{sanitizedValue}\"")
                    ("@SearchScoreContainsKey", containsKey)
                ]

            | Some (freeTxtKey, freeTxtKeyword), None, Some (prefixKey, prefixValue) ->
                let sanitizedPrefix = sanitize prefixValue
                sprintf
                    "INNER JOIN (
                        SELECT searchIndexTbl.SubjectId, MAX(freeTxtResult.[RANK]) AS FreeTxtRank, MAX(containsResult.[RANK]) AS ContainsRank
                        FROM [%s].[%s_SearchIndex] searchIndexTbl
                        JOIN FREETEXTTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreFreeTxtKeyword) freeTxtResult
                            ON searchIndexTbl.Id = freeTxtResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreFreeTxtKey
                        JOIN CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScorePrefix) containsResult
                            ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScorePrefixKey
                        GROUP BY searchIndexTbl.SubjectId) sortTable
                    ON stateTable.Id = sortTable.SubjectId"
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name,
                "sortTable.FreeTxtRank DESC, sortTable.ContainsRank DESC, stateTable.Id", // first by ranks then by Id to make stable order
                [
                    ("@SearchScoreFreeTxtKeyword", freeTxtKeyword)
                    ("@SearchScoreFreeTxtKey", freeTxtKey)
                    ("@SearchScorePrefix", $"\"%s{sanitizedPrefix}*\"")
                    ("@SearchScorePrefixKey", prefixKey)
                ]

            | None, Some (containsKey, containsValue), Some (prefixKey, prefixValue) ->
                let sanitizedValue = sanitize containsValue
                let sanitizedPrefix = sanitize prefixValue
                sprintf
                    "INNER JOIN (
                        SELECT searchIndexTbl.SubjectId, MAX(containsResult.[RANK]) AS ContainsRank
                        FROM [%s].[%s_SearchIndex] searchIndexTbl
                        JOIN CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreContains) containsResult
                            ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreContainsKey
                        JOIN CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScorePrefix) containsResult
                            ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScorePrefixKey
                        GROUP BY searchIndexTbl.SubjectId) sortTable
                    ON stateTable.Id = sortTable.SubjectId"
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name,
                "sortTable.ContainsRank DESC, stateTable.Id", // first by ranks then by Id to make stable order
                [
                    ("@SearchScoreContains", $"\"%s{sanitizedValue}\"")
                    ("@SearchScoreContainsKey", containsKey)
                    ("@SearchScorePrefix", $"\"%s{sanitizedPrefix}*\"")
                    ("@SearchScorePrefixKey", prefixKey)
                ]

            | Some (freeTxtKey, freeTxtKeyword), Some (containsKey, containsValue), Some (prefixKey, prefixValue) ->
                let sanitizedValue = sanitize containsValue
                let sanitizedPrefix = sanitize prefixValue
                sprintf
                    "INNER JOIN (
                        SELECT searchIndexTbl.SubjectId, MAX(freeTxtResult.[RANK]) AS FreeTxtRank, MAX(containsResult.[RANK]) AS ContainsRank
                        FROM [%s].[%s_SearchIndex] searchIndexTbl
                        JOIN FREETEXTTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreFreeTxtKeyword) freeTxtResult
                            ON searchIndexTbl.Id = freeTxtResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreFreeTxtKey
                        JOIN CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScoreContains) containsResult
                            ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScoreContainsKey
                        JOIN CONTAINSTABLE ([%s].[%s_SearchIndex], ValueStr, @SearchScorePrefix) containsResult
                            ON searchIndexTbl.Id = containsResult.[KEY] AND searchIndexTbl.[Key] = @SearchScorePrefixKey
                        GROUP BY searchIndexTbl.SubjectId) sortTable
                    ON stateTable.Id = sortTable.SubjectId"
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name
                        ecosystemName lifeCycleAdapter.LifeCycle.Name,
                "sortTable.FreeTxtRank DESC, sortTable.ContainsRank DESC, stateTable.Id", // first by ranks then by Id to make stable order
                [
                    ("@SearchScoreFreeTxtKeyword", freeTxtKeyword)
                    ("@SearchScoreFreeTxtKey", freeTxtKey)
                    ("@SearchScoreContains", $"\"%s{sanitizedValue}\"")
                    ("@SearchScoreContainsKey", containsKey)
                    ("@SearchScorePrefix", $"\"%s{sanitizedPrefix}*\"")
                    ("@SearchScorePrefixKey", prefixKey)
                ]

        | UntypedOrderBy.SubjectId direction ->
            "",
            sprintf "stateTable.Id %s" (match direction with OrderDirection.Ascending -> "ASC" | OrderDirection.Descending -> "DESC"),
            []

        | UntypedOrderBy.Random ->
            "",
            sprintf "CHECKSUM(NEWID())",
            []

        | UntypedOrderBy.NumericIndexEntry (key, direction) ->
            sprintf "LEFT JOIN [%s].[%s_Index] sortTable ON stateTable.Id = sortTable.SubjectId AND sortTable.[Key] = @SortKey AND sortTable.ValueStr IS NULL " ecosystemName lifeCycleAdapter.LifeCycle.Name,
            sprintf "sortTable.ValueInt %s, stateTable.Id" (match direction with OrderDirection.Ascending -> "ASC" | OrderDirection.Descending -> "DESC"),
            [("@SortKey", key)]

        | UntypedOrderBy.StringIndexEntry  (key, direction) ->
            sprintf "LEFT JOIN [%s].[%s_Index] sortTable ON stateTable.Id = sortTable.SubjectId AND sortTable.[Key] = @SortKey AND sortTable.ValueInt IS NULL " ecosystemName lifeCycleAdapter.LifeCycle.Name,
            sprintf "sortTable.ValueStr %s, stateTable.Id" (match direction with OrderDirection.Ascending -> "ASC" | OrderDirection.Descending -> "DESC"),
            [("@SortKey", key)]

    let generateSelectAndParams (startingParamGroupSuffix: int) (shouldJoinToStateTable: bool) (optimizedPredicate: OptimizedUntypedPredicate) selectColumns excludeTopLevelSearchIndex =
        let nodeSql, queryParams, nextGroupSuffix =
            generateSqlForIndexQueryNode optimizedPredicate excludeTopLevelSearchIndex (* isTopLevelAnd *) true startingParamGroupSuffix Map.empty

        {| SelectClause =
            sprintf "
                SELECT %s FROM (%s) innerQuery %s"
                    selectColumns
                    nodeSql
                    (
                        if shouldJoinToStateTable then
                            (sprintf "JOIN [%s].[%s] stateTable ON stateTable.Id = innerQuery.SubjectId"
                                ecosystemName lifeCycleAdapter.LifeCycle.Name)
                        else
                            ""
                    )
           QueryParams = queryParams
           NextParamGroupSuffix = nextGroupSuffix |}

    let totalCountSql =
        // unfiltered total count can be slow / O(N) for large tables, so using catalog views to get approximate count fast
        sprintf
            @"SELECT SUM(p.rows)
                FROM sys.tables t
                INNER JOIN sys.partitions p ON t.object_id = p.object_id
                INNER JOIN sys.indexes i ON p.index_id = i.index_id AND p.object_id = i.object_id
                WHERE t.object_id = Object_id('[%s].[%s]') AND i.name = 'PK_%s'"
            ecosystemName lifeCycleAdapter.LifeCycle.Name lifeCycleAdapter.LifeCycle.Name

    // expected order of columns: Subject, LastUpdatedOn, Operation, LastOperationBy, Version
    let readTemporalSnapshot (pKey: string) (cursor: SqlDataReader) : TemporalSnapshot<_, _, _, _> =
        let subject =
            (cursor.Item 0 :?> byte[])
            |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name pKey
            |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
        let asOf = cursor.GetDateTimeOffset 1
        let operation = DataEncode.decodeSubjectAuditOperation (cursor.Item 2 :?> byte[])
        let by = cursor.GetString 3
        let version = cursor.Item 4 :?> int64 |> uint64

        {
            AsOf = asOf
            By = by
            Subject = subject
            Operation = operation
            Version = version
        }

    let readSubjectAuditData (cursor: SqlDataReader) =
        let asOf = cursor.GetDateTimeOffset (1)
        let isConstruction = cursor.GetBoolean 4
        let operationBytes = cursor.Item 2 :?> byte[]
        let operation =
            if isConstruction then
                operationBytes
                |> Compression.ofCompressedJsonText<Constructor>
                |> Result.map (fun a -> a :?> 'Constructor)
                |> Result.map SubjectAuditOperation.Construct
            else
                operationBytes
                |> Compression.ofCompressedJsonText<LifeAction>
                |> Result.map (fun a -> a :?> 'LifeAction)
                |> Result.map SubjectAuditOperation.Act
            |> Result.mapError (sprintf "%A")

        let by = cursor.GetString (3)
        let version = cursor.Item 0 :?> int64 |> uint64
        {
            AsOf = asOf
            By = by
            Version = version
            Operation = operation
        }

    let getPromotedIndicesConfig (adapter: HostedLifeCycleAdapter<_, _, _, _, _, _>) =
        match adapter.LifeCycle.Storage.Type with
        | StorageType.Persistent (promotedIndicesConfig, _) ->
            promotedIndicesConfig.Mappings
        | StorageType.Volatile
        | StorageType.Custom _ ->
            Map.empty

    let addPageParams (command: SqlCommand) (page: ResultPage) =
        command.Parameters.AddWithValue("@skip", page.Offset).SqlDbType <- SqlDbType.BigInt
        // min valid page size in SQL is 1
        command.Parameters.AddWithValue("@pageSize", max 1us page.Size).SqlDbType <- SqlDbType.Int


    member private _.ReadListAsync (page: ResultPage) (cursor: SqlDataReader) (f: SqlDataReader -> 'T) : Task<List<'T>> =
        AsyncSeq.unfoldAsync (
            fun _ ->
                async {
                    match! cursor.ReadAsync() |> Async.AwaitTask with
                    | true ->
                        let item = f cursor
                        return Some (item, Nothing)
                    | false ->
                        return None
                }
        ) Nothing
        |> AsyncSeq.toListAsync
        // e.g. truncate to zero if someone actually requested empty page (useless but api allows it)
        |> Async.Map (List.truncate (int page.Size))
        |> Async.StartAsTask

    interface ISubjectRepo<'Subject, 'LifeAction, 'Constructor, 'SubjectId, 'SubjectIndex, 'OpError> with

        member _.Any(query: PreparedIndexPredicate<'SubjectIndex>): System.Threading.Tasks.Task<bool> =
            let optimizedPredicate = optimizeQueryWithPromotedIndices (getPromotedIndicesConfig lifeCycleAdapter) query.Predicate

            let nodeSql, queryParams, _ = generateSqlForIndexQueryNode optimizedPredicate (* excludeTopLevelSearchIndex *) false (* isTopLevelAnd *) true 0 Map.empty
            let sql = sprintf "SELECT TOP 1 1 AS HasAny FROM (%s) innerQuery" nodeSql

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command = new SqlCommand(sql, connection)
                queryParams
                |> Map.iter (
                    fun key value ->
                        command.Parameters.AddWithValue(key, value) |> ignore
                )
                do! connection.OpenAsync()
                let! res = command.ExecuteScalarAsync()
                return res <> null
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.DoesExistById(id: 'SubjectId) : System.Threading.Tasks.Task<bool> =
            let sql = sprintf "SELECT TOP 1 1 AS HasAny FROM [%s].[%s] WHERE Id = @id" ecosystemName lifeCycleAdapter.LifeCycle.Name

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)
                command.Parameters.AddWithValue("@id", (getIdString id)) |> ignore
                do! connection.OpenAsync()
                let! res = command.ExecuteScalarAsync()
                return res <> null
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member this.GetById(id: 'SubjectId) : System.Threading.Tasks.Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            getIdString id
            |> (this :> ISubjectRepo<_, _, _, _, _, _>).GetByIdStr

        member _.GetByIdStr(idStr: string) : System.Threading.Tasks.Task<Option<VersionedSubject<'Subject, 'SubjectId>>> =
            let sql = sprintf "SELECT TOP 1 Subject, SubjectLastUpdatedOn, Version FROM [%s].[%s] WHERE Id = @id" ecosystemName lifeCycleAdapter.LifeCycle.Name

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)
                command.Parameters.AddWithValue("@id", idStr) |> ignore
                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()
                let! hasRow = cursor.ReadAsync()

                return
                    match hasRow with
                    | true ->
                        {
                            Subject =
                                (cursor.Item 0 :?> byte[])
                                |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name idStr
                                |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                            AsOf = cursor.GetDateTimeOffset 1
                            Version = cursor.GetInt64 2 |> uint64
                        }
                        |> Some
                    | false ->
                        None
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member this.GetByIds(ids: Set<'SubjectId>) : System.Threading.Tasks.Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            ids
            |> Set.map getIdString
            |> (this :> ISubjectRepo<_, _, _, _, _, _>).GetByIdsStr

        member _.GetByIdsStr(idsStr: Set<string>): System.Threading.Tasks.Task<List<VersionedSubject<'Subject, 'SubjectId>>> =

            if idsStr.IsEmpty
            then
                // because SQL fails for empty IdList, also no need to waste roundtrip to SQL server
                Task<List<'Subject>>.FromResult []
            else
                let sql = sprintf "SELECT Subject, SubjectLastUpdatedOn, Version, s.[Id] FROM [%s].[%s] s JOIN @ids i ON s.Id = i.Id" ecosystemName lifeCycleAdapter.LifeCycle.Name

                fun () -> backgroundTask {
                    use connection = new SqlConnection(sqlConnectionString)
                    use command    = new SqlCommand(sql, connection)

                    let idsStrRecords =
                        let dataTable = new DataTable()
                        dataTable.Columns.Add("Id", typeof<string>).MaxLength <- 80
                        Set.toSeq idsStr
                        |> Seq.iter (fun s ->
                            dataTable.Rows.Add(s) |> ignore)
                        dataTable
                    command.Parameters.Add("@ids", SqlDbType.Structured)
                    |> fun param ->
                        param.TypeName <- sprintf "[%s].IdList" ecosystemName
                        param.Value <- idsStrRecords

                    do! connection.OpenAsync()
                    use! cursor = command.ExecuteReaderAsync()

                    return!
                        AsyncSeq.unfoldAsync (
                            fun _ ->
                                async {
                                    match! cursor.ReadAsync() |> Async.AwaitTask with
                                    | true ->
                                        let pKey = cursor.GetString 3
                                        return
                                            (
                                                {
                                                    Subject =
                                                        (cursor.Item 0 :?> byte[])
                                                        |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name pKey
                                                        |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                                                    AsOf = cursor.GetDateTimeOffset 1
                                                    Version = cursor.GetInt64 2 |> uint64
                                                },
                                                Nothing
                                            )
                                            |> Some
                                    | false ->
                                        return None
                                }
                        ) Nothing
                        |> AsyncSeq.toListAsync
                        |> Async.StartAsTask
                }
                |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.FilterFetchIds (query: IndexQuery<'SubjectIndex>) : Task<List<'SubjectId>> =
            let optimizedPredicate = optimizeQueryWithPromotedIndices (getPromotedIndicesConfig lifeCycleAdapter) query.Predicate

            let maybeTopFreeTextParams = tryFindExactlyOneTopLevelFreeTextMatchArgs query.Predicate
            let maybeTopContainsTextParams = tryFindExactlyOneTopLevelContainsMatchArgs query.Predicate
            let maybeTopContainsPrefixTextParams = tryFindExactlyOneTopLevelContainsMatchPrefixArgs query.Predicate
            let selectAndParams = generateSelectAndParams 0 true optimizedPredicate "stateTable.Subject, stateTable.[Id]" maybeTopFreeTextParams.IsSome
            let sortTableJoin, orderBy, sortParams = generateSortTableJoinOrderByAndParams query.ResultSetOptions maybeTopFreeTextParams maybeTopContainsTextParams maybeTopContainsPrefixTextParams

            let sql = sprintf "
                %s
                %s
                ORDER BY %s
                OFFSET @skip ROWS
                FETCH NEXT @pageSize ROWS ONLY" selectAndParams.SelectClause sortTableJoin orderBy

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                selectAndParams.QueryParams
                |> Map.iter (
                    fun key value ->
                        command.Parameters.AddWithValue(key, value) |> ignore
                )
                sortParams |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
                addPageParams command query.ResultSetOptions.Page

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                return!
                    this.ReadListAsync
                        query.ResultSetOptions.Page
                        cursor
                        (fun cursor ->
                            let pKey = cursor.GetString 1
                            (cursor.Item 0 :?> byte[])
                            // Yes, we need to fetch full subject values to get the typed SubjectId.  TODO: fix this once we have enforced Id <-> string isomorphism.
                            |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name pKey
                            |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject |> getId)
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.FilterFetchSubjects (query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            let optimizedPredicate = optimizeQueryWithPromotedIndices (getPromotedIndicesConfig lifeCycleAdapter) query.Predicate

            let maybeTopFreeTextParams = tryFindExactlyOneTopLevelFreeTextMatchArgs query.Predicate
            let maybeTopContainsTextParams = tryFindExactlyOneTopLevelContainsMatchArgs query.Predicate
            let maybeTopContainsPrefixTextParams = tryFindExactlyOneTopLevelContainsMatchPrefixArgs query.Predicate
            let selectAndParams = generateSelectAndParams 0 true optimizedPredicate "stateTable.Subject, stateTable.SubjectLastUpdatedOn, stateTable.Version, stateTable.[Id]" maybeTopFreeTextParams.IsSome
            let sortTableJoin, orderBy, sortParams = generateSortTableJoinOrderByAndParams query.ResultSetOptions maybeTopFreeTextParams maybeTopContainsTextParams maybeTopContainsPrefixTextParams
            let sql = sprintf "
                %s
                %s
                ORDER BY %s
                OFFSET @skip ROWS
                FETCH NEXT @pageSize ROWS ONLY" selectAndParams.SelectClause sortTableJoin orderBy

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                selectAndParams.QueryParams
                |> Map.iter (
                    fun key value ->
                        command.Parameters.AddWithValue(key, value) |> ignore
                )
                sortParams |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
                addPageParams command query.ResultSetOptions.Page

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                return!
                    this.ReadListAsync
                        query.ResultSetOptions.Page
                        cursor
                        (fun cursor ->
                            let compressedJson = (cursor.Item 0 :?> byte[])
                            let asOf = cursor.GetDateTimeOffset 1
                            let version = cursor.GetInt64 2 |> uint64
                            let pKey = cursor.GetString 3
                            Task.Run (fun () ->
                                {
                                    Subject =
                                        compressedJson
                                        |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name pKey
                                        |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                                    AsOf = asOf
                                    Version = version
                                }))
                    |> Task.map Task.WhenAll
                    |> Task.unwrap
                    |> Task.map List.ofArray
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member this.FilterFetchSubjectsWithTotalCount  (query: IndexQuery<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            let optimizedPredicate = optimizeQueryWithPromotedIndices (getPromotedIndicesConfig lifeCycleAdapter) query.Predicate

            let maybeTopFreeTextParams = tryFindExactlyOneTopLevelFreeTextMatchArgs query.Predicate
            let maybeTopContainsTextParams = tryFindExactlyOneTopLevelContainsMatchArgs query.Predicate
            let maybeTopContainsPrefixTextParams = tryFindExactlyOneTopLevelContainsMatchPrefixArgs query.Predicate
            let selectAndParams = generateSelectAndParams 0 true optimizedPredicate "stateTable.Subject, stateTable.SubjectLastUpdatedOn, stateTable.Version, stateTable.[Id]" maybeTopFreeTextParams.IsSome
            let sortTableJoin, orderBy, sortParams = generateSortTableJoinOrderByAndParams query.ResultSetOptions maybeTopFreeTextParams maybeTopContainsTextParams maybeTopContainsPrefixTextParams
            let dataSql = sprintf "
                %s
                %s
                ORDER BY %s
                OFFSET @skip ROWS
                FETCH NEXT @pageSize ROWS ONLY" selectAndParams.SelectClause sortTableJoin orderBy

            // COUNT_BIG(*) here is sufficient as opposed to count(distinct *) as the inner statements
            // provide distinct result sets using UNION, INTERSECT and EXCEPT
            let countSqlAndParams =
                generateSelectAndParams selectAndParams.NextParamGroupSuffix false optimizedPredicate "COUNT_BIG(*)" maybeTopFreeTextParams.IsSome

            // There's no special magic that SQL Server provides to count the total size of a filtered
            // resultset. We just have to execute another query to figure that out.
            // However we can send both queries together to the server, and read two data sets back, one with the data, the other with a single
            // integer value that is the total count
            let sql = sprintf "%s; %s" dataSql countSqlAndParams.SelectClause

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                selectAndParams.QueryParams
                |> Map.iter (
                    fun key value ->
                        command.Parameters.AddWithValue(key, value) |> ignore
                )

                countSqlAndParams.QueryParams
                |> Map.iter (
                    fun key value ->
                        command.Parameters.AddWithValue(key, value) |> ignore
                )

                sortParams |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
                addPageParams command query.ResultSetOptions.Page

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                let! data =
                    this.ReadListAsync
                        query.ResultSetOptions.Page
                        cursor
                        (fun cursor ->
                            let pKey = cursor.GetString 3
                            {
                                Subject =
                                    (cursor.Item 0 :?> byte[])
                                    |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name pKey
                                    |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                                AsOf = cursor.GetDateTimeOffset 1
                                Version = cursor.GetInt64 2 |> uint64
                            })

                match! cursor.NextResultAsync() with
                | true ->
                    match! cursor.ReadAsync() with
                    | true ->
                        let total = uint64 (cursor.GetInt64(0))
                        return (data, total)
                    | false ->
                        return (data, uint64 data.Length)
                | false ->
                    return (data, uint64 data.Length)
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.FilterCountSubjects (query: PreparedIndexPredicate<'SubjectIndex>) : Task<uint64> =
            let optimizedPredicate = optimizeQueryWithPromotedIndices (getPromotedIndicesConfig lifeCycleAdapter) query.Predicate

            // COUNT_BIG(*) here is sufficient as opposed to count(distinct *) as the inner statements
            // provide distinct result sets using UNION, INTERSECT and EXCEPT
            let selectAndParams = generateSelectAndParams 0 (* shouldJoinToStateTable *) false optimizedPredicate "COUNT_BIG(*)" (* excludeTopLevelSearchIndex *) false
            let sql = selectAndParams.SelectClause
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                selectAndParams.QueryParams
                |> Map.iter (
                    fun key value ->
                        command.Parameters.AddWithValue(key, value) |> ignore
                )

                do! connection.OpenAsync()
                let! totalCountObj = command.ExecuteScalarAsync()
                return Convert.ToUInt64 totalCountObj
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.FetchAllSubjects (options: ResultSetOptions<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>>> =
            let sortTableJoin, orderBy, sortParams = generateSortTableJoinOrderByAndParams options.Options None None None

            let sql = sprintf "
                    SELECT stateTable.Subject, stateTable.SubjectLastUpdatedOn, stateTable.Version, stateTable.[Id] FROM [%s].[%s] stateTable
                    %s
                    ORDER BY %s
                    OFFSET @skip ROWS
                    FETCH NEXT @pageSize ROWS ONLY" ecosystemName lifeCycleAdapter.LifeCycle.Name sortTableJoin orderBy

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                sortParams |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
                addPageParams command options.Options.Page

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                return!
                    this.ReadListAsync
                        options.Options.Page
                        cursor
                        (fun cursor ->
                            let pKey = cursor.GetString 3
                            {
                                Subject =
                                    (cursor.Item 0 :?> byte[])
                                    |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name pKey
                                    |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                                AsOf = cursor.GetDateTimeOffset 1
                                Version = cursor.GetInt64 2 |> uint64
                            })
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.CountAllSubjects () : Task<uint64> =
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(totalCountSql, connection)

                do! connection.OpenAsync()
                let! totalCountObj = command.ExecuteScalarAsync()
                return Convert.ToUInt64 totalCountObj
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.FetchAllSubjectsWithTotalCount (options: ResultSetOptions<'SubjectIndex>) : Task<List<VersionedSubject<'Subject, 'SubjectId>> * uint64> =
            let sortTableJoin, orderBy, sortParams = generateSortTableJoinOrderByAndParams options.Options None None None

            let dataSql = sprintf "
                    SELECT stateTable.Subject, stateTable.SubjectLastUpdatedOn, stateTable.Version, stateTable.[Id] FROM [%s].[%s] stateTable
                    %s
                    ORDER BY %s
                    OFFSET @skip ROWS
                    FETCH NEXT @pageSize ROWS ONLY" ecosystemName lifeCycleAdapter.LifeCycle.Name sortTableJoin orderBy

            let sql = sprintf "%s;%s" dataSql totalCountSql

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                sortParams |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
                addPageParams command options.Options.Page

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                let! data =
                    this.ReadListAsync
                        options.Options.Page
                        cursor
                        (fun cursor ->
                            let pKey = cursor.GetString 3
                            {
                                Subject =
                                    (cursor.Item 0 :?> byte[])
                                    |> Compression.ofCompressedJsonTextWithContextInfoInError lifeCycleAdapter.LifeCycle.Name pKey
                                    |> fun (x: Subject<'SubjectId>) -> x :?> 'Subject
                                AsOf = cursor.GetDateTimeOffset 1
                                Version = cursor.GetInt64 2 |> uint64
                            })

                match! cursor.NextResultAsync() with
                | true ->
                    match! cursor.ReadAsync() with
                    | true ->
                        let total = uint64 (cursor.GetInt64(0))
                        return (data, total)
                    | false ->
                        return (data, uint64 data.Length)
                | false ->
                    return (data, uint64 data.Length)
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions


        member this.GetVersionSnapshotByIdStr (idStr: string) (ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)

                use command =
                    match ofVersion with
                    | GetSnapshotOfVersion.Latest ->
                        let sql = $"SELECT TOP 1 [Subject], [SubjectLastUpdatedOn], [Operation], [LastOperationBy], [Version] FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}] WHERE Id = @id"
                        let command = new SqlCommand(sql, connection)
                        command.Parameters.AddWithValue("@id", idStr) |> ignore
                        command

                    | GetSnapshotOfVersion.Specific specificVersion ->
                        let sql = $"SELECT TOP 1 [Subject], [SubjectLastUpdatedOn], [Operation], [LastOperationBy], [Version] FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_HistoryWithCurrent] WHERE Id = @id AND Version = @version"
                        let command = new SqlCommand(sql, connection)
                        command.Parameters.AddWithValue("@id", idStr) |> ignore
                        command.Parameters.Add("@version", SqlDbType.BigInt).Value <- (int64 specificVersion)
                        command

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                match! cursor.ReadAsync() |> Async.AwaitTask with
                | false ->
                    return None
                | true ->
                    return Some (readTemporalSnapshot idStr cursor)
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member this.GetVersionSnapshotById (id: 'SubjectId) (ofVersion: GetSnapshotOfVersion) : Task<Option<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            (this :> ISubjectRepo<_, _, _, _, _, _>).GetVersionSnapshotByIdStr ((id :> SubjectId).IdString) ofVersion

        member this.FetchWithHistoryById (id: 'SubjectId) (fromLastUpdatedOn: Option<DateTimeOffset>) (toLastUpdatedOn: Option<DateTimeOffset>) (page: ResultPage): Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            (this :> ISubjectRepo<_, _, _, _, _, _>).FetchWithHistoryByIdStr (id :> SubjectId).IdString fromLastUpdatedOn toLastUpdatedOn page

        member _.FetchWithHistoryByIdStr (idStr: string) (fromLastUpdatedOn: Option<DateTimeOffset>) (toLastUpdatedOn: Option<DateTimeOffset>) (page: ResultPage): Task<List<TemporalSnapshot<'Subject, 'LifeAction, 'Constructor, 'SubjectId>>> =
            let sql = $"""
                SELECT [Subject], [SubjectLastUpdatedOn], [Operation], [LastOperationBy], [Version]
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_HistoryWithCurrent]
                WHERE Id = @id
                    {(match fromLastUpdatedOn with | Some _ -> "AND [SubjectLastUpdatedOn] >= @from" | None -> "")}
                    {(match toLastUpdatedOn with | Some _ -> "AND [SubjectLastUpdatedOn] <= @to" | None -> "")}
                ORDER BY [SubjectLastUpdatedOn] DESC
                OFFSET @skip ROWS
                FETCH NEXT @pageSize ROWS ONLY"""

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                command.Parameters.AddWithValue("@id", idStr) |> ignore
                addPageParams command page

                // TODO from/to parameters were missing here, safe to add?
                match fromLastUpdatedOn with
                | None -> ()
                | Some dt -> command.Parameters.AddWithValue("@from", dt).SqlDbType <- SqlDbType.DateTimeOffset

                match toLastUpdatedOn with
                | None -> ()
                | Some dt -> command.Parameters.AddWithValue("@to", dt).SqlDbType <- SqlDbType.DateTimeOffset

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                return!
                    this.ReadListAsync
                        page
                        cursor
                        (readTemporalSnapshot idStr)
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member this.FetchAuditTrail (idStr: string) (page: ResultPage) : Task<List<SubjectAuditData<'LifeAction, 'Constructor>>> =
            let sql = $"
                SELECT [Version], [SubjectLastUpdatedOn], [Operation], [LastOperationBy], [IsConstruction]
                FROM [{ecosystemName}].[{lifeCycleAdapter.LifeCycle.Name}_HistoryWithCurrent]
                WHERE Id = @id
                ORDER BY [Version] DESC
                OFFSET @skip ROWS
                FETCH NEXT @pageSize ROWS ONLY"

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)

                command.Parameters.AddWithValue("@id", idStr) |> ignore
                addPageParams command page

                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()

                return!
                    this.ReadListAsync
                        page
                        cursor
                        readSubjectAuditData
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member this.GetSideEffectPermanentFailures (scope: UpdatePermanentFailuresScope) =
            // TODO: fix copy-and-paste from SqlStorageHandler
            backgroundTask {
                let tableName = sprintf "[%s].[%s_SideEffect]" ecosystemName lifeCycleAdapter.LifeCycle.Name
                let sql = sprintf "
                        WITH failureGroups AS
                        (
                            SELECT SubjectId, SideEffectSeqNumber FROM %s
                            WHERE
                                FailureSeverity IS NOT NULL
                                AND ISNULL(@sideEffectId, SideEffectId) = SideEffectId
                                AND ISNULL(@subjectId, SubjectId) = SubjectId
                                AND ISNULL(@seqNumber, SideEffectSeqNumber) = SideEffectSeqNumber
                            GROUP BY SubjectId, SideEffectSeqNumber
                            ORDER BY SubjectId, SideEffectSeqNumber
                            OFFSET 0 ROWS
                            FETCH NEXT @batchSize ROWS ONLY
                        )
                        SELECT se.SubjectId, se.SideEffectId, se.SideEffectSeqNumber, se.SideEffect, se.CreatedOn, se.FailureReason, se.FailureSeverity
                        FROM %s AS se
                        JOIN failureGroups ON failureGroups.SubjectId = se.SubjectId AND failureGroups.SideEffectSeqNumber = se.SideEffectSeqNumber
                        WHERE
                            se.FailureSeverity IS NOT NULL
                            AND ISNULL(@sideEffectId, se.SideEffectId) = se.SideEffectId
                        ORDER BY se.SubjectId, se.SideEffectSeqNumber" tableName tableName

                let batchSize, subjectIdArg, sideEffectIdArg, seqNumArg =
                    match scope with
                    | UpdatePermanentFailuresScope.Single (subjectId, sideEffectId) ->
                        1uy, box subjectId, box sideEffectId, box DBNull.Value
                    | UpdatePermanentFailuresScope.SeqNum (subjectId, seqNum) ->
                        1uy, box subjectId, box DBNull.Value, seqNum |> uint64ToInt64MaintainOrder |> box
                    | UpdatePermanentFailuresScope.Subject subjectId ->
                        50uy, box subjectId, box DBNull.Value, box DBNull.Value
                    | UpdatePermanentFailuresScope.NextSeqBatch batchSize ->
                        batchSize, box DBNull.Value, box DBNull.Value, box DBNull.Value

                use connection = new SqlConnection(sqlConnectionString)
                do! connection.OpenAsync()
                use command = connection.CreateCommand()
                command.CommandText <- sql
                command.Parameters.Add("@batchSize", SqlDbType.TinyInt).Value <- batchSize
                command.Parameters.Add("@subjectId", SqlDbType.NVarChar).Value <- subjectIdArg
                command.Parameters.Add("@sideEffectId", SqlDbType.UniqueIdentifier).Value <- sideEffectIdArg
                command.Parameters.Add("@seqNumber", SqlDbType.BigInt).Value <- seqNumArg

                use! reader = command.ExecuteReaderAsync()

                return!
                    AsyncSeq.unfoldAsync (
                        fun _ ->
                            async {
                                match! reader.ReadAsync() |> Async.AwaitTask with
                                | true ->
                                    let subjectId = reader.GetString 0
                                    let sideEffectId = reader.GetGuid 1
                                    let sideEffectSeqNum = reader.GetInt64 2 |> int64ToUInt64MaintainOrder
                                    let sideEffectData = (reader.Item 3 :?> byte[]) |> gzipDecompressToUtf8String
                                    let createdOn = reader.GetDateTimeOffset 4
                                    let failureReason = reader.GetString 5
                                    let failureSeverity = reader.GetByte 6

                                    return Some ({
                                        SubjectIdStr = subjectId
                                        SideEffectId = sideEffectId
                                        SeqNum = sideEffectSeqNum
                                        FailureSeverity = failureSeverity
                                        FailureReason = failureReason
                                        SideEffectData = sideEffectData
                                        CreatedOn = createdOn
                                    }, Nothing)
                                | false ->
                                    return None
                            }
                    ) Nothing
                    |> AsyncSeq.toListAsync
                    |> Async.StartAsTask
            }

type SqlServerSubjectBlobRepo
    (
        config: SqlServerConnectionStrings
    ) =

    interface IBlobRepo with
        member _.GetBlobData (ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) : Task<Option<BlobData>> =
            let sqlConnectionString = config.ForEcosystem ecosystemName
            let sql = sprintf "SELECT TOP 1 [MimeType], [Data] FROM [%s].[%s_Blob] WHERE SubjectId = @subjectId AND Id = @id" ecosystemName subjectRef.LifeCycleName

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)
                command.Parameters.AddWithValue("@subjectId", subjectRef.SubjectIdStr) |> ignore
                command.Parameters.AddWithValue("@id", blobId) |> ignore
                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync()
                match! cursor.ReadAsync() with
                | true ->
                    let mimeType = if cursor.IsDBNull 0 then null else cursor.GetString(0)
                    let data = (cursor.Item 1) :?> byte[]
                    return Some { Data = data; MimeType = MimeType.ofString mimeType }
                | false ->
                    return None
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions

        member _.GetBlobDataStream (ecosystemName: string) (subjectRef: LocalSubjectPKeyReference) (blobId: Guid) (readBlobDataStream: Option<BlobDataStream> -> Task) : Task =
            let sqlConnectionString = config.ForEcosystem ecosystemName
            let sql = sprintf "SELECT TOP 1 [MimeType], DataLength([Data]), [Data] FROM [%s].[%s_Blob] WHERE SubjectId = @subjectId AND Id = @id" ecosystemName subjectRef.LifeCycleName

            fun () -> backgroundTask {
                use connection = new SqlConnection(sqlConnectionString)
                use command    = new SqlCommand(sql, connection)
                command.Parameters.AddWithValue("@subjectId", subjectRef.SubjectIdStr) |> ignore
                command.Parameters.AddWithValue("@id", blobId) |> ignore
                do! connection.OpenAsync()
                use! cursor = command.ExecuteReaderAsync(CommandBehavior.SequentialAccess)
                match! cursor.ReadAsync() with
                | true ->
                    let mimeType = if cursor.IsDBNull 0 then null else cursor.GetString(0)
                    let dataLength = cursor.GetInt64(1)
                    use dataStream = cursor.GetStream(2)
                    use memoryStream = new System.IO.MemoryStream()
                    do! dataStream.CopyToAsync(memoryStream)
                    memoryStream.Position <- 0L
                    do! readBlobDataStream (Some { TotalBytes = dataLength; Stream = memoryStream; MimeType = MimeType.ofString mimeType })
                | false ->
                    do! readBlobDataStream None
            }
            |> SqlServerTransientErrorDetection.wrapTransientExceptions
            :> Task

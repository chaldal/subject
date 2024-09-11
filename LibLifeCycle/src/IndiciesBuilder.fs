namespace LibLifeCycle

[<AutoOpen>]
module IndicesWorkflow =

    type IndicesBuilder() =
        member _.Zero<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                       when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                       and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                       and  'SubjectSearchIndex  :> SubjectSearchIndex
                       and  'SubjectGeographyIndex :> SubjectGeographyIndex
                       and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                       and  'OpError             :> OpError>() : seq<'SubjectIndex> =
            Seq.empty

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntry: 'SubjectNumericIndex) : seq<'SubjectIndex> =
            'SubjectIndex.New (Choice1Of4 indexEntry)
            |> Seq.singleton

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(maybeIndexEntry: Option<'SubjectNumericIndex>) : seq<'SubjectIndex> =
            match maybeIndexEntry with
            | Some indexEntry ->
                'SubjectIndex.New (Choice1Of4 indexEntry)
                |> Seq.singleton
            | None ->
                Seq.empty

        member _.YieldFrom<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntries: seq<'SubjectNumericIndex>) : seq<'SubjectIndex> =
            indexEntries
            |> Seq.map (Choice1Of4 >> 'SubjectIndex.New)

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntry: 'SubjectStringIndex) : seq<'SubjectIndex> =
            'SubjectIndex.New (Choice2Of4 indexEntry)
            |> Seq.singleton

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(maybeIndexEntry: Option<'SubjectStringIndex>) : seq<'SubjectIndex> =
            match maybeIndexEntry with
            | Some indexEntry ->
                'SubjectIndex.New (Choice2Of4 indexEntry)
                |> Seq.singleton
            | None ->
                Seq.empty

        member _.YieldFrom<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntries: seq<'SubjectStringIndex>) : seq<'SubjectIndex> =
            indexEntries
            |> Seq.map (Choice2Of4 >> 'SubjectIndex.New)

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntry: 'SubjectSearchIndex) : seq<'SubjectIndex> =
            'SubjectIndex.New (Choice3Of4 indexEntry)
            |> Seq.singleton

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(maybeIndexEntry: Option<'SubjectSearchIndex>) : seq<'SubjectIndex> =
            match maybeIndexEntry with
            | Some indexEntry ->
                'SubjectIndex.New (Choice3Of4 indexEntry)
                |> Seq.singleton
            | None ->
                Seq.empty

        member _.YieldFrom<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntries: seq<'SubjectSearchIndex>) : seq<'SubjectIndex> =
            indexEntries
            |> Seq.map (Choice3Of4 >> 'SubjectIndex.New)

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntry: 'SubjectGeographyIndex) : seq<'SubjectIndex> =
            'SubjectIndex.New (Choice4Of4 indexEntry)
            |> Seq.singleton

        member _.Yield<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(maybeIndexEntry: Option<'SubjectGeographyIndex>) : seq<'SubjectIndex> =
            match maybeIndexEntry with
            | Some indexEntry ->
                'SubjectIndex.New (Choice4Of4 indexEntry)
                |> Seq.singleton
            | None ->
                Seq.empty

        member _.YieldFrom<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError
                        when 'SubjectNumericIndex :> SubjectNumericIndex<'OpError>
                        and  'SubjectStringIndex  :> SubjectStringIndex<'OpError>
                        and  'SubjectSearchIndex  :> SubjectSearchIndex
                        and  'SubjectGeographyIndex :> SubjectGeographyIndex
                        and  'SubjectIndex :> SubjectIndex<'SubjectIndex, 'SubjectNumericIndex, 'SubjectStringIndex, 'SubjectSearchIndex, 'SubjectGeographyIndex, 'OpError>
                        and  'OpError             :> OpError>(indexEntries: seq<'SubjectGeographyIndex>) : seq<'SubjectIndex> =
            indexEntries
            |> Seq.map (Choice4Of4 >> 'SubjectIndex.New)

        member _.Combine (subjectIndex: seq<'SubjectIndex>, res: unit -> seq<'SubjectIndex>) =
            res()
            |> Seq.append subjectIndex

        member _.Delay (value: unit -> seq<'SubjectIndex>) : unit -> seq<'SubjectIndex> =
            value

        member _.Run (value: unit -> seq<'SubjectIndex>) : seq<'SubjectIndex> =
            value()

    let indices = IndicesBuilder()
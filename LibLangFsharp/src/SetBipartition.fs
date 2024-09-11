[<AutoOpen>]
module SetBipartition

type SetBipartition<'V when 'V: comparison> =
    private | Internals of TheLeft: Set<'V> * TheRight: Set<'V>

    with
        static member empty =
            Internals (Set.empty, Set.empty)

        static member ofLeft (left: Set<'V>) =
            Internals (left, Set.empty)

        static member ofRight (right: Set<'V>) =
            Internals (Set.empty, right)

        static member ofLeftAndRight (left: Set<'V>, right: Set<'V>) =
            match Set.intersect left right |> Set.count = 0 with
            | true  -> Internals (left, right)
            | false -> failwith "Trying to construct a SetBipartition.ofLeftAndRight from non-disjoint sets"

        member private this.Parts : Set<'V> * Set<'V> =
            match this with
            | Internals (left, right) -> (left, right)

        member this.Left : Set<'V> = this.Parts |> fst

        member this.Right : Set<'V> = this.Parts |> snd

        member this.MoveRight (items: Set<'V>) : SetBipartition<'V> =
            let (left, right) = this.Parts
            (Set.difference left items, Set.union right items)
            |> Internals

        member this.MoveLeft (items: Set<'V>) : SetBipartition<'V> =
            let (left, right) = this.Parts
            (Set.union left items, Set.difference right items)
            |> Internals

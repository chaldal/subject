module ``Index Query Optimizer Tests``

open LibLifeCycle
open LibLifeCycleHost.Storage.SqlServer.SqlServerPromotedIndexQueryOptimizer

open Xunit
open FsUnit.Xunit

let testAlgo query config expected =
    query
    |> optimizeQueryWithPromotedIndices config
    |> should equal expected

[<Fact>]
let ``And with 1 promotable child`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.EqualToNumeric ("key2", 2)
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected = OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1", PromotedValue "1"))

    testAlgo query config expected

[<Fact>]
let ``Or with 1 promotable child`` () =
    let query =
        UntypedPredicate.Or (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.EqualToNumeric ("key2", 2)
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Or (
            OptimizedUntypedPredicate.InNumeric ("key1", [1], Some (PromotedKey "key1", PromotedValue "1")),
            OptimizedUntypedPredicate.InNumeric ("key2", [2], None)
        )

    testAlgo query config expected

[<Fact>]
let ``Diff with promotable child on included side`` () =
    let query =
        UntypedPredicate.Diff (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.EqualToNumeric ("key2", 2)
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Diff (
            OptimizedUntypedPredicate.InNumeric ("key1", [1], Some (PromotedKey "key1", PromotedValue "1")),
            OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1", PromotedValue "1"))
        )

    testAlgo query config expected

[<Fact>]
let ``Diff with promotable child on excluded side`` () =
    let query =
        UntypedPredicate.Diff (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.EqualToNumeric ("key2", 2)
        )
    let config = [ PromotedKey "key2", nel { Choice1Of2 (BaseKey "key2") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Diff (
            OptimizedUntypedPredicate.InNumeric ("key1", [1], None),
            OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key2", PromotedValue "2"))
        )

    testAlgo query config expected

[<Fact>]
let ``Diff with promotable child on both sides`` () =
    let query =
        UntypedPredicate.Diff (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.EqualToNumeric ("key2", 2)
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") }; PromotedKey "key2", nel { Choice1Of2 (BaseKey "key2") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Diff (
            OptimizedUntypedPredicate.InNumeric ("key1", [1], Some (PromotedKey "key1", PromotedValue "1")),
            OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1", PromotedValue "1"))
        )

    testAlgo query config expected

[<Fact>]
let ``And with compound promotable children`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.EqualToNumeric ("key2", 2)
        )
    let config = [ PromotedKey "key1key2", nel { Choice1Of2 (BaseKey "key1"); Choice2Of2 (BaseSeparator "-"); Choice1Of2 (BaseKey "key2")} ] |> Map.ofSeq
    let expected = OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1key2", PromotedValue "1-2"))

    testAlgo query config expected

[<Fact>]
let ``And with transitive compound promotable`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.And (
                UntypedPredicate.EqualToNumeric ("key2", 2),
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key3", 3),
                    UntypedPredicate.And (
                        UntypedPredicate.EqualToNumeric ("key4", 4),
                        UntypedPredicate.EqualToNumeric ("key5", 5)
                    )
                )
            )
        )
    let config = [ PromotedKey "key1key5", nel { Choice1Of2 (BaseKey "key1"); Choice2Of2 (BaseSeparator "-"); Choice1Of2 (BaseKey "key5") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.And (
            OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1key5", PromotedValue "1-5")),
            OptimizedUntypedPredicate.And (
                OptimizedUntypedPredicate.InNumeric ("key3", [3], Some (PromotedKey "key1key5", PromotedValue "1-5")),
                OptimizedUntypedPredicate.InNumeric ("key4", [4], Some (PromotedKey "key1key5", PromotedValue "1-5"))
            )
        )

    testAlgo query config expected

[<Fact>]
let ``And with promotion passed down across child Or`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.Or (
                UntypedPredicate.EqualToNumeric ("key2", 2),
                UntypedPredicate.EqualToNumeric ("key3", 3)
            )
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Or (
            OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1", PromotedValue "1")),
            OptimizedUntypedPredicate.InNumeric ("key3", [3], Some (PromotedKey "key1", PromotedValue "1"))
        )

    testAlgo query config expected

[<Fact>]
let ``And not trimmed if child cannot use promotion`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.Matches ("matchKey", "keywords")
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.And (
            OptimizedUntypedPredicate.InNumeric ("key1", [1], Some (PromotedKey "key1", PromotedValue "1")),
            OptimizedUntypedPredicate.Matches ("matchKey", "keywords")
        )

    testAlgo query config expected

[<Fact>]
let ``And not trimmed if deeper child cannot use promotion`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.And (
                UntypedPredicate.Or (
                    UntypedPredicate.Matches ("matchKey1", "keyword1"),
                    UntypedPredicate.Matches ("matchKey2", "keyword2")
                ),
                UntypedPredicate.Or (
                    UntypedPredicate.Matches ("matchKey3", "keyword3"),
                    UntypedPredicate.Matches ("matchKey4", "keyword4")
                )
            )
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.And (
            OptimizedUntypedPredicate.InNumeric ("key1", [1], Some (PromotedKey "key1", PromotedValue "1")),
            OptimizedUntypedPredicate.And (
                OptimizedUntypedPredicate.Or (
                    OptimizedUntypedPredicate.Matches ("matchKey1", "keyword1"),
                    OptimizedUntypedPredicate.Matches ("matchKey2", "keyword2")
                ),
                OptimizedUntypedPredicate.Or (
                    OptimizedUntypedPredicate.Matches ("matchKey3", "keyword3"),
                    OptimizedUntypedPredicate.Matches ("matchKey4", "keyword4")
                )
            )
        )

    testAlgo query config expected

[<Fact>]
let ``And is trimmed when deep child is other promotion, other promotion dose not overwrite trimmed node`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.Or (
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key2", 2),
                    UntypedPredicate.EqualToNumeric ("key3", 3)
                ),
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key2", 2),
                    UntypedPredicate.EqualToNumeric ("key4", 4)
                )
            )
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") }; PromotedKey "key2", nel { Choice1Of2 (BaseKey "key2") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Or (
            OptimizedUntypedPredicate.And (
                OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1", PromotedValue "1")),
                OptimizedUntypedPredicate.InNumeric ("key3", [3], Some (PromotedKey "key1", PromotedValue "1"))
            ),
            OptimizedUntypedPredicate.And (
                OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1", PromotedValue "1")),
                OptimizedUntypedPredicate.InNumeric ("key4", [4], Some (PromotedKey "key1", PromotedValue "1"))
            )
        )

    testAlgo query config expected

[<Fact>]
let ``Compound when part of the compound is inside an Or`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.Or (
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key2", 2),
                    UntypedPredicate.EqualToNumeric ("key3", 3)
                ),
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key2", 2),
                    UntypedPredicate.EqualToNumeric ("key4", 4)
                )
            )
        )
    let config = [ PromotedKey "key1key2", nel { Choice1Of2 (BaseKey "key1"); Choice2Of2 (BaseSeparator "-"); Choice1Of2 (BaseKey "key2") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Or (
            OptimizedUntypedPredicate.InNumeric ("key3", [3], Some (PromotedKey "key1key2", PromotedValue "1-2")),
            OptimizedUntypedPredicate.InNumeric ("key4", [4], Some (PromotedKey "key1key2", PromotedValue "1-2"))
        )

    testAlgo query config expected

[<Fact>]
let ``Deep child still promoted when parent not promotable`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.Or (
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key1", 1),
                    UntypedPredicate.EqualToNumeric ("key2", 2)
                ),
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key3", 3),
                    UntypedPredicate.EqualToNumeric ("key4", 4)
                )
            ),
            UntypedPredicate.Matches ("key", "keyword")
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.And (
            OptimizedUntypedPredicate.Or (
                OptimizedUntypedPredicate.InNumeric ("key2", [2], Some (PromotedKey "key1", PromotedValue "1")),
                OptimizedUntypedPredicate.And (
                    OptimizedUntypedPredicate.InNumeric ("key3", [3], None),
                    OptimizedUntypedPredicate.InNumeric ("key4", [4], None)
                )
            ),
            OptimizedUntypedPredicate.Matches ("key", "keyword")
        )

    testAlgo query config expected

[<Fact>]
let ``Compound promotable trimmed`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key1", 1),
            UntypedPredicate.And (
                UntypedPredicate.EqualToNumeric ("key2", 2),
                UntypedPredicate.And (
                    UntypedPredicate.EqualToNumeric ("key3", 3),
                    UntypedPredicate.EqualToNumeric ("key4", 4)
                )
            )
        )
    let config = [ PromotedKey "key1key2", nel { Choice1Of2 (BaseKey "key1"); Choice2Of2 (BaseSeparator "-"); Choice1Of2 (BaseKey "key2") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.And (
            OptimizedUntypedPredicate.InNumeric ("key3", [3], Some (PromotedKey "key1key2", PromotedValue "1-2")),
            OptimizedUntypedPredicate.InNumeric ("key4", [4], Some (PromotedKey "key1key2", PromotedValue "1-2"))
        )

    testAlgo query config expected

[<Fact>]
let ``Basic numeric between`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.GreaterThanNumeric ("key1", 0),
            UntypedPredicate.LessThanNumeric ("key1", 10)
        )
    let expected = OptimizedUntypedPredicate.BetweenNumeric ("key1", PredicateBound.Exclusive 0, PredicateBound.Exclusive 10, None)

    testAlgo query Map.empty expected

[<Fact>]
let ``Multiple inequalities reduced`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.GreaterThanNumeric ("key1", 0),
            UntypedPredicate.And (
                UntypedPredicate.GreaterThanNumeric ("key1", 5),
                UntypedPredicate.And (
                    UntypedPredicate.GreaterThanOrEqualToNumeric ("key1", 2),
                    UntypedPredicate.LessThanNumeric ("key1", 10)
                )
            )
        )
    let expected = OptimizedUntypedPredicate.BetweenNumeric ("key1", PredicateBound.Exclusive 5, PredicateBound.Exclusive 10, None)

    testAlgo query Map.empty expected

[<Fact>]
let ``Basic string between`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.GreaterThanString ("key1", "abc"),
            UntypedPredicate.LessThanString ("key1", "abcdefg")
        )
    let expected = OptimizedUntypedPredicate.BetweenString ("key1", PredicateBound.Exclusive "abc", PredicateBound.Exclusive "abcdefg", None)

    testAlgo query Map.empty expected

[<Fact>]
let ``Between with promotion`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("promotable", 1),
            UntypedPredicate.And (
                UntypedPredicate.GreaterThanNumeric ("key1", 0),
                UntypedPredicate.LessThanNumeric ("key1", 10)
            )
        )
    let config = [ PromotedKey "promotable", nel { Choice1Of2 (BaseKey "promotable") } ] |> Map.ofSeq
    let expected = OptimizedUntypedPredicate.BetweenNumeric ("key1", PredicateBound.Exclusive 0, PredicateBound.Exclusive 10, Some (PromotedKey "promotable", PromotedValue "1"))

    testAlgo query config expected

[<Fact>]
let ``Basic In`` () =
    let query =
        UntypedPredicate.Or (
            UntypedPredicate.EqualToNumeric ("key1", 0),
            UntypedPredicate.EqualToNumeric ("key1", 1)
        )
    let expected = OptimizedUntypedPredicate.InNumeric ("key1", [0; 1], None)

    testAlgo query Map.empty expected

[<Fact>]
let ``Large In`` () =
    let query =
        UntypedPredicate.Or (
            UntypedPredicate.EqualToNumeric ("key1", 0),
            UntypedPredicate.Or (
                UntypedPredicate.EqualToNumeric ("key1", 1),
                UntypedPredicate.Or (
                    UntypedPredicate.EqualToNumeric ("key1", 2),
                    UntypedPredicate.Or (
                        UntypedPredicate.EqualToNumeric ("key1", 3),
                        UntypedPredicate.Or (
                            UntypedPredicate.EqualToNumeric ("key1", 4),
                            UntypedPredicate.Or (
                                UntypedPredicate.EqualToNumeric ("key1", 5),
                                UntypedPredicate.EqualToNumeric ("key1", 6)
                            )
                        )
                    )
                )
            )
        )
    let expected = OptimizedUntypedPredicate.InNumeric ("key1", [0; 1; 2; 3; 4; 5; 6], None)

    testAlgo query Map.empty expected

[<Fact>]
let ``Large In under And`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key2", 0),
            UntypedPredicate.Or (
                UntypedPredicate.EqualToNumeric ("key1", 0),
                UntypedPredicate.Or (
                    UntypedPredicate.EqualToNumeric ("key1", 1),
                    UntypedPredicate.Or (
                        UntypedPredicate.EqualToNumeric ("key1", 2),
                        UntypedPredicate.Or (
                            UntypedPredicate.EqualToNumeric ("key1", 3),
                            UntypedPredicate.Or (
                                UntypedPredicate.EqualToNumeric ("key1", 4),
                                UntypedPredicate.Or (
                                    UntypedPredicate.EqualToNumeric ("key1", 5),
                                    UntypedPredicate.EqualToNumeric ("key1", 6)
                                )
                            )
                        )
                    )
                )
            )
        )
    let expected = OptimizedUntypedPredicate.And(
        OptimizedUntypedPredicate.InNumeric ("key2", [0], None),
        OptimizedUntypedPredicate.InNumeric ("key1", [0; 1; 2; 3; 4; 5; 6], None)
    )

    testAlgo query Map.empty expected

[<Fact>]
let ``Large In with promotion`` () =
    let query =
        UntypedPredicate.And (
            UntypedPredicate.EqualToNumeric ("key2", 0),
            UntypedPredicate.Or (
                UntypedPredicate.EqualToNumeric ("key1", 0),
                UntypedPredicate.Or (
                    UntypedPredicate.EqualToNumeric ("key1", 1),
                    UntypedPredicate.Or (
                        UntypedPredicate.EqualToNumeric ("key1", 2),
                        UntypedPredicate.Or (
                            UntypedPredicate.EqualToNumeric ("key1", 3),
                            UntypedPredicate.Or (
                                UntypedPredicate.EqualToNumeric ("key1", 4),
                                UntypedPredicate.Or (
                                    UntypedPredicate.EqualToNumeric ("key1", 5),
                                    UntypedPredicate.EqualToNumeric ("key1", 6)
                                )
                            )
                        )
                    )
                )
            )
        )
    let config = [ PromotedKey "key2", nel { Choice1Of2 (BaseKey "key2") } ] |> Map.ofSeq
    let expected = OptimizedUntypedPredicate.InNumeric ("key1", [0; 1; 2; 3; 4; 5; 6], Some (PromotedKey "key2", PromotedValue "0"))

    testAlgo query config expected

[<Fact>]
let ``Promotion in In ignored`` () =
    let query =
        UntypedPredicate.Or (
            UntypedPredicate.EqualToNumeric ("key1", 0),
            UntypedPredicate.Or (
                UntypedPredicate.EqualToNumeric ("key1", 1),
                UntypedPredicate.Or (
                    UntypedPredicate.EqualToNumeric ("key1", 2),
                    UntypedPredicate.Or (
                        UntypedPredicate.EqualToNumeric ("key1", 3),
                        UntypedPredicate.Or (
                            UntypedPredicate.EqualToNumeric ("key1", 4),
                            UntypedPredicate.Or (
                                UntypedPredicate.EqualToNumeric ("key1", 5),
                                UntypedPredicate.EqualToNumeric ("key1", 6)
                            )
                        )
                    )
                )
            )
        )
    let config = [ PromotedKey "key1", nel { Choice1Of2 (BaseKey "key1") } ] |> Map.ofSeq
    let expected = OptimizedUntypedPredicate.InNumeric ("key1", [0; 1; 2; 3; 4; 5; 6], None)

    testAlgo query config expected

[<Fact>]
let ``In with trimmable And`` () =
    let query =
        UntypedPredicate.Or (
            UntypedPredicate.EqualToNumeric ("key1", 0),
            UntypedPredicate.Or (
                UntypedPredicate.EqualToNumeric ("key1", 1),
                UntypedPredicate.Or (
                    UntypedPredicate.EqualToNumeric ("key1", 2),
                    UntypedPredicate.And (
                        UntypedPredicate.EqualToNumeric ("key1", 3),
                        UntypedPredicate.EqualToNumeric ("key2", 10)
                    )
                )
            )
        )
    let config = [ PromotedKey "key2", nel { Choice1Of2 (BaseKey "key2") } ] |> Map.ofSeq
    let expected =
        OptimizedUntypedPredicate.Or (
            OptimizedUntypedPredicate.InNumeric ("key1", [0; 1; 2], None),
            OptimizedUntypedPredicate.InNumeric ("key1", [3], Some (PromotedKey "key2", PromotedValue "10"))
        )

    testAlgo query config expected

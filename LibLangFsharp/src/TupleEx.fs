[<AutoOpen>]
// was just "module Tuple" but it breaks all references to System.Tuple when LibLangFSharp referenced in a C# project
module TupleEx

type Tuple =
    static member Curry (ctor: ('P1 * 'P2 -> 'T)) : 'P1 -> 'P2 -> 'T =
        fun p1 -> fun p2 -> ctor (p1, p2)

    static member Curry (ctor: ('P1 * 'P2 * 'P3 -> 'T)) : 'P1 -> 'P2 -> 'P3 -> 'T =
        fun p1 -> fun p2 -> fun p3 -> ctor (p1, p2, p3)

    static member Curry (ctor: ('P1 * 'P2 * 'P3 * 'P4 -> 'T)) : 'P1 -> 'P2 -> 'P3 -> 'P4 -> 'T =
        fun p1 -> fun p2 -> fun p3 -> fun p4 -> ctor (p1, p2, p3, p4)

    static member Curry (ctor: ('P1 * 'P2 * 'P3 * 'P4 * 'P5 -> 'T)) : 'P1 -> 'P2 -> 'P3 -> 'P4 -> 'P5 -> 'T =
        fun p1 -> fun p2 -> fun p3 -> fun p4 -> fun p5 -> ctor (p1, p2, p3, p4, p5)

    static member Map (f1: 'T1 -> 'T2) (f2: 'T3 -> 'T4) (p1: 'T1, p2: 'T3): 'T2 * 'T4 =
        (f1 p1, f2 p2)

let inline thrd ((_, _, c): (_ * _ * 'C)) : 'C = c

[<AutoOpen>]
module RecordEqualityComparisonHelper

    // This helper allows you to specify a custom key for a record type to be used for Set equality.
    // It requires one line of attributes, one line on which you define the key, and another line of
    // boilerplate to hook up the functions. Like this:
    //
    // [<CustomEquality; CustomComparison>]
    // type MyRecord = {
    //     Id: string
    //     ...
    // } with
    //     member private this.REC = RecordEqualityComparison(this, (fun t -> t.Id))
    //     override (* BOILERPLATE *) this.Equals(thatObj) = this.REC.SetEquals thatObj; override this.GetHashCode() = this.REC.SetGetHashCode; interface System.IComparable with member this.CompareTo that = this.REC.SetCompareTo that
    //
    // NOTE: We cannot move the with block into a type extension below the original definition, because the compiler says:
    // "Override implementations in augmentations are now deprecated.
    //  Override implementations should be given as part of the initial declaration of a type."
    //
    // NOTE: Abstractly only euqality should be required for Sets, but since they are internally implemented
    // as trees, comparison is required.
    //
    // NOTE: Records cannot inherit from abstract classes, and F# doesn't have mixins/traits, so that sort of approach doesn't work.
    type RecordEqualityComparison<'T, 'K when 'K: equality and 'K: comparison>(this: 'T, keyFn: 'T -> 'K) =
        #if FABLE_COMPILER
        member _.SetEquals (_thatObj: obj) =
            failwith "Not supported in Fable"
        #else
        member _.SetEquals (thatObj: obj) =
            match thatObj with
            | :? 'T as that -> keyFn this = keyFn that
            | _ -> false
            #endif

        member _.SetGetHashCode = hash (keyFn this)

        // can't use the System.IComparable<'T>'s version of CompareTo because you get the following error:
        // The type 'YourType' does not support the 'comparison' constraint. For example, it does not support
        // the 'System.IComparable' interface
        #if FABLE_COMPILER
        member _.SetCompareTo (_thatObj: obj) =
            failwith "Not supported in Fable"
        #else
        member _.SetCompareTo (thatObj: obj) =
            match thatObj with
            | :? 'T as that -> compare (keyFn this) (keyFn that)
            | _ -> invalidArg "thatObj" "cannot compare values of different types"
        #endif

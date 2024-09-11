[<AutoOpen>]
module Operators

// Operator to invoke implicit type conversion operators that are defined in C#
let inline (!?) (x:^a) : ^b = ((^a or ^b) : (static member op_Implicit : ^a -> ^b) x)

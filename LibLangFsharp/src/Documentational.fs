[<AutoOpen>]
module Documentational

// To make it clear what the "unit" you're returning is supposed to mean
let Noop = ()
let Nothing = ()
let NoopFn = fun () -> Noop

let cannotHappenTypeNarrowing<'T> : 'T = failwith "Ugh, F# doesn't do type narrowing... this match case should have been excluded as a possibility by the compiler"

let shouldNotReachHereBecause<'T> (reason: string) : 'T = sprintf "It should not reach here because <%s>" reason |> failwith

let notReachableButNecessaryForBackendModellingReasons<'T> : 'T = failwith "It should not reach here but backend modelling needs it."

let exceptionForStackTrace () : exn =
    exn "Exception for stack trace"
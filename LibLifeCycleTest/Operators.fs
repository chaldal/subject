[<AutoOpen>]
module Operators

let ``💣``<'T> : 'T =
    failwith "KABOOM! Should not reach"

// Just a signal that all is good, in tests
let ``👍`` : unit =
    ()
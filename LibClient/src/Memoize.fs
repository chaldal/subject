module LibClient.Memoize

open Fable.Core

[<Import("default","fast-memoize"); Emit("$0")>]
let memoize<'a1, 'b> :('a1 -> 'b) -> ('a1 -> 'b) = jsNative

let memoize2 (fn: 'a -> 'b -> 'c) : ('a -> 'b -> 'c) =
  let mem = memoize (fun (a, b) -> fn a b)
  (fun a b -> mem (a, b))

let memoize3 (fn: 'a -> 'b -> 'c -> 'd) : ('a -> 'b -> 'c -> 'd) =
  let mem = memoize (fun (a, b, c) -> fn a b c)
  (fun a b c -> mem (a, b, c))

let memoize4 (fn: 'a -> 'b -> 'c -> 'd -> 'e) : ('a -> 'b -> 'c -> 'd -> 'e) =
  let mem = memoize (fun (a, b, c, d) -> fn a b c d)
  (fun a b c d -> mem (a, b, c, d))

let memoize5 (fn: 'a -> 'b -> 'c -> 'd -> 'e -> 'f) : ('a -> 'b -> 'c -> 'd -> 'e -> 'f) =
  let mem = memoize (fun (a, b, c, d, e) -> fn a b c d e)
  (fun a b c d e -> mem (a, b, c, d, e))

let memoize6 (fn: 'a -> 'b -> 'c -> 'd -> 'e -> 'f -> 'g) : ('a -> 'b -> 'c -> 'd -> 'e -> 'f -> 'g) =
  let mem = memoize (fun (a, b, c, d, e, f) -> fn a b c d e f)
  (fun a b c d e f -> mem (a, b, c, d, e, f))

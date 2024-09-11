module LibClient.Services.AudioService

open Fable.Core
[<Emit("(new Audio($0)).play();")>]
let playSound(_filename: string) : unit = ()

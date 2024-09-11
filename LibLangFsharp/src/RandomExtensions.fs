[<AutoOpen>]
module RandomExtensions

open System
open System.Runtime.CompilerServices

[<Extension>]
type RandomExtensions =
    [<Extension>]
    static member NextUInt16(this: Random, minValue: uint16, maxValue: uint16) : uint16 =
        this.Next(int minValue, int maxValue)
        |> uint16
    
    [<Extension>]
    static member NextByte(this: Random, minValue: byte, maxValue: byte) : byte =
        this.Next(int minValue, int maxValue)
        |> byte

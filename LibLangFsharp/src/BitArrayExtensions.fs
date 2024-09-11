[<AutoOpen>]
module BitArrayExtensions

open System.Collections

#if !FABLE_COMPILER

module BitArray =
    let toByteArray (bitArray: BitArray) : byte[] =
        let result =
            let bitCount = bitArray.Count
            Array.create<byte>
                (bitCount / 8 + if bitCount % 8 = 0 then 0 else 1)
                0uy

        bitArray.CopyTo(result, 0)
        result

#endif

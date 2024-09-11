module LibLangFsharp.BloomFilter

open System

type BitArray = System.Collections.BitArray

type HashContextFunction<'T, 'C> = 'T -> 'C
type HashFunction<'C>            = 'C -> int

#if !FABLE_COMPILER

type BloomFilter<'T, 'C>(bitArray: BitArray, hashContextFunction: HashContextFunction<'T, 'C>, hashFunctions: List<HashFunction<'C>>) =
    do
        if bitArray.Length > Int32.MaxValue then
            failwithf
                "Size exceeds the maximum of a 32bit Int (size is %d)"
                bitArray.Length

    member __.Add (t: 'T) : unit =
        let ctx = hashContextFunction t
        hashFunctions
        |> List.iter (fun f -> bitArray.Set(f ctx, true))

    member this.Add (ts: seq<'T>) : unit =
        ts |> Seq.iter this.Add

    member __.Contains (t: 'T) : bool =
        let rec loop (context: 'C) (hashFunctions: List<HashFunction<'C>>) : bool =
            match hashFunctions with
            | [] ->
                true
            | func :: others ->
                if not bitArray.[func context] then
                    false
                else
                    loop context others

        loop (hashContextFunction t) hashFunctions

    member this.NotContains (t: 'T) : bool =
        this.Contains t |> not

    member internal __.Size = bitArray.Length

    member __.ToByteArray () : byte[] = BitArray.toByteArray bitArray

    member internal __.HashFunctionCount = byte (hashFunctions.Length)

    member internal __.BitArray = bitArray

    override this.GetHashCode () = hash(this.ToByteArray())

    override this.Equals (input: obj) : bool =
        match input with
        | :? BloomFilter<'T, 'C> as that ->
               this.GetType().FullName = that.GetType().FullName
            && this.ToByteArray()      = that.ToByteArray()
        | _ -> false

module BloomFilter =
    let serialize (bloomFilter: BloomFilter<_, _>) : byte[] =
        Array.append
            [|bloomFilter.HashFunctionCount|]
            (bloomFilter.ToByteArray())

    // Serialize an empty bloomfilter (where none of the bits are set)
    let serializeEmpty (bloomFilter: BloomFilter<_, _>) : byte[] =
        let bloomfilterArrayLenBytes = BitConverter.GetBytes(bloomFilter.ToByteArray().Length)

        Array.append
            [|bloomFilter.HashFunctionCount|]
            bloomfilterArrayLenBytes

let private makeGuidHashFunctions
        (targetBitArraySize: uint32)
        (hashFunctionCount: byte)
        : List<HashFunction<byte[]>> =
    if hashFunctionCount < 2uy || hashFunctionCount > 7uy then
        // if we want to remove these limitations, then:
        // for k = 1, do a special case hash by combining 2 int64s
        // for k > 7, copy the first 3 bytes to the end of the 16-byte guid array, to create a 19-byte array
        // for k > 16, further break the 16 byte guid array to bits
        // for k > 128, further combine individual bits to the bits
        // next to them to get a rectilinear distribution
        // EXTEND ONLY WHEN NEEDED. OK TO THROW OTHERWISE
        failwith "This algorithm is only designed for generating at least 2 hash functions and at most 7 hash functions"

    let guidBytesPerHashFunction  = byte(floor(16.0 / (float hashFunctionCount)))
    let targetBitArraySegmentSize = targetBitArraySize / (uint32 hashFunctionCount)

    seq { 0uy .. (hashFunctionCount - 1uy) }
    |> Seq.map
        (fun funNum ->
            let startIndex = funNum * guidBytesPerHashFunction
            let isLastFunc = (funNum = hashFunctionCount - 1uy)

            let targetBitArrayStartIndex = (int funNum) * (int targetBitArraySegmentSize)
            fun (ctx: byte[]) ->
                let valToHash = BitConverter.ToUInt32(ctx, int startIndex)
                let anotherValToHash =
                    if isLastFunc then
                        BitConverter.ToUInt32(ctx, 12)
                    else
                        0u

                targetBitArrayStartIndex + int ((valToHash ^^^ anotherValToHash) % targetBitArraySegmentSize)
        )
    |> Seq.toList

type GuidBloomFilter(size: uint32, hashFunctionCount: byte) =
    inherit BloomFilter<Guid, byte[]>(
        BitArray (int size),
        (fun guid -> guid.ToByteArray()),
        (makeGuidHashFunctions size hashFunctionCount)
    )

let private makeStringHashFunctions
        (bitArraySize:      uint32)
        (hashFunctionCount: byte)
        : List<HashFunction<byte[]>> =
    let bitArraySegmentSize = int(bitArraySize / (uint32 hashFunctionCount))

    seq { 0 .. (int hashFunctionCount - 1) }
    |> Seq.map
        (fun funcIndex ->
            let targetBitArraySegmentStartIndex = funcIndex * bitArraySegmentSize

            fun (bytes: byte[]) ->
                let hashedValue =
                    let splitBytes = Array.splitInto (int hashFunctionCount) bytes
                    match splitBytes.Length <= funcIndex with
                    | true -> 0
                    | false ->
                        splitBytes.[funcIndex]
                        |> Seq.chunkBySize 4
                        |> Seq.map
                            (fun chunk ->
                                let convertableChunk =
                                    match 4 - chunk.Length with
                                    | 0 -> chunk
                                    | padding ->
                                        Array.zeroCreate padding
                                        |> Array.append chunk

                                BitConverter.ToUInt32(convertableChunk, 0)
                                |> float
                            )
                        |> Seq.average
                        |> int
                        |> abs

                targetBitArraySegmentStartIndex + (hashedValue % bitArraySegmentSize)
        )
    |> Seq.toList

type StringBloomFilter internal (bitArray: BitArray, hashFunctionCount: byte) =
    inherit BloomFilter<string, byte[]>(
        bitArray,
        (fun str -> System.Text.Encoding.UTF8.GetBytes str),
        (makeStringHashFunctions (uint32 bitArray.Length) hashFunctionCount)
    )

    new (size: uint32, hashFunctionCount: byte) =
        StringBloomFilter(BitArray (int size), hashFunctionCount)

    static member Deserialize (bytes: byte[]) : StringBloomFilter =
        let (hashFunctionCountArray, byteArray) = Array.splitAt 1 bytes
        let hashFunctionCount = hashFunctionCountArray.[0]

        StringBloomFilter(BitArray byteArray, hashFunctionCount)

    // Deserialize an empty bloomfilter (where none of the bits are set)
    static member DeserializeEmpty (bytes: byte[]) : StringBloomFilter =
        let hashFunctionCount = bytes.[0]
        let arrayLength = BitConverter.ToInt32(bytes, 1)
        let byteArray = Array.create arrayLength 0uy

        StringBloomFilter(BitArray byteArray, hashFunctionCount)


#else

// Hacking... basically Fable doesn't have BitArray support, and we don't need the BloomFilter on the
// front end anyway, so we'll make front-end facing type a simple unit, and hack the Thoth encoder to ignore it.

type StringBloomFilter = NotUsedOnTheFrontEnd

#endif

#if !FABLE_COMPILER

open CodecLib

type StringBloomFilter with
    static member get_Codec () : Codec<_, StringBloomFilter> =
        (Codec.create (StringBloomFilter.Deserialize >> Ok) BloomFilter.serialize)
        |> Codec.compose Codecs.base64Bytes

#endif

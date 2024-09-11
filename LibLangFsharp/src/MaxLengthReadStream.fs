namespace LibLangFSharp
#if !FABLE_COMPILER

open System
open System.IO
open System.Threading.Tasks
open System.Threading

(*
  NOTE:
    READ BEFORE CHANGING
    Streams are fundamental, and a tiny bug would cause very hard-to-debug errors in the most unexpected places.
    All changes should be associated with unit tests. Scroll to the end of the file to see unit test instructions.
*)

(*
  MOTIVATION:
    This is primarily designed to decode compressed streams from requests that could contain a Zip Bomb
    https://en.wikipedia.org/wiki/Zip_bomb

    The stream wraps over an underlying stream and crashes if more than MaxLength number of bytes are deflated
*)

(*
How to test a zip bomb (in this case, a gzip bomb)
1. Use linux dd to generate a 100 MB file, gzipped, that would be just 100KB:
   dd if=/dev/zero bs=1M count=100 | gzip > ./bomb.gz
2. Make a request via curl, e.g.:
   curl -k --data-binary @bomb.gz -H "Content-Encoding: gzip" https://localhost:5001/api/v1/subject/Category/all

Observations:
1. ASP.NET Core / Kestrel won't prevent the upload, as its smaller than MaxRequestBodySize
2. This stream will kick in and prevent the bomb by throwing a MaxLengthReadStreamException
*)

exception MaxLengthReadStreamException of MaxLength: int
    with
        override this.Message =
            sprintf "Cant read more than %d bytes from this stream" this.MaxLength

type MaxLengthReadStream(stream: Stream, maxLength: int) =
    inherit Stream()

    let failNoWrite () =
        invalidOp "Only Reads are supported"

    let failNoSeek () =
        invalidOp "Seeking is not supported"

    let failMaxRead () =
        MaxLengthReadStreamException maxLength
        |> raise

    let mutable readSoFar = 0

    override _.Read(buffer, offset, count) =
        let allowedToRead = maxLength - readSoFar

        if allowedToRead <= 0 then
            failMaxRead ()
        else
            ()

        let actuallyToRead = if count > allowedToRead then allowedToRead else count
        let numRead = stream.Read(buffer, offset, actuallyToRead)

        readSoFar <- readSoFar + numRead

        // If we're max'ed out, ensure nothing else is left to be read
        if count > actuallyToRead && stream.ReadByte() <> -1 then
            failMaxRead ()

        numRead


    // override _.Read/

    override _.ReadAsync(buffer, offset, count, cancellationToken) =
        backgroundTask {
            let allowedToRead = maxLength - readSoFar

            if allowedToRead <= 0 then
                failMaxRead ()
            else
                ()

            let actuallyToRead = if count > allowedToRead then allowedToRead else count
            let! numRead = stream.ReadAsync(buffer, offset, actuallyToRead, cancellationToken)

            readSoFar <- readSoFar + numRead

            // If we're max'ed out, ensure nothing else is left to be read
            if count > actuallyToRead && stream.ReadByte() <> -1 then
                failMaxRead ()

            return numRead
        }

    override _.ReadAsync(memory, cancellationToken) =
        backgroundTask {
            let allowedToRead = maxLength - readSoFar

            if allowedToRead <= 0 then
                failMaxRead ()
            else
                ()

            let slice =
                if memory.Length > allowedToRead then
                    memory.Slice(0, allowedToRead)
                else
                    memory

            let! numRead = stream.ReadAsync(slice, cancellationToken)

            readSoFar <- readSoFar + numRead

            // If we're max'ed out, ensure nothing else is left to be read
            if slice.Length < memory.Length then
                // Try reading 1 more byte
                let anotherSlice = memory.Slice(allowedToRead, 1)
                let! anotherRead = stream.ReadAsync(anotherSlice, cancellationToken)
                if anotherRead > 0 then
                    failMaxRead ()
                else
                    ()
            else
                ()

            return numRead
        } |> System.Threading.Tasks.ValueTask<int>

    override _.ReadByte() =
        let res = stream.ReadByte()
        if readSoFar >= maxLength && res <> -1 then
            failMaxRead ()
        else
            readSoFar <- readSoFar + 1
            res

    override this.CopyTo(destination, bufferSize) =
        let actualBufferSize = min bufferSize maxLength
        let buffer = Array.zeroCreate<byte> actualBufferSize
        let mutable shouldLoop = true

        while shouldLoop do
            let numBytesRead = this.Read(buffer, 0, actualBufferSize)
            if numBytesRead = 0 then
                shouldLoop <- false
            else
                destination.Write(buffer, 0, numBytesRead)

                if readSoFar = maxLength then
                    // Ensure nothing else can be read
                    if this.ReadByte() <> -1 then
                        failMaxRead ()

    override this.CopyToAsync(destination, bufferSize, cancellationToken) =
        backgroundTask {
            let actualBufferSize = min bufferSize maxLength
            let buffer = Array.zeroCreate<byte> actualBufferSize
            let memory = Memory buffer
            let roMemory = ReadOnlyMemory buffer
            let mutable shouldLoop = true

            while shouldLoop do
                let! numBytesRead = this.ReadAsync(memory, cancellationToken)
                if numBytesRead = 0 then
                    shouldLoop <- false
                else
                    let writeSlice = roMemory.Slice(0, numBytesRead)
                    do! destination.WriteAsync(writeSlice, cancellationToken)

                    if readSoFar = maxLength then
                        // Ensure nothing else can be read
                        let! anotherRead = this.ReadAsync(memory.Slice(0, 1), cancellationToken)
                        if anotherRead <> 0 then
                            failMaxRead ()
        }

    override _.CanRead with get() = stream.CanRead
    override _.CanTimeout with get() = stream.CanTimeout
    override _.ReadTimeout with get() = stream.ReadTimeout
    override _.ReadTimeout with set(value) = stream.ReadTimeout <- value
    override _.WriteTimeout with get() = failNoWrite()
    override _.WriteTimeout with set(_) = failNoWrite()
    override _.CanSeek with get() = false
    override _.CanWrite with get() = false
    override _.Length with get() = stream.Length
    override _.Position with get() = stream.Position
    override _.Position with set(_) = failNoSeek()
    override _.Seek(_, _) = failNoSeek()
    override _.Flush() = failNoWrite()
    override _.FlushAsync(_) = failNoWrite()
    override _.SetLength(_) = failNoWrite()
    override _.Write(_, _, _) = failNoWrite()
    override _.WriteByte(_) = failNoWrite()
    override _.Dispose(_) = stream.Dispose()
    override _.DisposeAsync() = stream.DisposeAsync()

(*
// These need to be moved to a test suite that targets the framework
// Until then, to test:
//   - This whole module is self-contained
//   - Copy this file to another F# xunit test project
//   - Make changes, test, copy back (both new code & tests)
//   - Eventually, we'll move this into a test suite within the framework

open Xunit
open FsUnit.Xunit
open System.Threading

let getStreamBuffer (buffer: byte[]) maxLength =
    let ms = new MemoryStream(buffer)
    new MaxLengthReadStream(ms, maxLength)

let createBuffer (size: int) =
    let rnd = System.Random()
    let arr = Array.zeroCreate<byte> size
    rnd.NextBytes arr
    arr

let shouldFailMaxLengthCheck (runner: unit -> 'T) =
    (runner >> ignore) |> should throw typeof<MaxLengthReadStreamException>

let shouldFailMaxLengthCheckAsync (runner: unit -> Task<'T>) =
    backgroundTask {
        try
            let! _ = runner()
            failwith "Expecting MaxLengthReadStreamException, but succeeded"
        with
        | :? MaxLengthReadStreamException ->
            ()
        | ex ->
            failwithf "Expecting MaxLengthReadStreamException, got %A" ex
    }

let shouldFailMaxLengthCheckValueAsync<'T> (runner: unit -> ValueTask<'T>) : Task<unit> =
    backgroundTask {
        try
            let! _ = runner().AsTask()
            failwith "Expecting MaxLengthReadStreamException, but succeeded"
        with
        | :? MaxLengthReadStreamException ->
            ()
        | ex ->
            failwithf "Expecting MaxLengthReadStreamException, got %A" ex
    }

[<Fact>]
let ``Test ReadByte with readable bytes``() =
    let buffer = [| 5uy; 3uy; 5uy |]
    let stream = getStreamBuffer buffer 10

    stream.ReadByte() |> byte |> should equal buffer.[0]
    stream.ReadByte() |> byte |> should equal buffer.[1]
    stream.ReadByte() |> byte |> should equal buffer.[2]

[<Fact>]
let ``Test ReadByte with unreadable bytes``() =
    let buffer = createBuffer 3
    let stream = getStreamBuffer buffer 2

    stream.ReadByte() |> byte |> should equal buffer.[0]
    stream.ReadByte() |> byte |> should equal buffer.[1]
    (fun () -> stream.ReadByte()) |> shouldFailMaxLengthCheck

[<Fact>]
let ``Test ReadByte with readable bytes of max length``() =
    let buffer = createBuffer 3
    let stream = getStreamBuffer buffer 3

    stream.ReadByte() |> byte |> should equal buffer.[0]
    stream.ReadByte() |> byte |> should equal buffer.[1]
    stream.ReadByte() |> byte |> should equal buffer.[2]

[<Fact>]
let ``Test ReadByte with no bytes``() =
    let buffer = Array.empty
    let stream = getStreamBuffer buffer 3
    stream.ReadByte() |> should equal -1

[<Fact>]
let ``Test 1-shot Full Read with readable bytes``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 50

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 30) |> should equal 30
    (buffer = bytes) |> should equal true

[<Fact>]
let ``Test 2-shot Full Read with readable bytes``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 50

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 10) |> should equal 10
    stream.Read(bytes, 10, 20) |> should equal 20
    (buffer = bytes) |> should equal true

[<Fact>]
let ``Test 1-shot Full Read with readable bytes but lower maxlength than destination size``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 50

    let bytes = Array.zeroCreate 60
    stream.Read(bytes, 0, 60) |> should equal 30
    (buffer = (bytes |> Array.take 30)) |> should equal true

[<Fact>]
let ``Test 2-shot Full Read with readable bytes but lower maxlength than destination size``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 50

    let bytes = Array.zeroCreate 60
    stream.Read(bytes, 0, 10) |> should equal 10
    stream.Read(bytes, 10, 50) |> should equal 20
    (buffer = (bytes |> Array.take 30)) |> should equal true

[<Fact>]
let ``Test 2-shot Full Read with readable bytes of max length``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 30

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 10) |> should equal 10
    stream.Read(bytes, 10, 20) |> should equal 20
    (buffer = bytes) |> should equal true

[<Fact>]
let ``Test 1-shot empty Read``() =
    let buffer = createBuffer 0
    let stream = getStreamBuffer buffer 30

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 10) |> should equal 0
    (bytes = (Array.zeroCreate 30)) |> should equal true

[<Fact>]
let ``Test 1-shot Full Read with readable bytes of max length``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 30

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 30) |> should equal 30
    (buffer = bytes) |> should equal true

[<Fact>]
let ``Test 1-shot Full Read with readable bytes of max - 1 length``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 31

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 30) |> should equal 30
    (buffer = bytes) |> should equal true

[<Fact>]
let ``Test 2-shot Full Read with readable bytes of max - 1 length``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 31

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 10) |> should equal 10
    stream.Read(bytes, 10, 20) |> should equal 20
    (buffer = bytes) |> should equal true


[<Fact>]
let ``Test 1-shot Full Read with unreadable bytes``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 10

    let bytes = Array.zeroCreate 30
    (fun () -> stream.Read(bytes, 0, 30)) |> shouldFailMaxLengthCheck

[<Fact>]
let ``Test 2-shot Full Read with unreadable bytes``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 20

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 10) |> should equal 10
    (fun () -> stream.Read(bytes, 10, 20)) |> shouldFailMaxLengthCheck
    ((bytes |> Array.take 10) = (buffer |> Array.take 10)) |> should equal true

[<Fact>]
let ``Test 2-shot Full Read with unreadable bytes of max length - 1``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 29

    let bytes = Array.zeroCreate 30
    stream.Read(bytes, 0, 10) |> should equal 10
    (fun () -> stream.Read(bytes, 10, 20)) |> shouldFailMaxLengthCheck
    ((bytes |> Array.take 10) = (buffer |> Array.take 10)) |> should equal true

[<Fact>]
let ``Test 1-shot Full Read with unreadable bytes of max length - 1``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 29

    let bytes = Array.zeroCreate 30
    (fun () -> stream.Read(bytes, 0, 30)) |> shouldFailMaxLengthCheck

[<Fact>]
let ``Test 1-shot Full Async Read with readable bytes``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50

        let bytes = Array.zeroCreate 30
        let! numRead = stream.ReadAsync(bytes, 0, 30)
        numRead |> should equal 30
        (buffer = bytes) |> should equal true
    }

[<Fact>]
let ``Test 2-shot Full Async Read with readable bytes``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 10)
        read1 |> should equal 10
        let! read2 = stream.ReadAsync(bytes, 10, 20)
        read2 |> should equal 20
        (buffer = bytes) |> should equal true
    }

[<Fact>]
let ``Test 2-shot Full Async Read with readable bytes of max length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 30

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 10)
        read1 |> should equal 10
        let! read2 = stream.ReadAsync(bytes, 10, 20)
        read2 |> should equal 20
        (buffer = bytes) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Read with readable bytes but lower maxlength than destination size``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50

        let bytes = Array.zeroCreate 60
        let! read1 = stream.ReadAsync(bytes, 0, 60)
        read1 |> should equal 30
        (buffer = (bytes |> Array.take 30)) |> should equal true
    }

[<Fact>]
let ``Test 2-shot Full Async Read with readable bytes but lower maxlength than destination size``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50

        let bytes = Array.zeroCreate 60
        let! read1 = stream.ReadAsync(bytes, 0, 10)
        read1 |> should equal 10
        let! read2 = stream.ReadAsync(bytes, 10, 50)
        read2 |> should equal 20
        (buffer = (bytes |> Array.take 30)) |> should equal true
    }

[<Fact>]
let ``Test 1-shot empty Async Read``() =
    backgroundTask {
        let buffer = createBuffer 0
        let stream = getStreamBuffer buffer 30

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 10)
        read1 |> should equal 0
        (bytes = (Array.zeroCreate 30)) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Read with readable bytes of max length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 30

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 30)
        read1 |> should equal 30
        (buffer = bytes) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Read with readable bytes of max - 1 length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 31

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 30)
        read1 |> should equal 30
        (buffer = bytes) |> should equal true
    }

[<Fact>]
let ``Test 2-shot Full Async Read with readable bytes of max - 1 length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 31

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 10)
        read1 |> should equal 10
        let! read2 = stream.ReadAsync(bytes, 10, 20)
        read2 |> should equal 20
        (buffer = bytes) |> should equal true
    }


[<Fact>]
let ``Test 1-shot Full Async Read with unreadable bytes``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 10

        let bytes = Array.zeroCreate 30
        do! (fun () -> stream.ReadAsync(bytes, 0, 30)) |> shouldFailMaxLengthCheckAsync
    }

[<Fact>]
let ``Test 2-shot Full Async Read with unreadable bytes``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 20

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 10)
        read1 |> should equal 10
        do! (fun () -> stream.ReadAsync(bytes, 10, 20)) |> shouldFailMaxLengthCheckAsync
        ((bytes |> Array.take 10) = (buffer |> Array.take 10)) |> should equal true
    }

[<Fact>]
let ``Test 2-shot Full Async Read with unreadable bytes of max length - 1``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 29

        let bytes = Array.zeroCreate 30
        let! read1 = stream.ReadAsync(bytes, 0, 10)
        read1 |> should equal 10
        do! (fun () -> stream.ReadAsync(bytes, 10, 20)) |> shouldFailMaxLengthCheckAsync
        ((bytes |> Array.take 10) = (buffer |> Array.take 10)) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Read with unreadable bytes of max length - 1``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 29

        let bytes = Array.zeroCreate 30
        do! (fun () -> stream.ReadAsync(bytes, 0, 30)) |> shouldFailMaxLengthCheckAsync
    }

[<Fact>]
let ``Test Full Async Memory Read with readable bytes``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50

        let memory = Array.zeroCreate 30 |> Memory<byte>
        let! numRead = stream.ReadAsync(memory, CancellationToken.None)
        numRead |> should equal 30
        (buffer = memory.ToArray()) |> should equal true
    }

[<Fact>]
let ``Test 1-shot empty Async Memory Read``() =
    backgroundTask {
        let buffer = createBuffer 0
        let stream = getStreamBuffer buffer 30

        let memory = Array.zeroCreate 30 |> Memory<byte>
        let! read1 = stream.ReadAsync(memory, CancellationToken.None)
        read1 |> should equal 0
        (memory.ToArray() = (Array.zeroCreate 30)) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Memory Read with readable bytes of max length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 30

        let memory = Array.zeroCreate 30 |> Memory<byte>
        let! read1 = stream.ReadAsync(memory, CancellationToken.None)
        read1 |> should equal 30
        (buffer = memory.ToArray()) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Memory Read with readable bytes of max - 1 length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 31

        let memory = Array.zeroCreate 30 |> Memory<byte>
        let! read1 = stream.ReadAsync(memory, CancellationToken.None)
        read1 |> should equal 30
        (buffer = memory.ToArray()) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Memory Read with readable bytes but lower maxlength than destination size``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50

        let memory = Array.zeroCreate 60 |> Memory<byte>
        let! read1 = stream.ReadAsync(memory, CancellationToken.None)
        read1 |> should equal 30
        (buffer = (memory.ToArray() |> Array.take 30)) |> should equal true
    }

[<Fact>]
let ``Test 1-shot Full Async Memory Read with unreadable bytes``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 10

        let memory = Array.zeroCreate 30 |> Memory<byte>
        do! (fun () -> stream.ReadAsync(memory, CancellationToken.None)) |> shouldFailMaxLengthCheckValueAsync
    }

[<Fact>]
let ``Test 1-shot Full Async Memory Read with unreadable bytes of max length - 1``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 29

        let memory = Array.zeroCreate 30 |> Memory<byte>
        do! (fun () -> stream.ReadAsync(memory, CancellationToken.None)) |> shouldFailMaxLengthCheckValueAsync
    }

[<Fact>]
let ``Test CopyTo with readable bytes and buffer size lower more than input size and lower than max length``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 50
    let destination = new MemoryStream()
    stream.CopyTo(destination, 40)
    (buffer = destination.ToArray()) |> should equal true

[<Fact>]
let ``Test CopyTo with readable bytes and buffer size lower than input size``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 50
    let destination = new MemoryStream()
    stream.CopyTo(destination, 10)
    (buffer = destination.ToArray()) |> should equal true

[<Fact>]
let ``Test CopyTo with readable bytes and buffer size greater than max length``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 50
    let destination = new MemoryStream()
    stream.CopyTo(destination, 1000)
    (buffer = destination.ToArray()) |> should equal true

[<Fact>]
let ``Test CopyTo with unreadable bytes and buffer size lower than input size``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 20
    let destination = new MemoryStream()
    (fun () -> stream.CopyTo(destination, 10)) |> shouldFailMaxLengthCheck

[<Fact>]
let ``Test CopyTo with unreadable bytes and buffer size more than input size``() =
    let buffer = createBuffer 30
    let stream = getStreamBuffer buffer 20
    let destination = new MemoryStream()
    (fun () -> stream.CopyTo(destination, 50)) |> shouldFailMaxLengthCheck

[<Fact>]
let ``Test CopyToAsync with readable bytes and buffer size lower more than input size and lower than max length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50
        let destination = new MemoryStream()
        do! stream.CopyToAsync(destination, 40)
        (buffer = destination.ToArray()) |> should equal true
    }

[<Fact>]
let ``Test CopyToAsync with readable bytes and buffer size lower than input size``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50
        let destination = new MemoryStream()
        do! stream.CopyToAsync(destination, 10)
        (buffer = destination.ToArray()) |> should equal true
    }

[<Fact>]
let ``Test CopyToAsync with readable bytes and buffer size greater than max length``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 50
        let destination = new MemoryStream()
        do! stream.CopyToAsync(destination, 1000)
        (buffer = destination.ToArray()) |> should equal true
    }

[<Fact>]
let ``Test CopyToAsync with unreadable bytes and buffer size lower than input size``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 20
        let destination = new MemoryStream()
        do! (
            fun () ->
                backgroundTask {
                    do! stream.CopyToAsync(destination, 10)
                    return ()
                }) |> shouldFailMaxLengthCheckAsync
    }

[<Fact>]
let ``Test CopyToAsync with unreadable bytes and buffer size more than input size``() =
    backgroundTask {
        let buffer = createBuffer 30
        let stream = getStreamBuffer buffer 20
        let destination = new MemoryStream()
        do! (
            fun () ->
                backgroundTask {
                    do! stream.CopyToAsync(destination, 50)
                    return ()
                }) |> shouldFailMaxLengthCheckAsync
    }
*)
#endif
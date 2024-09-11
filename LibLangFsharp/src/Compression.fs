[<AutoOpen>]
module Compression

let private encoding = System.Text.Encoding.UTF8

#if !FABLE_COMPILER
let gzipCompressUtf8String (input: string) : byte[] =
    let stringBytes = encoding.GetBytes input
    use outputStream = new System.IO.MemoryStream()
    let gzipStream   = new System.IO.Compression.GZipStream(outputStream, System.IO.Compression.CompressionMode.Compress)
    gzipStream.Write(stringBytes, 0, stringBytes.Length)
    gzipStream.Close()
    outputStream.ToArray()

let gzipDecompressToUtf8String (bytes: byte[]) : string =
    use inputStream  = new System.IO.MemoryStream(bytes)
    use outputStream = new System.IO.MemoryStream()
    let gzipStream   = new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Decompress)
    gzipStream.CopyTo outputStream
    gzipStream.Close()
    outputStream.ToArray()|> encoding.GetString
#endif
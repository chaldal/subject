namespace System.Reflection

open System.Reflection
open System.Runtime.CompilerServices

#if !FABLE_COMPILER

[<Extension>]
type AssemblyExtensions =
    [<Extension>]
    static member LoadResource (this: Assembly, resourcePath: string) : byte[] =
        use resourceStream = this.GetManifestResourceStream resourcePath
        use memoryStream   = new System.IO.MemoryStream()
        resourceStream.CopyTo memoryStream
        memoryStream.ToArray()

#endif

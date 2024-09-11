module LibClient.Services.StorageService

// NOTE we are forced by the Fable runtime to take encode and decode functions as
// parameters, because unless we inline the get/put functions, the encoder cannot
// get the type information for the value, and we cannot inline, because apparently
// in F# you cannot have `abstract inline member`, nor can you have an `abstract member`
// that is then implemented with `override member inline`. Sigh.
[<AbstractClass>]
type StorageService() =
    abstract member Get<'T> : key: string -> decode: (string -> Result<'T, string>) -> Async<Option<'T>>
    abstract member Put<'T> : key: string -> value: 'T -> encode: ('T -> string) -> Async<unit>
    abstract member Remove : key: string -> Async<unit>

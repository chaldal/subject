[<AutoOpen>]
module AssertExtensions

open Xunit

module Assert =
    let Fail (message: string) : unit =
        Assert.True(false, message)

    let Success () : unit =
        Assert.True true

[<AutoOpen>]
module GuidExtensions

open System

type Guid with
    member this.ToTinyUuid() =
        this.ToByteArray()
        |> Convert.ToBase64String
        |> fun str -> str.Substring(0, str.Length - 2) // remove trailing == padding
        |> fun str -> str.Replace('+', '-')            // escape
        |> fun str -> str.Replace('/', '_')            // escape

    static member FromTinyUuid(tinyUuid: string) : Guid =
        tinyUuid
        |> fun str -> str.Replace('_', '/')
        |> fun str -> str.Replace('-', '+')
        |> sprintf "%s=="
        |> Convert.FromBase64String
        |> Guid

    static member TryFromTinyUuid(tinyUuid: string) : Option<Guid> =
        try
            tinyUuid
            |> fun str -> str.Replace('_', '/')
            |> fun str -> str.Replace('-', '+')
            |> sprintf "%s=="
            // TODO: TryFromBase64 instead of try-catch, requires .NET 5
            |> Convert.FromBase64String
            |> Guid
            |> Some
        with
        | _ ->
            None

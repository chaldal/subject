[<AutoOpen>]
module PrimitivesExtensions

let private base36Digits = new string(['0' .. '9'] @ ['a' .. 'z'] |> Array.ofSeq)
let private digit36Map = base36Digits |> Seq.mapi (fun i c -> (c, i)) |> Map.ofSeq
let private radix36 = base36Digits.Length

let private toBase36 number =
    let result =
        number
        |> abs
        |> List.unfold (function | 0 -> None | i -> Some (base36Digits.[i % radix36], i / radix36))
        |> List.rev
        |> Array.ofList
        |> System.String
    if result = ""
    then
        "0"
    elif number > 0 then
        result
    else
        sprintf "-%s" result

let private fromBase36 (str: string) =
    str.ToLower()
    |> Seq.rev
    |> Seq.fold (fun (pow, total) c -> (pow * 36, total + pow * (digit36Map.Item c))) (1, 0)
    |> snd

type System.Int32 with
    // NOTE if you try using TryParse in the implementation here, Fable crashes with an obscure
    // "Unexpected end of JSON input" error which is pretty hard to debug.
    static member ParseOption (value: string) : Option<System.Int32> =
        try
            Some (System.Int32.Parse value)
        with
        | _ -> None

    member this.ToBase36() : System.String =
        toBase36 this

    static member ParseBase36 (base36Str: string) : System.Int32 =
        fromBase36 base36Str

type System.UInt16 with
    // NOTE if you try using TryParse in the implementation here, Fable crashes with an obscure
    // "Unexpected end of JSON input" error which is pretty hard to debug.
    static member ParseOption (value: string) : Option<System.UInt16> =
        try
            Some (System.UInt16.Parse value)
        with
        | _ -> None

type System.Single with
    // NOTE if you try using TryParse in the implementation here, Fable crashes with an obscure
    // "Unexpected end of JSON input" error which is pretty hard to debug.
    static member ParseOption (value: string) : Option<System.Single> =
        try
            Some (System.Single.Parse value)
        with
        | _ -> None

type System.Double with
    // NOTE if you try using TryParse in the implementation here, Fable crashes with an obscure
    // "Unexpected end of JSON input" error which is pretty hard to debug.
    static member ParseOption (value: string) : Option<System.Double> =
        try
            Some (System.Double.Parse value)
        with
        | _ -> None

type System.Decimal with
    // NOTE if you try using TryParse in the implementation here, Fable crashes with an obscure
    // "Unexpected end of JSON input" error which is pretty hard to debug.
    static member ParseOption (value: string) : Option<System.Decimal> =
        try
            Some (System.Decimal.Parse value)
        with
        | _ -> None

type System.Guid with
    // NOTE if you try using TryParse in the implementation here, Fable crashes with an obscure
    // "Unexpected end of JSON input" error which is pretty hard to debug.
    static member ParseOption (value: string) : Option<System.Guid> =
        try
            Some (System.Guid.Parse value)
        with
        | _ -> None

type System.Boolean with
    static member ParseOption (value: string) : Option<System.Boolean> =
        match value with
        | "true"  -> Some true
        | "false" -> Some false
        | _       -> None


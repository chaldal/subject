[<AutoOpen>]
module EmailAddressModule

open System.Text.RegularExpressions

// https://stackoverflow.com/a/201378/6493611
let emailPattern = """^(?:[a-z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$"""
let emailRegex = Regex(emailPattern, RegexOptions.Compiled)

[<Struct>]
type EmailAddress = private {
    Value_: string
}
with
    member this.Value : string =
        this.Value_

    override this.ToString() =
        this.Value

type EmailAddressValidationError =
| EmptyString
| NoAtSymbol
| MultipleAtSymbols
| AtSymbolAtStart
| AtSymbolAtEnd
| NotAValidEmail

module EmailAddress =
    let tryOfString (source: string) : Result<EmailAddress, EmailAddressValidationError> =
        if System.String.IsNullOrWhiteSpace source then
            Error EmptyString
        else
            let cleanedSource = source.Trim().ToLower()

            // Taking a leaf out of NET core's book here by keeping it simple, but largely effective, even if not to the letter of the RFC.
            // https://github.com/dotnet/runtime/blob/main/src/libraries/System.ComponentModel.Annotations/src/System/ComponentModel/DataAnnotations/EmailAddressAttribute.cs
            match cleanedSource.IndexOf("@"), cleanedSource.LastIndexOf("@") with
            | -1, -1                                            -> Error NoAtSymbol
            | x, y when x <> y && y > -1                        -> Error MultipleAtSymbols
            | 0, _                                              -> Error AtSymbolAtStart
            | x, _ when x = cleanedSource.Length - 1            -> Error AtSymbolAtEnd
            | _, _ when not (emailRegex.IsMatch(cleanedSource)) -> Error NotAValidEmail
            | _                                                 -> { Value_ = cleanedSource } |> Ok


// CODECs

#if !FABLE_COMPILER

open CodecLib

type EmailAddress
with
    static member get_Codec () : Codec<_, EmailAddress> =
        Codec.create (fun x -> EmailAddress.tryOfString x |> Result.mapError (fun _ -> Uncategorized "Failed parsing")) (fun x -> x.Value_)
        |> Codec.compose Codecs.string

#endif
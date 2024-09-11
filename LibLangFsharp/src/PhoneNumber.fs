[<AutoOpen>]
module PhoneNumberModule

open System.Text.RegularExpressions

type ValidatedPhoneNumber =
    private
    | BdMobileNumber          of PhoneNumber: string
    | BdLandlineNumber        of PhoneNumber: string
    | UsPhoneNumber           of PhoneNumber: string
    | OtherCountryPhoneNumber of PhoneNumber: string
with
    member this.Value =
        match this with
        | BdMobileNumber          ph
        | BdLandlineNumber        ph
        | UsPhoneNumber           ph
        | OtherCountryPhoneNumber ph
            -> ph

    override this.ToString() =
        this.Value

let private validSecondDigitForBdMobileNumbers   = [ '1'; '3'; '4'; '5'; '6'; '7'; '8'; '9' ] |> Seq.map (fun i -> (i, true)) |> dict
let private validSecondDigitForBdLandlineNumbers = [ '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9' ] |> Seq.map (fun i -> (i, true)) |> dict

type PhoneNumberValidationError =
| EmptyString
| InvalidSecondDigitForBdMobileNumber
| BdMobileNumbersNeedToBe13DigitsLong
| InvalidSecondDigitForBdLandlineNumber
| BdLandlineNumbersNeedToBe11DigitsLong
| UsPhoneNumberRequires11Digits
| InvalidBdPhoneNumber
| NumberTooShort
with
    member this.ToDisplayString : string =
        match this with
        | PhoneNumberValidationError.EmptyString -> "Cannot be empty"
        | InvalidSecondDigitForBdMobileNumber    -> "Invalid second digit for BD mobile number"
        | BdMobileNumbersNeedToBe13DigitsLong    -> "BD mobile numbers must be at least 13 digits long"
        | InvalidSecondDigitForBdLandlineNumber  -> "Second digit is invalid for BD landline number"
        | BdLandlineNumbersNeedToBe11DigitsLong  -> "BD landline number must contain 11 digits"
        | UsPhoneNumberRequires11Digits          -> "US phone number must contain 11 digits"
        | InvalidBdPhoneNumber                   -> "Invalid BD phone number"
        | NumberTooShort                         -> "Too short"

let private nonNumericChar =
#if FABLE_COMPILER
    Regex(@"\D+")
#else
    Regex(@"\D+", RegexOptions.Compiled)
#endif

[<Struct>]
type PhoneNumber = private {
    Value_: ValidatedPhoneNumber
}
with
    member this.Value : string =
        this.Value_.Value

    member this.IsBdMobileNumber : bool =
        match this.Value_ with
        | BdMobileNumber _ -> true
        | _                -> false

    member this.TrimCountryCode : string =
        let Trim (phoneNumber: string) (countryCode: string) =
            match phoneNumber.StartsWith(countryCode) with
            | true -> phoneNumber.[countryCode.Length..]
            | false -> phoneNumber

        match this.Value_ with
        | BdLandlineNumber _
        | BdMobileNumber _ ->
            Trim this.Value "+88"
        | UsPhoneNumber _ ->
            Trim this.Value "+1"
        | OtherCountryPhoneNumber _ ->
            this.Value

    override this.ToString() =
        this.Value

module PhoneNumber =
    let validatePhoneNumber (phoneNumber: string) : Result<ValidatedPhoneNumber, PhoneNumberValidationError> =
        if System.String.IsNullOrWhiteSpace phoneNumber then
            Error EmptyString
        else
            let phoneNumber    = phoneNumber.Trim()
            let startsWithPlus = phoneNumber.StartsWith("+")

            let phoneNumber =
                phoneNumber
                // os are actually 0s
                |> fun s -> s.Replace('o', '0').Replace('O', '0')
                // Strip out all non-numeric characters
                |> fun s -> nonNumericChar.Replace(s,"")
                // Strip out leading 00
                |> fun s -> if s.StartsWith "00" then s.Substring 2 else s

            if phoneNumber.StartsWith "8801" then
                if phoneNumber.Length = 13 then
                    if validSecondDigitForBdMobileNumbers.ContainsKey (phoneNumber.[4]) then
                        sprintf "+%s" phoneNumber |> BdMobileNumber |> Ok
                    else
                        Error InvalidSecondDigitForBdMobileNumber
                else
                    Error BdMobileNumbersNeedToBe13DigitsLong
            elif phoneNumber.StartsWith "8802" then
                if phoneNumber.Length = 11 then
                    if validSecondDigitForBdLandlineNumbers.ContainsKey (phoneNumber.[4]) then
                        sprintf "+%s" phoneNumber |> BdLandlineNumber |> Ok
                    else
                        Error InvalidSecondDigitForBdLandlineNumber
                else
                    Error BdLandlineNumbersNeedToBe11DigitsLong
            elif phoneNumber.StartsWith "88" then
                Error InvalidBdPhoneNumber
            elif phoneNumber.Length = 11 && phoneNumber.StartsWith "01" then
                if validSecondDigitForBdMobileNumbers.ContainsKey (phoneNumber.[2]) then
                    sprintf "+88%s" phoneNumber |> BdMobileNumber |> Ok
                else
                    Error InvalidSecondDigitForBdMobileNumber
            elif phoneNumber.Length = 9 && phoneNumber.StartsWith "02" then
                if validSecondDigitForBdLandlineNumbers.ContainsKey (phoneNumber.[2]) then
                    sprintf "+88%s" phoneNumber |> BdLandlineNumber |> Ok
                else
                    Error InvalidSecondDigitForBdLandlineNumber
            elif phoneNumber.Length = 10 && phoneNumber.[0] = '1' then
                if validSecondDigitForBdMobileNumbers.ContainsKey (phoneNumber.[1]) then
                    sprintf "+880%s" phoneNumber |> BdMobileNumber |> Ok
                else
                    Error InvalidSecondDigitForBdMobileNumber
            elif phoneNumber.Length = 8 && phoneNumber.[0] = '2' then
                if validSecondDigitForBdLandlineNumbers.ContainsKey (phoneNumber.[1]) then
                    sprintf "+880%s" phoneNumber |> BdLandlineNumber |> Ok
                else
                    Error InvalidSecondDigitForBdLandlineNumber
            elif startsWithPlus && phoneNumber.[0] = '1' then
                if phoneNumber.Length = 11 then
                    sprintf "+%s" phoneNumber |> UsPhoneNumber |> Ok
                else
                    Error UsPhoneNumberRequires11Digits
            elif startsWithPlus then
                if phoneNumber.Length > 6 then
                    sprintf "+%s" phoneNumber |> OtherCountryPhoneNumber |> Ok
                else
                    Error NumberTooShort
            else
                Error InvalidBdPhoneNumber


    let tryOfString (source: string) : Result<PhoneNumber, PhoneNumberValidationError> =
        validatePhoneNumber source |> Result.map (fun x -> { Value_ = x } )

    let ofStringUnsafe (source: string) : PhoneNumber =
        match tryOfString source with
        | Ok phoneNumber ->
            phoneNumber
        | Error phoneNumberValidationError ->
            failwith phoneNumberValidationError.ToDisplayString

/// should use qualified PhoneNumber.validatePhoneNumber instead, left here only for compatibility
let validatePhoneNumber (phoneNumber: string) : Result<ValidatedPhoneNumber, PhoneNumberValidationError> =
    PhoneNumber.validatePhoneNumber phoneNumber

// CODECs

#if !FABLE_COMPILER

open CodecLib

type ValidatedPhoneNumber
with
    static member get_Codec () =
        let validateOnDecode
            (ctor: string -> ValidatedPhoneNumber)
            (encodedPhoneNumberStr: string)
            : ValidatedPhoneNumber =
            let decoded = ctor encodedPhoneNumberStr
            match PhoneNumber.validatePhoneNumber encodedPhoneNumberStr with
            | Ok validated ->
                if decoded = validated then
                    decoded
                else
                    failwithf "invalid phone number decoded: %A vs validated from raw string: %A" decoded validated
            | Error err ->
                failwithf "malformed phone number, can't decode: %A" err

        function
        | BdMobileNumber _ ->
            codec {
                let! payload = reqWith Codecs.string "BdMobileNumber" (function (BdMobileNumber x) -> Some x | _ -> None)
                return (validateOnDecode BdMobileNumber) payload
            }
        | BdLandlineNumber _ ->
            codec {
                let! payload = reqWith Codecs.string "BdLandlineNumber" (function (BdLandlineNumber x) -> Some x | _ -> None)
                return (validateOnDecode BdLandlineNumber) payload
            }
        | UsPhoneNumber _ ->
            codec {
                let! payload = reqWith Codecs.string "UsPhoneNumber" (function (UsPhoneNumber x) -> Some x | _ -> None)
                return (validateOnDecode UsPhoneNumber) payload
            }
        | OtherCountryPhoneNumber _ ->
            codec {
                let! payload = reqWith Codecs.string "OtherCountryPhoneNumber" (function (OtherCountryPhoneNumber x) -> Some x | _ -> None)
                return (validateOnDecode OtherCountryPhoneNumber) payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type PhoneNumber
with
    static member get_Codec () : Codec<_, PhoneNumber> =
        Codec.create (fun x -> Ok { Value_ = x }) (fun x -> x.Value_)
        |> Codec.compose (ValidatedPhoneNumber.get_Codec ())

    // need it because PhoneNumber used as generic parameters in some subjects which requires type hints for serialization
    static member TypeLabel () = "PhoneNumber"

#endif
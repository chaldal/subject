[<AutoOpen>]
module LibLifeCycleTest.GenPhoneNumber

open System
open FsCheck

type private PhoneNumberTypes = BdMobileNumber | BdLandlineNumber | UsPhoneNumber | OtherCountryPhoneNumber

let private bdPhoneNumber =
    gen {
         let! operatorDigit = Gen.elements [ '1'; '3'; '4'; '5'; '6'; '7'; '8'; '9' ]

         let! otherDigits =
             genDigits
             |> Gen.arrayOfLength 8
             |> Gen.map String

         return
             sprintf "+8801%c%s" operatorDigit otherDigits
             |> PhoneNumber.tryOfString
             |> Result.toOption
             |> Option.get
    }

let genBdMobileNumber  : Gen<PhoneNumber> =
    bdPhoneNumber

let genPhoneNumber : Gen<PhoneNumber> =
    gen {
        match! Arb.generate<PhoneNumberTypes> with
        | BdMobileNumber ->
            // BD Mobile number
            return! bdPhoneNumber

        | BdLandlineNumber ->
            // BD Mobile number
            let! operatorDigit = Gen.elements [ '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9' ]

            let! otherDigits =
                genDigits
                |> Gen.arrayOfLength 6
                |> Gen.map String

            return
                sprintf "+8802%c%s" operatorDigit otherDigits
                |> PhoneNumber.tryOfString
                |> Result.toOption
                |> Option.get

        | UsPhoneNumber ->
            let! digits =
                genDigits
                |> Gen.arrayOfLength 10
                |> Gen.map String

            return
                sprintf "+1%s" digits
                |> PhoneNumber.tryOfString
                |> Result.toOption
                |> Option.get

        | OtherCountryPhoneNumber ->
            let! numDigits = Gen.choose(6, 10)
            let! digits =
                genDigits
                |> Gen.arrayOfLength numDigits
                |> Gen.map String

            let! startingDigit =
                genDigits
                |> Gen.where (fun ch -> ch <> '0' && ch <> '1' && ch <> '2' && ch <> '8')

            return
                sprintf "+%c%s" startingDigit digits
                |> PhoneNumber.tryOfString
                |> Result.toOption
                |> Option.get
    }
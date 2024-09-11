[<AutoOpen>]
module LibLifeCycleTest.GenCommon

open System
open FsCheck
open LibLifeCycleTypes.File

let genBool = Gen.elements [true; false]

// TODO, replace with the ne PhoneNumber type
let genBdPhoneNumber =
    Gen.choose(0, 9)
    |> Gen.arrayOfLength 8
    |> Gen.map (
        fun digits ->
            digits
            |> Seq.map (sprintf "%d")
            |> fun digitStr -> String.Join("", digitStr)
            |> sprintf "+88017%s"
    )

let genDhakaLatLngTuple = gen {
    let! lat =
        Gen.choose(237788430, 237966710)
        |> Gen.map (fun i -> (float i) / (pown 10.0 7))

    let! lng =
        Gen.choose(903938590, 904160250)
        |> Gen.map (fun i -> (float i) / (pown 10.0 7))

    return lat, lng
}

let genDhakaLatLng = gen {
    let! lat, lng = genDhakaLatLngTuple
    match! genBool with
    | true  -> return Some (lat, lng)
    | false -> return None
}

let genImageUrl =
    Gen.choose(1, 46)
    |> Gen.map (sprintf "https://eggtestdata.blob.core.windows.net/catalogimages/%d.jpg")

let genGuid =
    Arb.generate<Guid>

let genArb<'T> =
    Arb.generate<'T>

let genImageFile = gen {
    let! fileData =
        Gen.choose(1,127)
        |> Gen.map
            (fun length ->
                length
                |> Array.zeroCreate
                |> FileData.Bytes
            )

    let mimeType = (MimeType.ofString "image/jpeg").Value

    let file = {
        Data = fileData
        MimeType = mimeType
    }

    return file
}

let genPdfFile = gen {
    let! fileData =
        Gen.choose(1,127)
        |> Gen.map
            (fun length ->
                length
                |> Array.zeroCreate
                |> FileData.Bytes
            )

    let mimeType = (MimeType.ofString "application/pdf").Value

    let file = {
        Data = fileData
        MimeType = mimeType
    }

    return file
}

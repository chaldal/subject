module LibCodecValidation.Tests

open FsUnit.Xunit
open Xunit

open CodecLib
open EvolutionCheckerLib

let shouldNotEvolve = function
    | Ok o -> failwithf $"Evolution should fail, but got Ok: %A{o}"
    | Error _ -> ()

let shouldEvolve = function
    | Ok _ -> ()
    | Error e -> failwithf $"Evolution should pass, but got error: %A{e}"

type TypeWithOptionalFieldOptWtih = {
    MaybeInt: Option<int>
}
with
    static member get_Codec () =
        codec {
            let! maybeInt = optWith Codecs.int "int" (fun x -> x.MaybeInt)
            return { MaybeInt = maybeInt }
        }
        |> ofObjCodec

type TypeWithOptionalFieldReqWith = {
    MaybeInt: Option<int>
}
with
    static member get_Codec () =
        codec {
            let! maybeInt = reqWith (Codecs.option Codecs.int) "int" (fun x -> Some x.MaybeInt)
            return { MaybeInt = maybeInt }
        }
        |> ofObjCodec

[<Fact>]
let ``Cannot replace optWith by reqWith`` () =
    let jsonNode1 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, TypeWithOptionalFieldOptWtih>
    let jsonNode2 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, TypeWithOptionalFieldReqWith>
    checkEvolutionCorrectness 0u jsonNode1 jsonNode2
    |> shouldNotEvolve

[<Fact>]
let ``Can replace reqWith by optWith`` () =
    let jsonNode1 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, TypeWithOptionalFieldReqWith>
    let jsonNode2 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, TypeWithOptionalFieldOptWtih>
    checkEvolutionCorrectness 0u jsonNode1 jsonNode2
    |> shouldEvolve

type UnionCaseType1 =
    | Case1 of int
    | Case2 of string
with
    static member get_Codec () =
        function
        | Case1 _ ->
            codec {
                let! payload = reqWith Codecs.int "Case1" (function | Case1 x -> Some x | _ -> None)
                return Case1 payload
            }
        | Case2 _ ->
            codec {
                let! payload = reqWith Codecs.string "Case2" (function | Case2 x -> Some x | _ -> None)
                return Case2 payload
            }
        |> mergeUnionCases
        |> ofObjCodec

type UnionCaseType1CaseRemoved =
    | Case2 of string
with
    static member get_CodecEvolved () =
        function
        | Case2 _ ->
            codec {
                let! payload = reqWith Codecs.string "Case2" (function | Case2 x -> Some x)
                return Case2 payload
            }
            |> withDecoders [
                decoder {
                    let! payload = reqDecodeWithCodec Codecs.int "Case1"
                    return Case2 (string payload)
                }
            ]
        |> mergeUnionCases
        |> ofObjCodec

    static member get_CodecWrong () =
        function
        | Case2 _ ->
            codec {
                let! payload = reqWith Codecs.string "Case2" (function | Case2 x -> Some x)
                return Case2 payload
            }
        |> mergeUnionCases
        |> ofObjCodec

[<Fact>]
let ``Decoder must be added for removed case`` () =
    let jsonNode1 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, UnionCaseType1>
    let jsonNodeEvolved = HelperMethods.getDecodersAsJsonNode (UnionCaseType1CaseRemoved.get_CodecEvolved<Encoding> ())
    let jsonNodeWrong = HelperMethods.getDecodersAsJsonNode (UnionCaseType1CaseRemoved.get_CodecWrong<Encoding> ())
    checkEvolutionCorrectness 0u jsonNode1 jsonNodeEvolved
    |> shouldEvolve
    checkEvolutionCorrectness 0u jsonNode1 jsonNodeWrong
    |> shouldNotEvolve

type LifeAction1 =
    inherit IInterfaceCodec<LifeAction1>

type LifeAction2 =
    inherit IInterfaceCodec<LifeAction2>

type Act1 = {
    Number: int
}
with
    static member get_ObjCodec () =
        codec {
            let! number = reqWith Codecs.int "Number" (fun x -> Some x.Number)
            return { Number = number }
        }

    static member get_Codec () =
        Act1.get_ObjCodec ()
        |> ofObjCodec

    static member TypeLabel () = "Act"

    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction1, Act1> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| Act1.get_ObjCodec ())

type Act2 = {
    Desc: string
}
with
    static member get_ObjCodec () =
        codec {
            let! desc = reqWith Codecs.string "Desc" (fun x -> Some x.Desc)
            return { Desc = desc }
        }

    static member get_ObjDecoder () =
        decoder {
            let! number = reqDecodeWithCodec Codecs.int "Number"
            return { Desc = string number }
        }

    static member get_Codec () =
        Act2.get_ObjCodec ()
        |> withDecoders [
            Act2.get_ObjDecoder ()
        ]
        |> ofObjCodec

    static member TypeLabel () = "Act"

    static member Init (typeLabel: string, _typeParams: _) = initializeInterfaceImplementation<LifeAction2, Act2> (fun () -> attachCodecTypeLabel ("__type_" + typeLabel) <| Act2.get_ObjCodec ())

[<Fact>]
let ``Interface codec should include decoder`` () =
    let jsonNode1 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, Act1>
    let jsonNode2 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, Act2>
    checkEvolutionCorrectness 0u jsonNode1 jsonNode2
    |> shouldEvolve

    Act1.Init(Act1.TypeLabel(), [||])
    Act2.Init(Act2.TypeLabel(), [||])
    let jsonNode3 = HelperMethods.getJsonNodeFromCodecCollectionSubtypes CodecCollection<Encoding, LifeAction1>.GetSubtypes
    let jsonNode4 = HelperMethods.getJsonNodeFromCodecCollectionSubtypes CodecCollection<Encoding, LifeAction2>.GetSubtypes
    checkEvolutionCorrectness 0u jsonNode3 jsonNode4
    |> shouldNotEvolve

[<RequireQualifiedAccess>]
type UnionCaseType2 =
| Case1 of string
| Case2 of int

type UnionCaseType2 with
    static member private get_ObjCodec_AllCases () =
        function
        | Case1 _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Case1 _ -> Some 0 | _ -> None)
                and! payload = reqWith Codecs.string "NotRunning" (function Case1 x -> Some x | _ -> None)
                return Case1 payload
            }
        | Case2 _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Case2 _ -> Some 0 | _ -> None)
                and! payload = reqWith (Codecs.int) "WaitingForResponse" (function Case2 x -> Some x | _ -> None)
                return Case2 payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (UnionCaseType2.get_ObjCodec_AllCases ())


type Record2 = {
    Name: string
}
with
    static member get_Codec () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)
            and! name = reqWith Codecs.string "Name" (fun x -> Some x.Name)
            return
                {
                    Name = name
                }
        }
        |> ofObjCodec

type Record2Evolved = {
    Name: string
    Case: UnionCaseType2
}
with
    static member get_Codec () =
        codec {
            let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 1)
            and! name = reqWith Codecs.string "Name" (fun x -> Some x.Name)
            and! maybeCase = optWith codecFor<_, UnionCaseType2> "Case" (fun x -> Some x.Case)
            return
                {
                    Name = name
                    Case =
                        maybeCase
                        |> Option.defaultValue (UnionCaseType2.Case1 "default")
                }
        }
        |> ofObjCodec

[<Fact>]
let ``Record evolved with optWith of union cases`` () =
    let jsonNode1 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, Record2>
    let jsonNode2 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, Record2Evolved>
    checkEvolutionCorrectness 0u jsonNode1 jsonNode2
    |> shouldEvolve

[<RequireQualifiedAccess>]
type UnionCaseType3 =
| Case1 of int

type UnionCaseType3 with
    static member private get_ObjCodec_AllCases () =
        function
        | Case1 _ ->
            codec {
                let! _version = reqWith Codecs.int "__v1" (function Case1 _ -> Some 0)
                and! payload = reqWith Codecs.int "Case1" (function Case1 x -> Some x)
                return Case1 payload
            }
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (UnionCaseType3.get_ObjCodec_AllCases ())

[<RequireQualifiedAccess>]
type UnionCaseType3Evolved =
| Case1 of int * string

type UnionCaseType3Evolved with
    static member private get_ObjCodec_AllCases () =
        function
        | Case1 _ ->
            doubleEncode
                (
                    codec {
                        let! _version = reqWith Codecs.int "__v2" (function Case1 _ -> Some 0)
                        and! payload = reqWith (Codecs.tuple2 Codecs.int Codecs.string) "Case1" (function Case1 (x1, x2) -> Some (x1, x2))
                        return Case1 payload
                    }
                )
                (
                    codec {
                        let! _version = reqWith Codecs.int "__v1" (function Case1 _ -> Some 0)
                        and! payload = reqWith Codecs.int "Case1" (function Case1 (x, _) -> Some x)
                        return Case1 (payload, "default")
                    }
                )
        |> mergeUnionCases
    static member get_Codec () = ofObjCodec (UnionCaseType3Evolved.get_ObjCodec_AllCases ())

[<Fact>]
let ``Double encoding is backward compatible`` () : unit =
    let jsonNode1 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, UnionCaseType3>
    let jsonNodeEvolved = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, UnionCaseType3Evolved>
    checkEvolutionCorrectness 0u jsonNode1 jsonNodeEvolved
    |> shouldEvolve

[<Fact(Skip = "LibCodeValidation can't assert forward compatibility yet")>]
let ``Double encoding is forward compatible`` () : unit =
    let jsonNode1 = HelperMethods.getDecodersAsJsonNode codecFor<Encoding, UnionCaseType3>
    let jsonNodeEvolved = codecFor<Encoding, UnionCaseType3Evolved>.Encoder
    checkEvolutionCorrectness 0u jsonNodeEvolved jsonNode1
    |> shouldEvolve

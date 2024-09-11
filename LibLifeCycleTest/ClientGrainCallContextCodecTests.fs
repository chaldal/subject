module LibLifeCycleCore.``ClientGrainCallContext Codec Tests``

open CodecLib
open FsUnit.Xunit
open Xunit


[<Fact>]
let ``can roundtrip ClientGrainCallContext data`` () =
    let original =
        {
            SessionHandle = SessionHandle.Session ("id", Authenticated ("james.bond", System.DateTimeOffset.UtcNow))
            CallOrigin = CallOrigin.Internal
        }

    let json = toJsonText original
    let parseResult: Result<ClientGrainCallContext, _> = ofJsonText json

    match parseResult with
    | Ok result ->
        result
        |> should equal original
    | Error _ ->
        ``💣``

[<Fact>]
let ``can deserialize ClientGrainCallContext V0,0 data`` () =
    let json = """{"SessionId":"id"}"""
    let expected =
        {
            SessionHandle = SessionHandle.Session ("id", Anonymous)
            CallOrigin = CallOrigin.Internal
        }

    let parseResult: Result<ClientGrainCallContext, _> = ofJsonText json

    match parseResult with
    | Ok result ->
        result
        |> should equal expected
    | Error _ ->
        ``💣``

[<Fact>]
let ``can deserialize ClientGrainCallContext V1,0 data`` () =
    let json = """{"SessionHandle":{"SessionId":"id"},"CallOrigin":{"External":{"RemoteAddress":"addr","Headers":[["first",["first value"]]],"__v1":0},"__v1":0},"__v1":0}"""
    let expected =
        {
            SessionHandle = SessionHandle.Session ("id", Anonymous)
            CallOrigin =
                CallOrigin.External
                    {
                        RemoteAddress = "addr"
                        Headers =
                            [
                                (
                                    "first",
                                    [
                                        "first value"
                                    ]
                                    |> Set.ofList
                                )
                            ]
                            |> Map.ofList
                    }
        }

    let parseResult: Result<ClientGrainCallContext, _> = ofJsonText json

    match parseResult with
    | Ok result ->
        result
        |> should equal expected
    | Error _ ->
        ``💣``

module LibClient.Services.LocalStorageService

open Fable.Core
open Fable.Core.JsInterop

let keyFromParts (parts: List<string>) : string =
    parts |> String.concat "-"

type LocalStorageService(appPrefix: string) =
    inherit LibClient.Services.StorageService.StorageService()

    let extendKey (useSiteKey: string) : string = appPrefix + "-" + useSiteKey

    override _.Get<'T> (key: string) (decode: string -> Result<'T, string>) : Async<Option<'T>> = async {
        let! maybeRawItem = ReactXP.Helpers.ReactXPRaw?Storage?getItem(extendKey key) |> Async.AwaitPromise

        match Option.ofObj maybeRawItem with
        | None         -> return None
        | Some rawItem ->
            match decode rawItem with
            | Ok decodedItem -> return Some decodedItem
            | Error error    ->
                LibClient.Logging.Log.Error ("Failed to decode local storage item. {key}, {error}", key, error)
                return None
    }

    override _.Put<'T> (key: string) (value: 'T) (encode: 'T -> string) : Async<unit> = async {
        let encodedValue = encode value
        return! ReactXP.Helpers.ReactXPRaw?Storage?setItem(extendKey key, encodedValue) |> Async.AwaitPromise
    }

    override _.Remove (key: string) : Async<unit> = async {
        return! ReactXP.Helpers.ReactXPRaw?Storage?removeItem(extendKey key) |> Async.AwaitPromise
    }
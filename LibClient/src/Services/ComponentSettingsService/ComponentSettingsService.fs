module LibClient.Services.ComponentSettingsService

open LibClient.Json

type ComponentSettingsService(storageService: LibClient.Services.StorageService.StorageService) =
    let extendKey (useSiteKey: string) : string = "ComponentSetting-" + useSiteKey

    member inline this.PotentiallyTypeUnsafeGet<'T> (pstoreKey: string) (defaultValue: 'T) : Async<'T> =
        this.GetWithDecoderWithDefault
            pstoreKey
            defaultValue
            Json.FromString<'T>

    member private _.GetWithDecoder<'T> (pstoreKey: string) (decode: string -> Result<'T, string>) : Async<Option<'T>> = async {
        let! maybeValue = storageService.Get (extendKey pstoreKey) decode
        return maybeValue
    }

    member (* want private, need public because inline *) this.GetWithDecoderWithDefault<'T> (pstoreKey: string) (defaultValue: 'T) (decode: string -> Result<'T, string>) : Async<'T> = async {
        let! maybeValue = this.GetWithDecoder pstoreKey decode
        return maybeValue |> Option.getOrElse defaultValue
    }

    member inline this.PotentiallyTypeUnsafeUpdate<'T> (pstoreKey: string) (update: Option<'T> -> Option<'T>) : Async<unit> =
        this.UpdateWithCoders
            pstoreKey
            update
            Json.FromString<'T>
            (fun t -> Json.ToString<'T> t) // TODO need to make this common, lest everybody has to deal with the stupid zero

    member (* want private, need public because inline *) this.UpdateWithCoders<'T> (pstoreKey: string) (update: Option<'T> -> Option<'T>) (decode: string -> Result<'T, string>) (encode: 'T -> string) : Async<unit> = async {
        let! maybeCurrentValue = this.GetWithDecoder pstoreKey decode
        match update maybeCurrentValue with
        | None              -> return Noop
        | Some updatedValue -> return! storageService.Put (extendKey pstoreKey) updatedValue encode
    }

    // NOTE the reason we need this accessor is because using a straight-up Update function
    // allows you to pass it _any_ record modifying function, and it will compile, i.e. there
    // is no way to enforce that the update function is called with the correct Settings type.
    // So the idea is that if we are forced to build the accessor once, and are forced to explicitly
    // provide the Settings type at construction time, we are much less likely to make an error
    member inline this.BuildMeTypeSafeAccessor<'T> (pstoreKey: string) = {|
        Get    = fun (defaultValue: 'T) -> this.PotentiallyTypeUnsafeGet pstoreKey defaultValue
        Update = fun (update: Option<'T> -> Option<'T>) -> this.PotentiallyTypeUnsafeUpdate pstoreKey update
    |}

[<AutoOpen>]
module LibClient.AsyncDataModule

type RequestFailure = 
| ClientError of statusCode: int * response: string
| ServerError of statusCode: int * response: string
| Unknown     of statusCode: int * response: string
    static member ofStatusCode (statusCode: int, response: string) : RequestFailure =
        match statusCode with
            | code when (400 <= code && code <= 499)  -> RequestFailure.ClientError (code, response)
            | code when (500 <= code && code <= 599)  -> RequestFailure.ServerError (code, response)
            | _ -> RequestFailure.Unknown (statusCode, response)

type AsyncDataFailure =
| NetworkFailure
| RequestFailure      of RequestFailure
| UserReadableFailure of UserReadableMessage: string
| UnknownFailure      of DeveloperReadableMessage: string
with
    member this.DisplayReason =
        match this with
        | NetworkFailure -> 
            "Network failure: Unable to reach the server."
        | RequestFailure requestFailure ->
            match requestFailure with
            | RequestFailure.ClientError (statusCode, response) ->
                $"Client error ({statusCode}): {response}"
            | RequestFailure.ServerError (statusCode, response) ->
                $"Server error ({statusCode}): {response}"
            | RequestFailure.Unknown (statusCode, response) ->
                $"Unknown error ({statusCode}): {response}"
        | UserReadableFailure message ->
            $"User-readable failure: {message}"
        | UnknownFailure developerMessage ->
            $"Unknown failure: {developerMessage}"

exception AsyncDataException of AsyncDataFailure

type AsyncData<'T> =
| Uninitialized
| WillStartFetchingSoonHack
| Fetching  of OldValue: Option<'T>
| Failed    of AsyncDataFailure
| Available of 'T
| AccessDenied
| Unavailable // e.g. a deleted entity

module AsyncData =
    let toOption<'T> (value: AsyncData<'T>) : Option<'T> =
        match value with
        | Available t -> Some t
        | _           -> None

    let makeFetching<'T> (currentValue: AsyncData<'T>) : AsyncData<'T> =
        match currentValue with
        | Available value        -> Fetching (Some value)
        | Fetching maybeOldValue -> Fetching maybeOldValue
        | _                      -> Fetching None

    let map<'T, 'U> (mapper: 'T -> 'U) (source: AsyncData<'T>) : AsyncData<'U> =
        match source with
        | Uninitialized             -> Uninitialized
        | WillStartFetchingSoonHack -> WillStartFetchingSoonHack
        | Fetching oldValue         -> Fetching (oldValue |> Option.map mapper)
        | Failed error              -> Failed error
        | Available value           -> Available (mapper value)
        | Unavailable               -> Unavailable
        | AccessDenied              -> AccessDenied

    let treatFetchingSomeAsAvailable (source: AsyncData<'T>) : AsyncData<'T> =
        match source with
        | Fetching (Some oldValue) -> Available oldValue
        | _ -> source

    let mapIfAvailable<'T, 'U> (mapper: 'T -> Option<'U>) (source: AsyncData<'T>) : AsyncData<'U> =
        match source with
        | Uninitialized             -> Uninitialized
        | WillStartFetchingSoonHack -> WillStartFetchingSoonHack
        | Fetching oldValue         -> Fetching (oldValue |> Option.flatMap mapper)
        | Failed error              -> Failed error
        | Unavailable               -> Unavailable
        | AccessDenied              -> AccessDenied
        | Available value ->
            match mapper value with
            | None             -> Unavailable
            | Some mappedValue -> Available mappedValue

    let tryMapIfAvailable<'T, 'U> (mapper: 'T -> Option<'U>) (source: AsyncData<'T>) : Option<AsyncData<'U>> =
        match source with
        | Uninitialized             -> Uninitialized                                |> Some
        | WillStartFetchingSoonHack -> WillStartFetchingSoonHack                    |> Some
        | Fetching oldValue         -> Fetching (oldValue |> Option.flatMap mapper) |> Some
        | Failed error              -> Failed error                                 |> Some
        | Unavailable               -> Unavailable                                  |> Some
        | AccessDenied              -> AccessDenied                                 |> Some
        | Available value ->
            match mapper value with
            | None             -> None
            | Some mappedValue -> Available mappedValue |> Some

    let flatten<'T> (source: AsyncData<AsyncData<'T>>) : AsyncData<'T> =
        match source with
        | Uninitialized                            -> Uninitialized
        | WillStartFetchingSoonHack                -> WillStartFetchingSoonHack
        | Failed error                             -> Failed error
        | Unavailable                              -> Unavailable
        | AccessDenied                             -> AccessDenied
        | Available value                          -> value
        | Fetching (Some (Fetching maybeOldValue)) -> Fetching maybeOldValue
        | Fetching (Some (Available oldValue))     -> Fetching (Some oldValue)
        | Fetching _                               -> Fetching None


    let spread<'T1, 'T2> (asyncT1: AsyncData<'T1>) (asyncT2: AsyncData<'T2>) : AsyncData<'T1 * 'T2> =
        match (asyncT1, asyncT2) with
        | (Uninitialized, _) | (_, Uninitialized) -> Uninitialized

        | (Fetching None, _)                           -> Fetching None
        | (_, Fetching None)                           -> Fetching None
        | (WillStartFetchingSoonHack, _)               -> WillStartFetchingSoonHack
        | (_, WillStartFetchingSoonHack)               -> WillStartFetchingSoonHack
        | (Fetching (Some old1), Fetching (Some old2)) -> Fetching (Some (old1, old2))
        | (Fetching (Some old1), Available value2)     -> Fetching (Some (old1, value2))
        | (Available value1, Fetching (Some old2))     -> Fetching (Some (value1, old2))

        | (Failed error, _)  | (_, Failed error)  -> Failed error
        | (Unavailable, _)   | (_, Unavailable)   -> Unavailable
        | (AccessDenied, _)  | (_, AccessDenied)  -> AccessDenied
        | (Available t1, Available t2)            -> Available (t1, t2)

    let spread3<'T1, 'T2, 'T3> (asyncT1: AsyncData<'T1>) (asyncT2: AsyncData<'T2>) (asyncT3: AsyncData<'T3>) : AsyncData<'T1 * 'T2 * 'T3> =
        match spread asyncT1 (spread asyncT2 asyncT3) with
        | Uninitialized                  -> Uninitialized
        | WillStartFetchingSoonHack      -> WillStartFetchingSoonHack
        | Fetching (Some (v1, (v2, v3))) -> Fetching (Some (v1, v2, v3))
        | Fetching None                  -> Fetching None
        | Failed error                   -> Failed error
        | Unavailable                    -> Unavailable
        | AccessDenied                   -> AccessDenied
        | Available (v1, (v2, v3))       -> Available (v1, v2, v3)

    let spread4<'T1, 'T2, 'T3, 'T4> (asyncT1: AsyncData<'T1>) (asyncT2: AsyncData<'T2>) (asyncT3: AsyncData<'T3>) (asyncT4: AsyncData<'T4>) : AsyncData<'T1 * 'T2 * 'T3 * 'T4> =
        match spread asyncT1 (spread asyncT2 (spread asyncT3 asyncT4)) with
        | Uninitialized                        -> Uninitialized
        | WillStartFetchingSoonHack            -> WillStartFetchingSoonHack
        | Fetching (Some (v1, (v2, (v3, v4)))) -> Fetching (Some (v1, v2, v3, v4))
        | Fetching None                        -> Fetching None
        | Failed error                         -> Failed error
        | Unavailable                          -> Unavailable
        | AccessDenied                         -> AccessDenied
        | Available (v1, (v2, (v3, v4)))       -> Available (v1, v2, v3, v4)

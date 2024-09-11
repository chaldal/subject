// Provides a friendlier API around Fable.Browser.IndexedDB, which is a very raw surfacing of the underlying JS API.
// Note that some functionality may not be surfaced until there is justification to do so.

module rec LibClient.Services.IndexedDBService

// IndexedDB is not relevant on native.
#if EGGSHELL_PLATFORM_IS_WEB

open System
open LibClient
open Browser
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop

[<AutoOpen>]
module private Helpers =
    [<Emit("'indexedDB' in window")>]
    let nativeIsSupportedBrowserCheck() = jsNative

    let inline tryCoerce<'T> (value: obj) : Option<'T> =
        match value with
        | :? 'T as coerced -> Some coerced
        | _ -> None

    // Fable is incapable of type testing a generic type, so in those situations (as well as when we expect the underlying IndexedDB API
    // to provide a specific type) we force coercion.
    let forceCoerce<'T> (value: obj) : 'T =
        value :?> 'T

    // Wrap a synchronous operation with error handling.
    let inline wrapSync
            (getValue: unit -> 'Value)
            (tryMapDOMError: string -> string -> Option<'Error>)
            : Result<'Value, 'Error> =
        try
            let value = getValue ()
            Ok value
        with
        | exn ->
            match exn |> tryCoerce<DOMException> with
            | Some domException ->
                match tryMapDOMError domException.name domException.message with
                | Some mappedError -> mappedError |> Error
                | _ -> failwith $"DOM error with name '{domException.name}' is not currently handled"
            | None ->
                reraise ()

    // Wrap an IDBRequest with Async and handle errors appropriately.
    let inline wrapDbRequest
            (createRequest: unit -> IDBRequest)
            (mapSuccess: Option<obj> -> 'Value)
            (tryMapDOMError: string -> string -> Option<'Error>)
            : Async<Result<'Value, 'Error>> =
        Async.FromContinuations
            (fun (return', throw', _cancel) ->
                let handleError (maybeDOMException: Option<DOMException>) (maybeException: Option<exn>) : unit =
                    let maybeNameAndMessage =
                        maybeDOMException
                        |> Option.map (fun domException -> (domException.name, domException.message))

                    match maybeNameAndMessage, maybeException with
                    | Some (name, message), _ ->
                        match tryMapDOMError name message with
                        | Some mappedError ->
                            mappedError |> Error |> return'
                        | None ->
                            $"DOM error with name '{name}' is not currently handled" |> Exception |> throw'
                    | None, Some exn ->
                        exn |> throw'
                    | None, None ->
                        "Undefined error" |> Exception |> throw'

                try
                    let request = createRequest ()

                    request.onsuccess <- (fun _ -> request.result |> mapSuccess |> Ok |> return')

                    // Some errors are reported via onerror callback, and some are raised as exceptions when creating the request. Regardless,
                    // we use the same logic to handle the error.
                    request.onerror <- (fun _ -> handleError (Some request.error) None)
                with
                | exn ->
                    match exn |> tryCoerce<DOMException> with
                    | Some domException ->
                        handleError (Some domException) (Some exn)
                    | None ->
                        handleError None (Some exn)
            )

[<RequireQualifiedAccess>]
type AdvanceError =
| TransactionInactive of Message: string
| InvalidState of Message: string

[<RequireQualifiedAccess>]
type ContinueError =
| TransactionInactive of Message: string
| InvalidKey of Message: string
| InvalidState of Message: string

[<RequireQualifiedAccess>]
type ContinueWithPrimaryKeyError =
| TransactionInactive of Message: string
| InvalidKey of Message: string
| InvalidState of Message: string
| InvalidAccess of Message: string

[<RequireQualifiedAccess>]
type UpdateError =
| TransactionInactive of Message: string
| ReadOnly of Message: string
| InvalidState of Message: string
| InvalidKey of Message: string
| DataCouldNotBeCloned of Message: string

[<RequireQualifiedAccess>]
type AddError =
| TransactionInactive of Message: string
| TransactionIsReadOnly of Message: string
| ConstraintError of Message: string

[<RequireQualifiedAccess>]
type GetError =
| TransactionInactive of Message: string
| InvalidKeyRange of Message: string
| InvalidState of Message: string

[<RequireQualifiedAccess>]
type GetAllError =
| TransactionInactive of Message: string
| InvalidKeyRange of Message: string
| InvalidState of Message: string

[<RequireQualifiedAccess>]
type GetAllKeysError =
| TransactionInactive of Message: string
| InvalidKeyRange of Message: string
| InvalidState of Message: string

[<RequireQualifiedAccess>]
type PutError =
| TransactionIsReadOnly of Message: string
| TransactionInactive of Message: string
| InvalidKey of Message: string
| InvalidState of Message: string
| DataCouldNotBeCloned of Message: string

[<RequireQualifiedAccess>]
type GetKeyError =
| TransactionInactive of Message: string
| InvalidState of Message: string
| InvalidKeyRange of Message: string

[<RequireQualifiedAccess>]
type DeleteError =
| TransactionIsReadOnly of Message: string
| TransactionInactive of Message: string
| InvalidState of Message: string
| InvalidKeyRange of Message: string

[<RequireQualifiedAccess>]
type CountError =
| InvalidState of Message: string
| TransactionInactive of Message: string
| InvalidKeyRange of Message: string

[<RequireQualifiedAccess>]
type ClearError =
| TransactionIsReadOnly of Message: string
| TransactionInactive of Message: string

[<RequireQualifiedAccess>]
type IterateCursorError =
| InvalidState of Message: string
| TransactionInactive of Message: string
| InvalidKey of Message: string
| InvalidDirection of Message: string

[<RequireQualifiedAccess>]
type GetIndexError =
| InvalidState of Message: string
| NotFound of Message: string

[<RequireQualifiedAccess>]
type CreateObjectStoreError =
| InvalidTransactionType of Message: string
| TransactionInactive of Message: string
| AlreadyExists of Message: string
| InvalidKeyPath of Message: string

[<RequireQualifiedAccess>]
type GetObjectStoreError =
| InvalidState of Message: string
| NotFound of Message: string

[<RequireQualifiedAccess>]
type BeginTransactionError =
| InvalidState of Message: string
| NotFound of Message: string
| InvalidAccess of Message: string

[<RequireQualifiedAccess>]
type GetDatabasesError =
| SecurityViolation of Message: string

type Version = uint64

// NOTE: no binary blob/array support until we need it
type Key =
| String of string
| Date of DateTimeOffset
| Float of float
with
    member internal this.ToRaw(): obj =
        match this with
        | String value -> value
        | Date value -> value
        | Float value -> value

    static member internal FromRaw(raw: obj): Key =
        match raw with
        | :? string as value -> String value
        | :? DateTimeOffset as value -> Date value
        | :? float as value -> Float value
        | _ -> failwith $"Unsupported key: {raw}"

[<RequireQualifiedAccess>]
type KeyPath =
| String of string
| Array of array<string>
with
    member internal this.ToRaw(): U2<string, ResizeArray<string>> =
        match this with
        | String value -> value |> U2.Case1
        | Array value -> value |> ResizeArray |> U2.Case2

    static member internal FromRaw(raw: U2<string, ResizeArray<string>>): KeyPath =
        match raw with
        | U2.Case1 value -> value |> String
        | U2.Case2 value -> value |> Array.ofSeq |> Array

type AutoIncrement = bool

[<RequireQualifiedAccess>]
type KeyRange =
| Equal of Key
| LessThan of Key
| LessThanOrEqual of Key
| GreaterThan of Key
| GreaterThanOrEqual of Key
| BetweenExclusive of Key * Key
| BetweenInclusive of Key * Key
| BetweenExclusiveOfLowerInclusiveOfUpper of Key * Key
| BetweenInclusiveOfLowerExclusiveOfUpper of Key * Key
with
    member internal this.ToRaw(): IDBKeyRange =
        match this with
        | Equal key -> IDBKeyRange.only(key.ToRaw())
        | LessThan key -> IDBKeyRange.upperBound(key.ToRaw(), true)
        | LessThanOrEqual key -> IDBKeyRange.upperBound(key.ToRaw(), false)
        | GreaterThan key -> IDBKeyRange.lowerBound(key.ToRaw(), true)
        | GreaterThanOrEqual key -> IDBKeyRange.lowerBound(key.ToRaw(), false)
        | BetweenExclusive (lower, upper) -> IDBKeyRange.bound(lower.ToRaw(), upper.ToRaw(), true, true)
        | BetweenInclusive (lower, upper) -> IDBKeyRange.bound(lower.ToRaw(), upper.ToRaw(), false, false)
        | BetweenExclusiveOfLowerInclusiveOfUpper (lower, upper) -> IDBKeyRange.bound(lower.ToRaw(), upper.ToRaw(), true, false)
        | BetweenInclusiveOfLowerExclusiveOfUpper (lower, upper) -> IDBKeyRange.bound(lower.ToRaw(), upper.ToRaw(), false, true)

type CursorDirection =
| Next
| Previous
| NextUnique
| PreviousUnique
with
    member internal this.ToRaw(): IDBCursorDirection =
        match this with
        | Next -> IDBCursorDirection.Next
        | Previous -> IDBCursorDirection.Prev
        | NextUnique -> IDBCursorDirection.Nextunique
        | PreviousUnique -> IDBCursorDirection.Prevunique

    static member internal FromRaw(raw: IDBCursorDirection): CursorDirection =
        match raw with
        | IDBCursorDirection.Next -> Next
        | IDBCursorDirection.Prev -> Previous
        | IDBCursorDirection.Nextunique -> NextUnique
        | IDBCursorDirection.Prevunique ->PreviousUnique

type ICursor<'T> =
    abstract member Direction: CursorDirection
    abstract member Key: Option<Key>
    abstract member PrimaryKey: Option<Key>

    abstract member Advance: uint -> Result<unit, AdvanceError>
    abstract member Continue: Option<Key> -> Result<unit, ContinueError>
    abstract member ContinueWithPrimaryKey: Key -> Key -> Result<unit, ContinueWithPrimaryKeyError>

type ICursorWithValue<'T> =
    inherit ICursor<'T>

    abstract member Value: 'T

    // Even though these are defined in IDBCursor, they are documented to only work in IDCursorWithValue :/
    // See https://developer.mozilla.org/en-US/docs/Web/API/IDBCursor/delete
    abstract member Delete: unit -> Async<Result<unit, DeleteError>>
    abstract member Update: 'T -> Async<Result<Key, UpdateError>>

type IndexOptions = {
    IsUnique: bool
    IsMultiEntry: bool
}
with
    static member Default: IndexOptions =
        {
            IsUnique = false
            IsMultiEntry = false
        }

    static member FromRawIndex(rawIndex: IDBIndex): IndexOptions =
        {
            IsUnique = rawIndex.unique
            IsMultiEntry = rawIndex.multiEntry
        }

    member internal this.ToRaw(): IDBCreateIndexOptions =
        let options = createEmpty<IDBCreateIndexOptions>
        options.unique <- this.IsUnique
        options.multiEntry <- this.IsMultiEntry
        options

type IIndex<'T> =
    abstract member Name: string
    abstract member ObjectStore: IObjectStore<'T>
    abstract member KeyPath: KeyPath
    abstract member Options: IndexOptions

    abstract member Get: KeyRange -> Async<Result<Option<'T>, GetError>>
    abstract member GetKey: KeyRange -> Async<Result<Option<Key>, GetKeyError>>
    abstract member GetAll: Option<KeyRange> -> Option<uint> -> Async<Result<seq<'T>, GetAllError>>
    abstract member GetAllKeys: Option<KeyRange> -> Option<uint> -> Async<Result<seq<Key>, GetAllKeysError>>
    abstract member Count: Option<KeyRange> -> Async<Result<uint, CountError>>
    abstract member IterateCursor: (Option<ICursor<'T>> -> unit) -> Option<KeyRange> -> Option<CursorDirection> -> Result<unit, IterateCursorError>
    abstract member IterateCursorWithValue: (Option<ICursorWithValue<'T>> -> unit) -> Option<KeyRange> -> Option<CursorDirection> -> Result<unit, IterateCursorError>

type IObjectStoreBase =
    abstract member Name: string
    abstract member KeyPath: KeyPath
    abstract member AutoIncrement: AutoIncrement
    abstract member IndexNames: seq<string>

type IObjectStoreUpgrader =
    inherit IObjectStoreBase

    abstract member CreateIndex: string -> KeyPath -> IndexOptions -> unit
    abstract member DeleteIndex: string -> unit

type IObjectStore<'T> =
    inherit IObjectStoreBase

    abstract member GetIndex: string -> Result<IIndex<'T>, GetIndexError>

    abstract member Add: 'T -> Option<Key> -> Async<Result<unit, AddError>>
    abstract member Put: 'T -> Option<Key> -> Async<Result<unit, PutError>>
    abstract member Get: KeyRange -> Async<Result<Option<'T>, GetError>>
    abstract member GetKey: KeyRange -> Async<Result<Option<Key>, GetKeyError>>
    abstract member GetAll: Option<KeyRange> -> Option<uint> -> Async<Result<seq<'T>, GetAllError>>
    abstract member GetAllKeys: Option<KeyRange> -> Option<uint> -> Async<Result<seq<Key>, GetAllKeysError>>
    abstract member Delete: KeyRange -> Async<Result<unit, DeleteError>>
    abstract member Count: Option<KeyRange> -> Async<Result<uint, CountError>>
    abstract member Clear: unit -> Async<Result<unit, ClearError>>
    abstract member IterateCursor: (Option<ICursor<'T>> -> unit) -> Option<KeyRange> -> Option<CursorDirection> -> Result<unit, IterateCursorError>
    abstract member IterateCursorWithValue: (Option<ICursorWithValue<'T>> -> unit) -> Option<KeyRange> -> Option<CursorDirection> -> Result<unit, IterateCursorError>

type IDatabaseUpgrader =
    abstract member ObjectStoreNames: seq<string>

    abstract member CreateObjectStore: string -> Option<KeyPath> -> AutoIncrement -> Result<IObjectStoreUpgrader, CreateObjectStoreError>
    abstract member DeleteObjectStore: string -> unit

// NOTE: it might feel like the upgrade handler should be asynchronous, but IndexedDB is designed for it to
// be synchronous, so my attempts to do so were thwarted.
type UpgradeHandler = Version -> Version -> IDatabaseUpgrader -> unit

[<RequireQualifiedAccess>]
type TransactionMode =
| ReadOnly
| ReadWrite
with
    static member internal FromRaw(raw: IDBTransactionMode): TransactionMode =
        match raw with
        | IDBTransactionMode.Readonly -> TransactionMode.ReadOnly
        | IDBTransactionMode.Readwrite -> TransactionMode.ReadWrite
        | IDBTransactionMode.Versionchange ->
            // This shouldn't happen because the only transaction to use this mode is the one started when upgrading the DB,
            // and we simply don't expose transactions in that context.
            failwith "Unexpected transaction mode"

    member internal this.ToRaw(): IDBTransactionMode =
        match this with
        | ReadOnly -> IDBTransactionMode.Readonly
        | ReadWrite -> IDBTransactionMode.Readwrite

[<RequireQualifiedAccess>]
type TransactionDurability =
| Default
| Strict
| Relaxed
with
    static member internal FromRaw(raw: IDBTransactionDuarability): TransactionDurability =
        match raw with
        | IDBTransactionDuarability.Default -> TransactionDurability.Default
        | IDBTransactionDuarability.Strict -> TransactionDurability.Strict
        | IDBTransactionDuarability.Relaxed -> TransactionDurability.Relaxed

    member internal this.ToRaw(): IDBTransactionDuarability =
        match this with
        | Default -> IDBTransactionDuarability.Default
        | Strict -> IDBTransactionDuarability.Strict
        | Relaxed -> IDBTransactionDuarability.Relaxed

[<RequireQualifiedAccess>]
type TransactionOptions = {
    Durability: TransactionDurability
}
with
    static member Default: TransactionOptions =
        {
            Durability = TransactionDurability.Default
        }

    static member FromRaw(raw: IDBTransactionOptions): TransactionOptions =
        {
            Durability = TransactionDurability.FromRaw raw.durability
        }

    member internal this.ToRaw(): IDBTransactionOptions =
        let options = createEmpty<IDBTransactionOptions>
        options.durability <- this.Durability.ToRaw()
        options

type ITransaction =
    abstract member Database: IDatabase
    abstract member Mode: TransactionMode
    abstract member ObjectStoreNames: seq<string>

    abstract member GetObjectStore: string -> Result<IObjectStore<'T>, GetObjectStoreError>

    abstract member Abort: unit -> unit
    abstract member Commit: unit -> unit

type IDatabase =
    abstract member Name: string
    abstract member Version: Version
    abstract member ObjectStoreNames: seq<string>

    abstract member BeginTransaction: #seq<string> -> TransactionMode -> Option<TransactionOptions> -> Result<ITransaction, BeginTransactionError>

[<RequireQualifiedAccess>]
type OpenedOrUpgradedDb =
| Opened of IDatabase
| Upgraded of IDatabase * (* OldVersion *)Version * (* NewVersion *)Version
with
    member this.Db: IDatabase =
        match this with
        | Opened db
        | Upgraded (db, _, _) ->
            db

type DatabaseInfo = {
    Name: string
    Version: Version
}
with
    static member internal FromRaw(raw: DatabasesType) : DatabaseInfo =
        {
            Name = raw.name
            Version = uint64 raw.version
        }

type IIndexedDbService =
    abstract member IsSupported: bool

    abstract member GetDatabases: unit -> Async<Result<seq<DatabaseInfo>, GetDatabasesError>>
    abstract member OpenOrUpgradeDatabase: string -> Option<Version> -> UpgradeHandler -> Async<Result<OpenedOrUpgradedDb, unit>>

let private log = Log.WithCategory("IndexedDbService")

type Cursor<'T> internal(raw: IDBCursor) =
    interface ICursor<'T> with
        member _.Direction: CursorDirection = CursorDirection.FromRaw raw.direction

        member _.Key: Option<Key> = raw.key |> Option.map Key.FromRaw

        member _.PrimaryKey: Option<Key> = raw.primaryKey |> Option.map Key.FromRaw

        member _.Advance (count: uint) : Result<unit, AdvanceError> =
            wrapSync
                (fun () -> raw.advance(int count))
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> AdvanceError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> AdvanceError.InvalidState |> Some
                    | _ -> None
                )

        member _.Continue (maybeKey: Option<Key>) : Result<unit, ContinueError> =
            wrapSync
                (fun () ->
                    raw.``continue``(
                        ?key = (maybeKey |> Option.map (fun k -> k.ToRaw()))
                    )
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> ContinueError.TransactionInactive |> Some
                    | "DataError" -> message |> ContinueError.InvalidKey |> Some
                    | "InvalidStateError" -> message |> ContinueError.InvalidState |> Some
                    | _ -> None
                )

        member _.ContinueWithPrimaryKey (key: Key) (primaryKey: Key) : Result<unit, ContinueWithPrimaryKeyError> =
            wrapSync
                (fun () -> raw.continuePrimaryKey(key.ToRaw(), primaryKey.ToRaw()))
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> ContinueWithPrimaryKeyError.TransactionInactive |> Some
                    | "DataError" -> message |> ContinueWithPrimaryKeyError.InvalidKey |> Some
                    | "InvalidStateError" -> message |> ContinueWithPrimaryKeyError.InvalidState |> Some
                    | "InvalidAccessError" -> message |> ContinueWithPrimaryKeyError.InvalidAccess |> Some
                    | _ -> None
                )

type CursorWithValue<'T> internal(raw: IDBCursorWithValue) =
    inherit Cursor<'T>(raw)
    interface ICursorWithValue<'T> with
        member _.Value: 'T =
            match raw.value with
            | Some v ->
                v |> forceCoerce<'T>
            | None ->
                // The documentation makes it seem like this should never happen, though without being explicit about it.
                // See https://developer.mozilla.org/en-US/docs/Web/API/IDBCursorWithValue/value
                failwith "Cursor has no current value"

        member _.Delete () : Async<Result<unit, DeleteError>> =
            wrapDbRequest
                (fun () -> raw.delete())
                (fun _ -> ())
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> DeleteError.TransactionInactive |> Some
                    | "ReadOnlyError" -> message |> DeleteError.TransactionIsReadOnly |> Some
                    | "InvalidStateError" -> message |> DeleteError.InvalidState |> Some
                    | _ -> None
                )

        member _.Update (value: 'T) : Async<Result<Key, UpdateError>> =
            wrapDbRequest
                (fun () -> raw.update(value))
                (fun r ->
                    match r |> Option.map Key.FromRaw with
                    | Some key -> key
                    | None -> failwith "Invalid key returned during udpate"
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> UpdateError.TransactionInactive |> Some
                    | "ReadOnlyError" -> message |> UpdateError.ReadOnly |> Some
                    | "InvalidStateError" -> message |> UpdateError.InvalidState |> Some
                    | "DataError" -> message |> UpdateError.InvalidKey |> Some
                    | "DataCloneError" -> message |> UpdateError.DataCouldNotBeCloned |> Some
                    | _ -> None
                )

type Index<'T> internal(raw: IDBIndex) =
    let objectStore = lazy(ObjectStore(raw.objectStore))

    interface IIndex<'T> with
        member _.Name: string = raw.name

        member _.ObjectStore: IObjectStore<'T> = objectStore.Value

        member _.KeyPath: KeyPath = KeyPath.FromRaw(raw.keyPath)

        member _.Options: IndexOptions = IndexOptions.FromRawIndex(raw)

        member _.Get (keyRange: KeyRange) : Async<Result<Option<'T>, GetError>> =
            wrapDbRequest
                (fun () -> raw.get (keyRange.ToRaw()))
                (fun r -> r |> Option.map (fun r -> r |> forceCoerce<'T>))
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetError.TransactionInactive |> Some
                    | "DataError" -> message |> GetError.InvalidKeyRange |> Some
                    | "InvalidStateError" -> message |> GetError.InvalidState |> Some
                    | _ -> None
                )

        member _.GetKey (keyRange: KeyRange) : Async<Result<Option<Key>, GetKeyError>> =
            wrapDbRequest
                (fun () -> raw.getKey (keyRange.ToRaw()))
                (fun r -> r |> Option.map Key.FromRaw)
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetKeyError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> GetKeyError.InvalidState |> Some
                    | "DataError" -> message |> GetKeyError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.GetAll (maybeKeyRange: Option<KeyRange>) (maybeCount: Option<uint>) : Async<Result<seq<'T>, GetAllError>> =
            wrapDbRequest
                (fun () ->
                    raw.getAll(
                        ?query = (maybeKeyRange |> Option.map (fun keyRange -> keyRange.ToRaw())),
                        ?count = (maybeCount |> Option.map int)
                    )
                )
                (fun r ->
                    r
                    |> Option.map (fun r -> r |> forceCoerce<seq<obj>> |> Seq.map forceCoerce<'T>)
                    |> Option.defaultValue Seq.empty
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetAllError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> GetAllError.InvalidState |> Some
                    | "DataError" -> message |> GetAllError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.GetAllKeys (maybeKeyRange: Option<KeyRange>) (maybeCount: Option<uint>) : Async<Result<seq<Key>, GetAllKeysError>> =
            wrapDbRequest
                (fun () ->
                    raw.getAllKeys(
                        ?query = (maybeKeyRange |> Option.map (fun keyRange -> keyRange.ToRaw())),
                        ?count = (maybeCount |> Option.map int)
                    )
                )
                (fun r ->
                    r
                    |> Option.map (fun r -> r |> forceCoerce<seq<obj>> |> Seq.map Key.FromRaw)
                    |> Option.defaultValue Seq.empty
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetAllKeysError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> GetAllKeysError.InvalidState |> Some
                    | "DataError" -> message |> GetAllKeysError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.Count (maybeKeyRange: Option<KeyRange>) : Async<Result<uint, CountError>> =
            wrapDbRequest
                (fun () ->
                    raw.count(
                        ?key = (maybeKeyRange |> Option.map (fun keyRange -> keyRange.ToRaw()))
                    )
                )
                (fun r ->
                    match r |> Option.bind tryCoerce<uint> with
                    | Some count -> count
                    | None -> failwith $"Internal failure when resolving count. Result was {r}"
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> CountError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> CountError.InvalidState |> Some
                    | "DataError" -> message |> CountError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.IterateCursor (handler: Option<ICursor<'T>> -> unit) (maybeKeyRange: Option<KeyRange>) (maybeDirection: Option<CursorDirection>) : Result<unit, IterateCursorError> =
            wrapSync
                (fun () ->
                    let request =
                        raw.openKeyCursor(
                            ?range = (maybeKeyRange |> Option.map (fun v -> v.ToRaw())),
                            ?direction = (maybeDirection |> Option.map (fun v -> v.ToRaw()))
                        )

                    request.onsuccess <-
                        (fun _ ->
                            match request.result |> tryCoerce<IDBCursor> with
                            | Some rawCursor ->
                                let cursor: ICursor<'T> = Cursor<'T>(rawCursor)
                                cursor |> Some |> handler
                            | None ->
                                None |> handler
                        )

                    ()
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> IterateCursorError.InvalidState |> Some
                    | "TransactionInactiveError" -> message |> IterateCursorError.TransactionInactive |> Some
                    | "DataError" -> message |> IterateCursorError.InvalidKey |> Some
                    | _ -> None
                )

        member _.IterateCursorWithValue (handler: Option<ICursorWithValue<'T>> -> unit) (maybeKeyRange: Option<KeyRange>) (maybeDirection: Option<CursorDirection>) : Result<unit, IterateCursorError> =
            wrapSync
                (fun () ->
                    let request =
                        raw.openCursor(
                            ?range = (maybeKeyRange |> Option.map (fun v -> v.ToRaw())),
                            ?direction = (maybeDirection |> Option.map (fun v -> v.ToRaw()))
                        )

                    request.onsuccess <-
                        (fun _ ->
                            match request.result |> tryCoerce<IDBCursorWithValue> with
                            | Some rawCursor ->
                                let cursor: ICursorWithValue<'T> = CursorWithValue<'T>(rawCursor)
                                cursor |> Some |> handler
                            | None ->
                                None |> handler
                        )

                    ()
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> IterateCursorError.InvalidState |> Some
                    | "TransactionInactiveError" -> message |> IterateCursorError.TransactionInactive |> Some
                    | "DataError" -> message |> IterateCursorError.InvalidKey |> Some
                    | _ -> None
                )

type ObjectStore<'T> internal(raw: IDBObjectStore) =
    interface IObjectStoreBase with
        member _.Name = raw.name

        member _.KeyPath = KeyPath.FromRaw(raw.keyPath)

        member _.AutoIncrement = raw.autoIncrement

        member _.IndexNames: seq<string> =
            [0..raw.indexNames.length - 1]
            |> Seq.map (fun i -> raw.indexNames.Item i)

    interface IObjectStoreUpgrader with
        member _.CreateIndex (name: string) (keyPath: KeyPath) (options: IndexOptions) : unit =
            // NOTE: underlying JS wrapper has annoying overload by key path type rather than just taking KeyPath value.
            // See https://github.com/fable-compiler/fable-browser/pull/131#issuecomment-1863919500
            raw.createIndex(name, keyPath.ToRaw(), options.ToRaw())
            |> ignore

        member _.DeleteIndex (name: string) : unit =
            raw.deleteIndex(name)
            |> ignore

    interface IObjectStore<'T> with
        member _.GetIndex (name: string) : Result<IIndex<'T>, GetIndexError> =
            wrapSync
                (fun () ->
                    let rawDbIndex = raw.index(name)
                    let index: IIndex<'T> = Index<'T>(rawDbIndex)
                    index
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> GetIndexError.InvalidState |> Some
                    | "NotFoundError" -> message |> GetIndexError.NotFound |> Some
                    | _ -> None
                )

        member _.Add (value: 'T) (maybeKey: Option<Key>) : Async<Result<unit, AddError>> =
            wrapDbRequest
                (fun () -> raw.add(value, ?key = (maybeKey |> Option.map (fun key -> key.ToRaw()))))
                (fun _ -> ())
                (fun name message ->
                    match name with
                    | "ReadOnlyError" -> message |> AddError.TransactionIsReadOnly |> Some
                    | "TransactionInactiveError" -> message |> AddError.TransactionInactive |> Some
                    | "ConstraintError" -> message |> AddError.ConstraintError |> Some
                    | _ -> None
                )

        member _.Put (value: 'T) (maybeKey: Option<Key>) : Async<Result<unit, PutError>> =
            wrapDbRequest
                (fun () -> raw.put(value, ?key = (maybeKey |> Option.map (fun key -> key.ToRaw()))))
                (fun _ -> ())
                (fun name message ->
                    match name with
                    | "ReadOnlyError" -> message |> PutError.TransactionIsReadOnly |> Some
                    | "TransactionInactiveError" -> message |> PutError.TransactionInactive |> Some
                    | "DataError" -> message |> PutError.InvalidKey |> Some
                    | "InvalidStateError" -> message |> PutError.InvalidState |> Some
                    | "DataCloneError" -> message |> PutError.DataCouldNotBeCloned |> Some
                    | _ -> None
                )

        member _.Get (keyRange: KeyRange) : Async<Result<Option<'T>, GetError>> =
            wrapDbRequest
                (fun () -> raw.get (keyRange.ToRaw()))
                (fun r -> r |> Option.map forceCoerce<'T>)
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetError.TransactionInactive |> Some
                    | "DataError" -> message |> GetError.InvalidKeyRange |> Some
                    | "InvalidStateError" -> message |> GetError.InvalidState |> Some
                    | _ -> None
                )

        member _.GetKey (keyRange: KeyRange) : Async<Result<Option<Key>, GetKeyError>> =
            wrapDbRequest
                (fun () -> raw.getKey (keyRange.ToRaw()))
                (fun r -> r |> Option.map Key.FromRaw)
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetKeyError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> GetKeyError.InvalidState |> Some
                    | "DataError" -> message |> GetKeyError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.GetAll (maybeKeyRange: Option<KeyRange>) (maybeCount: Option<uint>) : Async<Result<seq<'T>, GetAllError>> =
            wrapDbRequest
                (fun () ->
                    raw.getAll(
                        ?query = (maybeKeyRange |> Option.map (fun keyRange -> keyRange.ToRaw())),
                        ?count = (maybeCount |> Option.map int)
                    )
                )
                (fun r ->
                    r
                    |> Option.map (fun r -> r |> forceCoerce<seq<obj>> |> Seq.map forceCoerce<'T>)
                    |> Option.defaultValue Seq.empty
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetAllError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> GetAllError.InvalidState |> Some
                    | "DataError" -> message |> GetAllError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.GetAllKeys (maybeKeyRange: Option<KeyRange>) (maybeCount: Option<uint>) : Async<Result<seq<Key>, GetAllKeysError>> =
            wrapDbRequest
                (fun () ->
                    raw.getAllKeys(
                        ?query = (maybeKeyRange |> Option.map (fun keyRange -> keyRange.ToRaw())),
                        ?count = (maybeCount |> Option.map int)
                    )
                )
                (fun r ->
                    r
                    |> Option.map (fun r -> r |> forceCoerce<seq<obj>> |> Seq.map Key.FromRaw)
                    |> Option.defaultValue Seq.empty
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> GetAllKeysError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> GetAllKeysError.InvalidState |> Some
                    | "DataError" -> message |> GetAllKeysError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.Delete (keyRange: KeyRange) : Async<Result<unit, DeleteError>> =
            wrapDbRequest
                (fun () -> raw.delete (keyRange.ToRaw()))
                (fun _ -> ())
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> DeleteError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> DeleteError.InvalidState |> Some
                    | "DataError" -> message |> DeleteError.InvalidKeyRange |> Some
                    | "ReadOnlyError" -> message |> DeleteError.TransactionIsReadOnly |> Some
                    | _ -> None
                )

        member _.Count (maybeKeyRange: Option<KeyRange>) : Async<Result<uint, CountError>> =
            wrapDbRequest
                (fun () ->
                    raw.count(
                        ?query = (maybeKeyRange |> Option.map (fun keyRange -> keyRange.ToRaw()))
                    )
                )
                (fun r ->
                    match r |> Option.bind tryCoerce<uint> with
                    | Some count -> count
                    | None -> failwith $"Internal failure when resolving count. Result was {r}"
                )
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> CountError.TransactionInactive |> Some
                    | "InvalidStateError" -> message |> CountError.InvalidState |> Some
                    | "DataError" -> message |> CountError.InvalidKeyRange |> Some
                    | _ -> None
                )

        member _.Clear () : Async<Result<unit, ClearError>> =
            wrapDbRequest
                (fun () -> raw.clear ())
                (fun _ -> ())
                (fun name message ->
                    match name with
                    | "TransactionInactiveError" -> message |> ClearError.TransactionInactive |> Some
                    | "ReadOnlyError" -> message |> ClearError.TransactionIsReadOnly |> Some
                    | _ -> None
                )

        member _.IterateCursor (handler: Option<ICursor<'T>> -> unit) (maybeKeyRange: Option<KeyRange>) (maybeDirection: Option<CursorDirection>) : Result<unit, IterateCursorError> =
            wrapSync
                (fun () ->
                    let request =
                        raw.openKeyCursor(
                            ?range = (maybeKeyRange |> Option.map (fun v -> v.ToRaw())),
                            ?direction = (maybeDirection |> Option.map (fun v -> v.ToRaw()))
                        )

                    request.onsuccess <-
                        (fun _ ->
                            match request.result |> tryCoerce<IDBCursor> with
                            | Some rawCursor ->
                                let cursor: ICursor<'T> = Cursor<'T>(rawCursor)
                                cursor |> Some |> handler
                            | None ->
                                None |> handler
                        )

                    ()
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> IterateCursorError.InvalidState |> Some
                    | "TransactionInactiveError" -> message |> IterateCursorError.TransactionInactive |> Some
                    | "DataError" -> message |> IterateCursorError.InvalidKey |> Some
                    | _ -> None
                )

        member _.IterateCursorWithValue (handler: Option<ICursorWithValue<'T>> -> unit) (maybeKeyRange: Option<KeyRange>) (maybeDirection: Option<CursorDirection>) : Result<unit, IterateCursorError> =
            wrapSync
                (fun () ->
                    let request =
                        raw.openCursor(
                            ?range = (maybeKeyRange |> Option.map (fun v -> v.ToRaw())),
                            ?direction = (maybeDirection |> Option.map (fun v -> v.ToRaw()))
                        )

                    request.onsuccess <-
                        (fun _ ->
                            match request.result |> tryCoerce<IDBCursorWithValue> with
                            | Some rawCursor ->
                                let cursor: ICursorWithValue<'T> = CursorWithValue<'T>(rawCursor)
                                cursor |> Some |> handler
                            | None ->
                                None |> handler
                        )

                    ()
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> IterateCursorError.InvalidState |> Some
                    | "TransactionInactiveError" -> message |> IterateCursorError.TransactionInactive |> Some
                    | "DataError" -> message |> IterateCursorError.InvalidKey |> Some
                    | _ -> None
                )

type DatabaseUpgrader internal(raw: IDBDatabase) =
    interface IDatabaseUpgrader with
        member _.ObjectStoreNames =
            [0..raw.objectStoreNames.length - 1]
            |> Seq.map (fun i -> raw.objectStoreNames.Item i)

        member _.CreateObjectStore (name: string) (maybeKeyPath: Option<KeyPath>) (autoIncrement: AutoIncrement) : Result<IObjectStoreUpgrader, CreateObjectStoreError> =
            let options = createEmpty<IDBCreateStoreOptions>

            maybeKeyPath
            |> Option.iter (fun keyPath -> options.keyPath <- keyPath.ToRaw() |> Some)
            options.autoIncrement <- Some autoIncrement

            wrapSync
                (fun () ->
                    let rawDbObjectStore = raw.createObjectStore(name, options)
                    let indexedDbObjectStore = ObjectStore(rawDbObjectStore)
                    indexedDbObjectStore
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> CreateObjectStoreError.InvalidTransactionType |> Some
                    | "TransactionInactiveError" -> message |> CreateObjectStoreError.TransactionInactive |> Some
                    | "ConstraintError" -> message |> CreateObjectStoreError.AlreadyExists |> Some
                    | "InvalidAccessError" -> message |> CreateObjectStoreError.InvalidKeyPath |> Some
                    | _ -> None
                )

        member _.DeleteObjectStore (name: string) : unit =
            raw.deleteObjectStore name

type Transaction internal(raw: IDBTransaction) =
    let database = lazy(Database(raw.db))

    interface ITransaction with
        member _.ObjectStoreNames: seq<string> =
            [0..raw.objectStoreNames.length - 1]
            |> Seq.map (fun objectStoreIndex -> raw.objectStoreNames.Item objectStoreIndex)

        member _.Database: IDatabase = database.Value

        member _.Mode: TransactionMode = TransactionMode.FromRaw(raw.mode)

        member _.GetObjectStore (name: string) : Result<IObjectStore<'T>, GetObjectStoreError> =
            wrapSync
                (fun () ->
                    let rawDbObjectStore = raw.objectStore(name)
                    let objectStore = ObjectStore<'T>(rawDbObjectStore)
                    objectStore
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> GetObjectStoreError.InvalidState |> Some
                    | "NotFoundError" -> message |> GetObjectStoreError.NotFound |> Some
                    | _ -> None
                )

        member _.Abort() : unit =
            raw.abort()

        member _.Commit() : unit =
            raw.commit()

type Database internal(raw: IDBDatabase) =
    interface IDatabase with
        member _.Name = raw.name

        member _.Version = raw.version |> uint64

        member _.ObjectStoreNames =
            [0..raw.objectStoreNames.length - 1]
            |> Seq.map (fun i -> raw.objectStoreNames.Item i)

        member _.BeginTransaction (objectStoreNames: #seq<string>) (mode: TransactionMode) (maybeOptions: Option<TransactionOptions>) : Result<ITransaction, BeginTransactionError> =
            wrapSync
                (fun () ->
                    let rawTransaction =
                        raw.transaction(
                            storeNames = objectStoreNames,
                            mode = mode.ToRaw(),
                            ?options = (maybeOptions |> Option.map (fun o -> o.ToRaw()))
                        )
                    let transaction = Transaction(rawTransaction)
                    transaction
                )
                (fun name message ->
                    match name with
                    | "InvalidStateError" -> message |> BeginTransactionError.InvalidState |> Some
                    | "NotFoundError" -> message |> BeginTransactionError.NotFound |> Some
                    | "InvalidAccessError" -> message |> BeginTransactionError.InvalidAccess |> Some
                    | _ -> None
                )

type IndexedDbService() =
    interface IIndexedDbService with
        member _.IsSupported: bool = nativeIsSupportedBrowserCheck()

        member _.GetDatabases (): Async<Result<seq<DatabaseInfo>, GetDatabasesError>> =
            async {
                try
                    let! databases = indexedDB.databases () |> Async.AwaitPromise

                    return
                        databases
                        |> Seq.map DatabaseInfo.FromRaw
                        |> Ok
                with
                | exn ->
                    return
                        match exn |> tryCoerce<DOMException> with
                        | Some domException ->
                            match domException.name with
                            | "SecurityError" ->  domException.message |> GetDatabasesError.SecurityViolation |> Error
                            | _ -> failwith $"DOM error with name '{domException.name}' is not currently handled"
                        | None ->
                            failwith $"Unhandled exception when getting databases: {exn}"
            }

        member _.OpenOrUpgradeDatabase (name: string) (maybeVersion: Option<Version>) (upgrade: UpgradeHandler): Async<Result<OpenedOrUpgradedDb, unit>> =
            let mutable maybeVersionUpgrade = None

            wrapDbRequest
                (fun () ->
                    let request =
                        indexedDB.``open``(
                            name  = name,
                            ?version = (maybeVersion |> Option.map int)
                        )

                    request.onupgradeneeded <-
                        (fun e ->
                            maybeVersionUpgrade <- Some (uint64 e.oldVersion, uint64 e.newVersion)

                            let rawDb = request.result |> forceCoerce<IDBDatabase>
                            let upgrader = DatabaseUpgrader(rawDb)

                            log.Debug("Requesting upgrade to DB '{Name}' from version {OldVersion} to version {NewVersion}", name, e.oldVersion, e.newVersion)

                            // Request the caller performs the upgrade.
                            try
                                upgrade (uint64 e.oldVersion) (uint64 e.newVersion) upgrader
                                log.Debug("DB '{Name}' was successfully upgraded from version {OldVersion} to version {NewVersion}", name, e.oldVersion, e.newVersion)
                            with
                            | exn ->
                                log.Error("DB '{Name}' could not be upgraded from version {OldVersion} to version {NewVersion} because the upgrade handler failed with exception {Exception}", name, e.oldVersion, e.newVersion, exn)
                                reraise ()
                        )

                    request
                )
                (fun r ->
                    match r |> Option.bind tryCoerce<IDBDatabase> with
                    | Some rawDb ->
                        let database: IDatabase = Database(rawDb)

                        match maybeVersionUpgrade with
                        | Some (oldVersion, newVersion) ->
                            (database, oldVersion, newVersion)
                            |> OpenedOrUpgradedDb.Upgraded
                        | None ->
                            database
                            |> OpenedOrUpgradedDb.Opened
                    | None ->
                        failwith "Invalid raw database result"
                )
                (fun _ _ -> None)

#endif

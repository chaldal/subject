[<AutoOpen>]
module
#if FABLE_COMPILER
    // See comment in LibLifeCycleTypes/AssemblyInfo.fs
    LibLifeCycleTypes_AccessControl
#else
    LibLifeCycleTypes.AccessControl
#endif

open System

type AccessUserId = // a bit weird type name to avoid name collisions with UserId elsewhere
| Anonymous
// AuthenticatedOn must be captured when user logs in, not to be updated upon revalidation
| Authenticated of UserIdStr: string * AuthenticatedOn: DateTimeOffset

type AccessControlled<'T, 'Id> =
| Granted of 'T
| Denied of 'Id

[<RequireQualifiedAccess>]
module AccessControlled =
    let map fn accessControlled =
        match accessControlled with
        | AccessControlled.Granted data ->
            data |> fn |> AccessControlled.Granted
        | AccessControlled.Denied id ->
            AccessControlled.Denied id

// Codecs

#if !FABLE_COMPILER

open CodecLib

type AccessUserId
with
    static member get_ObjCodec_AllVersions () =
        function
        | Anonymous ->
            codec {
                let! _ = reqWith Codecs.unit "Anonymous" (function Anonymous -> Some () | _ -> None)
                return Anonymous
            }
        | Authenticated _ ->
            codec {
                let! payload = reqWith (Codecs.tuple2 Codecs.string Codecs.dateTimeOffset) "Authenticated" (function Authenticated (x1, x2) -> Some (x1, x2) | _ -> None)
                return Authenticated payload
            }
        |> mergeUnionCases

    static member get_Codec () =
        AccessUserId.get_ObjCodec_AllVersions ()
        |> ofObjCodec


#endif
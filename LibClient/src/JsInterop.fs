module LibClient.JsInterop

open System
open Fable.Core.JsInterop

type TimeSpan = System.TimeSpan

open Fable.Core
open System.Reflection

let (==!>) (key: string) (v: obj) =
    key ==> v
    |> Some

let (==?>) (key: string) (maybeV: Option<'a>) =
    maybeV
    |> Option.map (fun v -> key ==> v)

// For cases where the properties of a JS object need to be selectively elided (such as when integrating
// with a JS library that makes assumptions about the validity of any included properties regardless of their
// actual value), use this function instead of createObj. The ==!> operator will always include the property
// in the object, whereas ==?> will only include it if the provided value is Some.
let createObjWithOptionalValues (maybeValues: #seq<(string * obj) option>) =
    maybeValues
    |> Seq.choose id
    |> createObj

#if FABLE_COMPILER
let runLater (delay: TimeSpan) (what: unit -> unit) : unit =
    Fable.Core.JS.setTimeout what (int delay.TotalMilliseconds)
    |> ignore
#else
let runLater (_delay: TimeSpan) (_what: unit -> unit) : unit =
    // TODO to get this working on the server side, we need to have some kind
    // of single-threaded event loop like thing going, probably using
    // SynchronizationContext, and posting `what` there.
    failwith "runLater is not implemented for running on the server side yet"
#endif

#if FABLE_COMPILER
let runLaterDisposable (delay: TimeSpan) (what: unit -> unit) : IDisposable =
    let timeoutId = Fable.Core.JS.setTimeout what (int delay.TotalMilliseconds)
    { new System.IDisposable with
        member _.Dispose() =
            Fable.Core.JS.clearTimeout timeoutId
    }

#else
let runLater (_delay: TimeSpan) (_what: unit -> unit) : unit =
    // TODO to get this working on the server side, we need to have some kind
    // of single-threaded event loop like thing going, probably using
    // SynchronizationContext, and posting `what` there.
    failwith "runLaterDisposable is not implemented for running on the server side yet"
#endif

#if FABLE_COMPILER
let runEvery (delay: TimeSpan) (what: unit -> unit) : IDisposable =
    let token = Fable.Core.JS.setInterval what (int delay.TotalMilliseconds)
    { new System.IDisposable with
        member _.Dispose() =
            Fable.Core.JS.clearInterval token
    }
#else
let runEvery (_delay: TimeSpan) (_what: unit -> unit) : IDisposable =
    failwith "runEvery is not implemented for running on the server side yet"
#endif

module Debounce =
    let mutable timer = 1

    let clear () : unit =
        Fable.Core.JS.clearTimeout(timer)

    #if FABLE_COMPILER
    let debounce (delay: TimeSpan) (what: unit -> unit) : unit =
        clear ()
        timer <- Fable.Core.JS.setTimeout what (int delay.TotalMilliseconds)
    #else
    let debounce (_delay: TimeSpan) (_what: unit -> unit) : unit =
        failwith "Debounce module is not implemented for running on the server side yet"
    #endif

let runOnNextTick : (unit -> unit) -> unit = runLater TimeSpan.Zero

// None is converted by Fable to a JS `null`, and in some cases,
// like when we're passing props and want to fall back on defaults,
// we want an `undefined`.
[<Emit("undefined")>]
let Undefined<'T> : Option<'T> = jsNative

let noneToUndefined (o: Option<'T>) : Option<'T> =
    match o with
    | Some t -> Some t
    | None   -> Undefined

let (?) = JsInterop.(?)

type JsNullable<'T> = NeverInstantiatedExplicitly
with
    member this.ToOption : Option<'T> =
        let rawOption = this :> obj |> Option.ofObj
        rawOption :> obj :?> Option<'T>

[<Emit("($0 instanceof Error)")>]
let isJsError (_candidate: obj) = jsNative

[<Emit("typeof($0) == 'undefined'")>]
let isUndefined (_: 'a) : bool = jsNative

// The reason it has `Native` in its name because this only applies to
// react native implementation. In react native it has a `global` keyword
// Which allow access to the global scope. Sort of an alternative of
// `window` keyword in browser
// This won't work for the web platform
[<Emit("global[$0] = $1")>]
let setNativeOnlyGlobal (_key: string) (_value: obj) : unit = jsNative

// All tupled functions in Fable are actually turned at call site (so _each_ call site where
// the function is called) into a lambda that takes a single tupledArg as a parameter,
// and pulls out individual parts of it using the [0], [1], etc syntax. This works consistently
// within Fable, since it's essentially a symmetric encoding (though feels rather wasteful... there
// must be a very good reason for it), but breaks down when you want to pass a regular mutli-argument
// function as a value to raw JS — instead this single-argument function is passed, and havoc insues.
// Note, that if you're passing it into a properly typed JS interface, Fable will do the right thing,
// but if you want to pass it as a raw value, it won't.
[<Emit("function () { return $0(Array.prototype.slice.call(arguments)); }")>]
let fixTupledFunctionOnRawJsBoundary (_f: 'T -> unit) : obj = jsNative

let objToMap (value: obj) : Map<string, obj> =
    value
    |> Fable.Core.JS.Constructors.Object.keys
    |> Seq.map (fun key ->
        (key, value?(key))
    )
    |> Map.ofSeq

[<Emit("Object.assign({}, $1, $0)")>]
let private extendNewEmptyObject<'T> (_additionalFields: obj) (_record: 'T) : 'T = jsNative

let extendRecordWithObj (additionalFieldsObj: obj) (record: 'T) : 'T =
    extendNewEmptyObject additionalFieldsObj record

let extendRecord (additionalFields: List<string * obj>) (record: 'T) : 'T =
    let additionalFieldsObj = Fable.Core.JsInterop.createObj additionalFields
    extendNewEmptyObject additionalFieldsObj record

[<Global>]
let encodeURI: string -> string = jsNative

[<Global>]
let encodeURIComponent: string -> string = jsNative

[<Global>]
let decodeURIComponent: string -> string = jsNative

let rec private deepEqualsHelper (x: obj) (y: obj) (keys: seq<string>) (examinedKeys: Set<string>) : bool =
    match Seq.tryHead keys with
    | None -> true
    | Some key ->
        if examinedKeys.Contains key then
            deepEqualsHelper x y (Seq.tail keys) examinedKeys
        else if x?(key) = y?(key) then
            deepEqualsHelper x y (Seq.tail keys) (examinedKeys.Add key)
        else
            false

let deepEquals (x: obj) (y: obj) : bool =
    if (isNull x) && (isNull y) then
        true
    else if (isUndefined x) && (isUndefined y) then
        true
    else
        let xKeys = JS.Constructors.Object.keys x
        let yKeys = JS.Constructors.Object.keys y
        if xKeys.Count <> yKeys.Count then
            false
        else
            deepEqualsHelper x y (Seq.append xKeys yKeys) Set.empty

module Reflection =
    let getRecordFieldValue (propertyInfo: PropertyInfo) (value: obj) : obj =
        value?(propertyInfo.Name)
        //propertyInfo.GetValue obj

    let private unionFieldNameRegex = new System.Text.RegularExpressions.Regex("^Data(\d)?$")

    // NOTE we'd like to be using PropertyInfo by which to get the field value,
    // but it seems like at runtime that data is unavailable, and field values are
    // just stored by index. Furthermore, teh Fable folks provided an implementation
    // for propertyInfo.GetValue, but it only works for records, not for union cases.
    let getUnionCaseFieldValue (fieldIndex: int) (value: obj) : obj =
        value?fields?(fieldIndex)

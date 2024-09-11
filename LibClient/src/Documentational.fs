[<AutoOpen>]
module LibClientDocumentational

let shouldNotReachHereBecauseWeChangedLocation<'T> : Async<'T> =
    Async.Never ()

let identity = id

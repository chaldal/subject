module LibClient.Services.HttpService.Types

type HttpAction = Get | Post | Put | Delete

let mapHttpResult<'T> (raw: obj) = raw :?> 'T

let NoPayload = fun () -> () :> obj

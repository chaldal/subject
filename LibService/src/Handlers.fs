module LibService.Handlers

open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http

let makePostHandler<'Request, 'Response> (doWork: 'Request -> Async<'Response>) : HttpHandler =
    (fun (_next: HttpFunc) (ctx: HttpContext) -> task {
        let! requestData = ctx.BindJsonAsync<'Request>()
        // TODO catch exceptions?
        // also need to let handlers specify http status code if they so desire
        let! response = doWork requestData
        return! ctx.WriteJsonAsync response
    })

let makeGetHandler<'Response> (doWork: unit -> Async<'Response>) : HttpHandler =
    (fun (_next: HttpFunc) (ctx: HttpContext) -> task {
        // TODO catch exceptions?
        // also need to let handlers specify http status code if they so desire
        let! response = doWork()
        return! ctx.WriteJsonAsync response
    })

let notAuthenticatedHandler : HttpHandler =
    RequestErrors.FORBIDDEN "Not authenticated"

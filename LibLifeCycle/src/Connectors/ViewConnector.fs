[<AutoOpen>]
module LibLifeCycle.Connectors.ViewConnector

open System
open LibLifeCycle
open System.Threading.Tasks

type ViewConnectorEnvironment = {
    ServiceProvider: IServiceProvider
} with interface Env

type ViewConnectorRequest<'Input, 'Output, 'OpError> =
| ReadView of 'Input * ResponseChannel<Result<'Output, 'OpError>>
with interface Request

let private requestProcessor (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>) (env: ViewConnectorEnvironment) (request: ViewConnectorRequest<'Input, 'Output, 'OpError>) : Task<ResponseVerificationToken> =
    match request with
    | ReadView (input, responseChannel) ->
        backgroundTask {
            let viewEnv = view.CreateEnv CallOrigin.Internal env.ServiceProvider
            let (ViewResult viewTask) = view.Read viewEnv input
            let! res = viewTask
            return responseChannel.Respond res
        }

let viewAsConnector (view: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>) =
    ConnectorBuilder.newConnector (sprintf "%sConnector" view.Name)
    |> ConnectorBuilder.withRequestProcessor (requestProcessor view)
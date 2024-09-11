[<RequireQualifiedAccess>]
module LibLifeCycle.ConnectorBuilder

open System.Threading.Tasks

type ConnectorName = private ConnectorName of string

let newConnector (name: string) : ConnectorName =
    ConnectorName name

let withRequestProcessor
        (messageProcessor: 'Env -> 'Request -> Task<ResponseVerificationToken>)
        (ConnectorName connectorName)
        : Connector<'Request, 'Env> =
    {
        RequestProcessor = messageProcessor
        Name = connectorName
        ShouldSendTelemetry = true
        Interceptors = []
    }

let withDisabledTelemetry (connector: Connector<'Request, 'Env>) =
    { connector with ShouldSendTelemetry = false }
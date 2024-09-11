namespace LibLifeCycleHost

open System
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Orleans
open Orleans.Concurrency
open Orleans.Runtime

open LibLifeCycle
open LibLifeCycleCore
open LibLifeCycleHost.AccessControl

[<StatelessWorker>]
[<Reentrant>]
type ViewGrain<'Input, 'Output, 'OpError
    when 'Input :> ViewInput<'Input>
    and  'Output :> ViewOutput<'Output>
    and 'OpError :> ViewOpError<'OpError>
    and 'OpError :> OpError>
    (
        viewAdapterCollection: ViewAdapterCollection,
        autoResolvedViewAdapter: ViewAdapter<'Input, 'Output, 'OpError>,
        serviceProvider: IServiceProvider,
        ctx: IGrainActivationContext
    ) =
    inherit Grain()

    let hostEcosystemGrainFactory = serviceProvider.GetRequiredService<IGrainFactory>()

    // views have higher risk of ambiguity by 'Input+'Output type combination, allow to specify view name explicitly
    let (grainPartition, explicitViewName) =
        let (grainPartition, pKey) = ctx.GrainIdentity.GetPrimaryKey()
        ((grainPartition |> GrainPartition), pKey)

    let viewAdapter: ViewAdapter<'Input, 'Output, 'OpError> =
        match viewAdapterCollection.GetViewAdapterByName explicitViewName with
        | Some untypedAdapter ->
            match untypedAdapter with
            | :? ViewAdapter<'Input, 'Output, 'OpError> as adapter -> adapter
            | _ -> autoResolvedViewAdapter
        | None ->
            autoResolvedViewAdapter

    let authorizeViewAndContinueWith
            (sessionHandle: SessionHandle)
            (callOrigin: CallOrigin)
            (input: 'Input)
            (unauthorizedResult: 'T)
            (authorizedContinuation: unit -> Task<'T>) =
        AccessControl.Views.authorizeViewAndContinueWith hostEcosystemGrainFactory grainPartition viewAdapter.View (SessionSource.Handle sessionHandle) callOrigin input unauthorizedResult authorizedContinuation

    let read
            (sessionHandle: SessionHandle)
            (callOrigin: CallOrigin)
            (input: 'Input) : Task<Result<'Output, GrainExecutionError<'OpError>>> =
        task {
            return!
                authorizeViewAndContinueWith
                    sessionHandle
                    callOrigin
                    input
                    (GrainExecutionError.AccessDenied |> Error)
                    (fun () ->
                        task {
                            let (ViewResult viewTask) = viewAdapter.View.Read callOrigin serviceProvider input
                            match! viewTask with
                            | Ok output -> return Ok output
                            | Error err -> return err |> GrainExecutionError.ExecutionError |> Error
                        }
                    )
        }

    interface IViewGrain<'Input, 'Output, 'OpError> with
        member _.Read (clientGrainCallContext: ClientGrainCallContext) (input: 'Input) =
            // TODO Not yet supported. This will only be used by Native Clients, and requires proper registration of input/output/operror serializers
            read clientGrainCallContext.SessionHandle clientGrainCallContext.CallOrigin input

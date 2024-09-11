[<AutoOpen>]
module LibLifeCycleTest.Cluster

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open LibLifeCycleHost.TelemetryModel
open Microsoft.Extensions.DependencyInjection
open LibLifeCycleCore
open Orleans
open System.Collections.Concurrent

type InitializationState =
| Uninitialized
| Initializing of Task<unit>
| Initialized

type TestPartition = {
    EcosystemDef:            EcosystemDef
    GrainPartition:          GrainPartition
    CapturedInteractions:    ConcurrentDictionary<SubjectId, Subject>
    mutable InitState:       InitializationState
    NamedValues:             ConcurrentDictionary<string, obj>
    mutable ConfigOverrides: Map<string, string>
    mutable UserId:          string
    mutable StasisWaitFor:   TimeSpan
}

type NamedValue<'T> = NamedValue of string
with
    member this.Name = match this with NamedValue str -> str
    static member ofName<'T> str : NamedValue<'T> = NamedValue str

type IClusterScope =
    inherit IDisposable

    abstract member ServiceProvider: IServiceProvider

type ICluster =
    inherit IAsyncDisposable

    abstract member Init: logOutput: (string -> unit) -> Task<unit>
    abstract member NewScope: TestPartition -> IClusterScope
    abstract member OperationTracker: OperationTracker
    abstract member ShouldExecutePartitionInitializer: bool

type LogicalCluster = internal {
    Root:          ICluster
    Partition:     TestPartition
} with
    member this.NewScope() =
        this.Root.NewScope this.Partition

type ClusterOperation<'T> = LogicalCluster -> Task<'T>

type PartitionOperation<'T> = TestPartition -> Task<'T>

let clusterOperationOfValue value =
    fun (_cluster: LogicalCluster) ->
        Task.FromResult value

let partitionOperationOfValue value =
    fun (_partition: TestPartition) ->
        Task.FromResult value

let defaultStasisWaitFor = TimeSpan.FromSeconds 15

// TODO: clear this out :)
type WhyIsThisHackNeeded =
// Subscriptions are processed asynchronously after a subject returns them
// In this duration, if an action on the subscription target would have triggered
// that subscription, this would get missed, even though the second action happens
// temporally after the first, causing relationships to break
// The way to solve this is to model relationships explicitly
| SubscriptionsAreProcessedAsynchronously
| ThereIsNoWayToObserveBehaviorThatShouldntHappenExceptWaiting

[<Extension>]
type IClusterScopeExtensions =
    [<Extension>]
    static member DefaultGrainFactory(clusterScope: IClusterScope) =
        clusterScope.ServiceProvider.GetRequiredService<IGrainFactory>()

    [<Extension>]
    static member DefaultGrainConnector(clusterScope: IClusterScope, grainPartition: GrainPartition) =
        let grainFactory = clusterScope.ServiceProvider.GetRequiredService<IGrainFactory>()
        GrainConnector(grainFactory, grainPartition, SessionHandle.NoSession, CallOrigin.Internal)

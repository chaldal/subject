[<AutoOpen>]
module LibLifeCycleTest.SimulationBuilder

open System
open System.Reflection
open System.Threading.Tasks
open FSharpPlus
open FsCheck
open LibLifeCycle
open LibLifeCycleTest

let private clusterOpToPartitionOp (rootCluster: ICluster) (clusterOp: ClusterOperation<'T>) : PartitionOperation<'T> =
    fun partition ->
        let cluster = { Root = rootCluster; Partition = partition }
        clusterOp cluster

type SimulationBuilder
    private (ecosystem, rootCluster: ICluster, testAssembly: Assembly, maybeInitializer: Option<PartitionOperation<unit>>) =

    let rootCluster = rootCluster
    let mutable rootClusterInitState = InitializationState.Uninitialized

    let maybeInitCluster () =
        lock rootCluster (
            fun _ ->
                match rootClusterInitState with
                | Initialized -> Task.FromResult ()
                | Initializing task -> task
                | Uninitialized ->
                    let initializingTaskSrc = TaskCompletionSource<unit>()
                    rootClusterInitState <- Initializing initializingTaskSrc.Task

                    backgroundTask {
                        try
                            initializingTaskSrc.SetResult(())
                        with
                        | ex ->
                            initializingTaskSrc.SetException(ex)
                    }
                    |> ignore

                    initializingTaskSrc.Task
        )

    let maybeInitPartition (partition: TestPartition) : Task<unit> =
        if rootCluster.ShouldExecutePartitionInitializer then
            match maybeInitializer with
            | None ->
                Task.FromResult ()
            | Some initializer ->
                lock partition (
                    fun _ ->
                        match partition.InitState with
                        | Initialized ->
                            Task.FromResult ()
                        | Initializing task ->
                            task
                        | Uninitialized ->
                            let initializingTaskSrc = TaskCompletionSource<unit>()
                            partition.InitState <- Initializing initializingTaskSrc.Task

                            backgroundTask {
                                try
                                    let! result = initializer partition
                                    initializingTaskSrc.SetResult(result)
                                with
                                | ex ->
                                    initializingTaskSrc.SetException(ex)
                            }
                            |> ignore

                            initializingTaskSrc.Task
                )
        else
            Task.FromResult ()

    let getCluster partition =
        backgroundTask {
            do! maybeInitCluster ()
            do! maybeInitPartition partition
            return { Root = rootCluster; Partition = partition }
        }

    new(ecosystem: Ecosystem, testAssembly: Assembly, configureServices: Microsoft.Extensions.DependencyInjection.IServiceCollection -> unit) =
        SimulationBuilder(ecosystem, new TestCluster(ecosystem, configureServices), testAssembly, None)

    new(ecosystem: Ecosystem, testAssembly: Assembly) =
        SimulationBuilder(ecosystem, new TestCluster(ecosystem, ignore), testAssembly, None)

    new(ecosystem: Ecosystem) =
        SimulationBuilder(ecosystem, Assembly.GetCallingAssembly())

    member _.TestAssembly =
        testAssembly

    member _.WithInitializer (initializer: PartitionOperation<unit>) =
        SimulationBuilder(ecosystem, rootCluster, testAssembly, Some initializer)

    member _.Bind (clusterOp: ClusterOperation<Gen<'T>>, binder: 'T -> PartitionOperation<'U>) : PartitionOperation<'U> =
        fun partition ->
            backgroundTask {
                let! cluster = getCluster partition
                let! genValue = clusterOp cluster
                let value = evalGen genValue
                return! binder value partition
            }

    member _.Bind (genValue: Gen<'T>, binder: 'T -> PartitionOperation<'U>) : PartitionOperation<'U> =
        fun partition ->
            backgroundTask {
                let value = evalGen genValue
                return! binder value partition
            }

    member _.Bind (clusterOp: ClusterOperation<'T>, binder: 'T -> PartitionOperation<'U>) : PartitionOperation<'U> =
        fun partition ->
            backgroundTask {
                let! cluster = getCluster partition
                let! value = clusterOp cluster
                return! binder value partition
            }

    member _.Bind (partitionOp: PartitionOperation<'T>, binder: 'T -> PartitionOperation<'U>) : PartitionOperation<'U> =
        fun partition ->
            backgroundTask {
                let! value = partitionOp partition
                return! binder value partition
            }

    member _.Return (value: 'T) : PartitionOperation<'T> =
        fun _partition ->
            Task.FromResult value

    member _.ReturnFrom (partitionOp: PartitionOperation<'T>) : PartitionOperation<'T> =
        fun partition ->
            backgroundTask {
                let! value = partitionOp partition
                return value
            }

    member _.ReturnFrom (clusterOp: ClusterOperation<'T>) : PartitionOperation<'T> =
        clusterOpToPartitionOp rootCluster clusterOp

    member this.Zero () = this.Return ()

    member _.TryFinally(body: unit -> PartitionOperation<'T>, compensation: unit -> unit) : PartitionOperation<'T> =
        fun partition ->
            backgroundTask {
                let! cluster = getCluster partition
                try
                    return! body () cluster.Partition
                finally
                    compensation()
            }

    member this.Using<'T, 'U when 'T :> IDisposable>(disposable: 'T, body: 'T -> PartitionOperation<'U>) : PartitionOperation<'U> =
        let body' = fun () -> body disposable
        this.TryFinally(body', fun () -> disposable.Dispose())

    member _.Delay(f) =
        f

    member _.Run(f) =
        f()

    member this.While(guard: unit -> bool, body: unit -> PartitionOperation<unit>) : PartitionOperation<unit> =
        fun partition ->
            backgroundTask {
                while guard() do
                    do! this.Bind(body(), fun () -> partitionOperationOfValue ()) partition
            }

    member this.For(sequence:seq<'T>, body: 'T -> PartitionOperation<unit>) =
        this.Using(sequence.GetEnumerator(), fun enum ->
            this.While(enum.MoveNext,
                this.Delay(fun () -> body enum.Current)))

    member this.Combine (a: PartitionOperation<unit>, b: unit -> PartitionOperation<'T>) : PartitionOperation<'T> =
        this.Bind(a, b)

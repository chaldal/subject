namespace LibLifeCycleTest.TestRunner // XUnit Test Discover doesn't work well within modules

open System
open System.Collections.Generic
open System.Threading.Tasks
open FSharp.Reflection
open LibLifeCycleHost.TelemetryModel
open Microsoft.Extensions.DependencyInjection
open Xunit.Sdk
open Xunit.Abstractions
open LibLifeCycleCore
open System.Threading
open LibLifeCycleTest

// Most of this was copy/pasted from FsCheck's PropertyAttribute/PropertyTestCase and PropertyDiscoverer and adapted,
// and so it likely doesn't adhere to our code standards

type SimulationTestCase(diagnosticMessageSink:IMessageSink, defaultMethodDisplay:TestMethodDisplay, testMethod:ITestMethod, ?testMethodArguments:obj []) =
    inherit XunitTestCase(diagnosticMessageSink, defaultMethodDisplay, TestMethodDisplayOptions.None, testMethod, (match testMethodArguments with | None -> null | Some v -> v))

    // Parameterless constructor is required for Visual Studio test runner, it performs test discovery in some backwards way unlike `dotnet test` or Rider
    new() = new SimulationTestCase(null, TestMethodDisplay.ClassAndMethod, (* testMethod *) null)

    // Run only two tests in parallel by default; the tests are fairly CPU heavy do
    // to various interactions and serialization/deserialization, and are never
    // blocked on I/O so if this value is made higher, we need to have at least that
    // many real CPU cores
    static member val Semaphore =
        System.Environment.GetEnvironmentVariable "PARALLELISM"
        |> Int32.TryParse
        |> function
           | true, v -> v
           | _ -> 2
        |> fun parallelism -> new SemaphoreSlim(parallelism)

    member private this.ProcessResult (test: XunitTest) (messageBus: IMessageBus) (cancellationTokenSource: CancellationTokenSource)  (testResultMsg: #TestResultMessage) =
        let summary = new RunSummary(Total = 1, Time = testResultMsg.ExecutionTime)
        messageBus.QueueMessage(testResultMsg) |> ignore
        if not (messageBus.QueueMessage(new TestFinished(test, summary.Time, testResultMsg.Output))) then
            cancellationTokenSource.Cancel() |> ignore
        if not (messageBus.QueueMessage(new TestCaseFinished(this, summary.Time, summary.Total, summary.Failed, summary.Skipped))) then
            cancellationTokenSource.Cancel() |> ignore

        summary

    override this.RunAsync(_diagnosticMessageSink:IMessageSink, messageBus:IMessageBus, constructorArguments:obj [], _aggregator:ExceptionAggregator, cancellationTokenSource: Threading.CancellationTokenSource) =
        let test = new XunitTest(this, this.DisplayName)
        let outputHelper = new TestOutputHelper()
        outputHelper.Initialize(messageBus, test)

        let dispose testClass =
            match testClass with
            | None -> ()
            | Some obj ->
                match box obj with
                | :? IDisposable as d -> d.Dispose()
                | _ -> ()

        let testExec() : Task<RunSummary> =
            backgroundTask {
                let timer = ExecutionTimer()

                try
                    let partitionGuid = Guid.NewGuid()
                    outputHelper.WriteLine(sprintf "Starting simulation \"%s\".\"%s\" on Grain Partition %A at %A"
                        this.TestMethod.TestClass.Class.Name this.TestMethod.Method.Name partitionGuid DateTimeOffset.Now)

                    let runMethod = this.TestMethod.Method.ToRuntimeMethod()

                    let target =
                        constructorArguments
                            |> Array.tryFind (fun x -> x :? TestOutputHelper)
                            |> Option.iter (fun x -> (x :?> TestOutputHelper).Initialize(messageBus, test))
                        let testClass = this.TestMethod.TestClass.Class.ToRuntimeType()
                        if this.TestMethod.TestClass <> null && not this.TestMethod.Method.IsStatic then
                            Some (test.CreateTestClass(testClass, constructorArguments, messageBus, timer, cancellationTokenSource))
                        else None

                    outputHelper.WriteLine(sprintf "Start running simulation at %A" DateTimeOffset.Now)

                    // The actual test method simply builds a workflow, and we don't need to count that into the test execution time
                    let taskOnPartition = runMethod.Invoke(target |> Option.toObj, Array.empty)
                    let partitionId = GrainPartition partitionGuid

                    let badLogCounters : LibLifeCycleTest.TestCluster.BadLogCounters = {
                        Warning  = ref 0
                        Error    = ref 0
                        Critical = ref 0
                    }

                    if LibLifeCycleTest.TestCluster.partitionIdToTestOutputHelper.TryAdd(partitionId, (outputHelper, badLogCounters)) |> not then
                        failwithf "Partition ID %A already exists, this is unexpected" partitionId

                    do! timer.AggregateAsync(
                            fun () ->
                                backgroundTask {
                                    do! SimulationTestCase.Semaphore.WaitAsync()
                                    try
                                        do! testRunnerCluster.Init(outputHelper.WriteLine)
                                        let rootOperationTracker = testRunnerCluster.OperationTracker
                                        do! rootOperationTracker.TrackOperation
                                                { Partition = partitionId
                                                  Type = OperationType.TestSimulation
                                                  Name = if testMethod <> null then testMethod.Method.Name else "NULL_METHOD"
                                                  MaybeParentActivityId = None
                                                  MakeItNewParentActivityId = true
                                                  BeforeRunProperties =
                                                    if testMethod <> null then
                                                      [
                                                        "Module", testMethod.TestClass.Class.Name
                                                        "Simulation", testMethod.Method.Name
                                                      ]
                                                    else []
                                                    |> Map.ofList }
                                                (fun () ->
                                                    backgroundTask {
                                                        let testPartition = {
                                                            EcosystemDef         = TestCluster.getEcosystemDefUnderTest()
                                                            GrainPartition       = partitionId
                                                            CapturedInteractions = System.Collections.Concurrent.ConcurrentDictionary<SubjectId, Subject>()
                                                            InitState            = InitializationState.Uninitialized
                                                            NamedValues          = System.Collections.Concurrent.ConcurrentDictionary<string, obj>()
                                                            StasisWaitFor        = defaultStasisWaitFor
                                                            ConfigOverrides      = Map.empty
                                                            UserId               = ""
                                                        }

                                                        do! taskOnPartition.GetType().GetMethod("Invoke").Invoke(taskOnPartition, [| testPartition |])
                                                        :?> System.Threading.Tasks.Task

                                                        return { ReturnValue = (); IsSuccess = Some true; AfterRunProperties = Map.empty }
                                                    })
                                    finally
                                        SimulationTestCase.Semaphore.Release() |> ignore
                                }
                                |> Task.Ignore
                            )

                    dispose target

                    removeAllConnectorInterceptionForGrainParition partitionId

                    match LibLifeCycleTest.TestCluster.partitionIdToTestOutputHelper.TryRemove partitionId with
                    | true, _ ->
                        match badLogCounters.GetFailureAggregateStringIfNonZero() with
                        | Some str ->
                            return failwithf "Simulation Completed in %.0f ms, however some bad logs were captured: %s. See test output for details"
                                (timer.Total * 1000m) str
                        | None ->
                            outputHelper.WriteLine(sprintf "Simulation Completed in %.0f ms" (timer.Total * 1000m))
                            let testResultMsg = TestPassed(test, timer.Total, outputHelper.Output)
                            return this.ProcessResult test messageBus cancellationTokenSource testResultMsg

                    | false, _ ->
                        return failwithf "Partition ID %A doesn't exist, this is unexpected" partitionId
                with
                | ex ->
                    outputHelper.WriteLine(sprintf "Exception during test, completed at %A" DateTimeOffset.Now)
                    let testResultMsg = TestFailed(test, timer.Total, outputHelper.Output, ex)
                    let summary = this.ProcessResult test messageBus cancellationTokenSource testResultMsg
                    summary.Failed <- summary.Failed + 1
                    return summary
            }

        if not (messageBus.QueueMessage(new TestCaseStarting(this))) then
            cancellationTokenSource.Cancel() |> ignore

        if not (messageBus.QueueMessage(new TestStarting(test))) then
            cancellationTokenSource.Cancel() |> ignore

        if not(String.IsNullOrEmpty(this.SkipReason)) then
            if not(messageBus.QueueMessage(new TestSkipped(test, this.SkipReason))) then
                cancellationTokenSource.Cancel() |> ignore
            if not(messageBus.QueueMessage(new TestCaseFinished(this, decimal 1, 0, 0, 1))) then
                cancellationTokenSource.Cancel() |> ignore
            RunSummary(Total = 1, Skipped = 1)
            |> Task.FromResult
        else
            testExec()

/// xUnit2 test case discoverer to link the method with the SimulationAttribute to the SimulationTestCase
/// so the test can be initiated with a random grain partition ID
type SimulationDiscoverer(messageSink: IMessageSink) =

    let hasZeroParameterCount (testMethod: ITestMethod) =
        testMethod.Method.GetParameters()
        |> Seq.isEmpty

    let doesReturnSimulationCExpr (testMethod: ITestMethod) =
        let retType = testMethod.Method.ReturnType.ToRuntimeType()
        if FSharpType.IsFunction retType then
            let methodInfo = retType.GetMethod("Invoke")
            if methodInfo <> null then
                let funcParams = methodInfo.GetParameters()
                funcParams.Length = 1 && funcParams.[0].ParameterType = typeof<TestPartition> && typeof<Task>.IsAssignableFrom methodInfo.ReturnType
            else
                false
        else
            false

    new () = SimulationDiscoverer(null)

    member __.MessageSink = messageSink

    interface IXunitTestCaseDiscoverer with

        override this.Discover(discoveryOptions:ITestFrameworkDiscoveryOptions, testMethod: ITestMethod, attr: IAttributeInfo) =
            if attr.GetNamedArgument<bool>("IsForDataSeedingOnly") then
                Seq.empty
            else
                if hasZeroParameterCount testMethod && doesReturnSimulationCExpr testMethod then
                    let ptc = new SimulationTestCase(this.MessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod)
                    Seq.singleton (ptc :> IXunitTestCase)
                else
                    Seq.empty

open System.Reflection

type SimulationTestFrameworkExecutor(assemblyName: AssemblyName, sourceInformationProvider: ISourceInformationProvider, messageSink: IMessageSink) =
    inherit XunitTestFrameworkExecutor(assemblyName, sourceInformationProvider, messageSink)

    override this.RunTestCases(testCases: IEnumerable<IXunitTestCase>, executionMessageSink: IMessageSink, executionOptions: ITestFrameworkExecutionOptions) =
        let testAssembly = base.TestAssembly
        let diagnosticMessageSink = base.DiagnosticMessageSink
        task {
            try
                use assemblyRunner = new XunitTestAssemblyRunner(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
                do! assemblyRunner.RunAsync() |> Task.Ignore
            finally
                // can't do! in finally
                testRunnerCluster.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult()
        }
        |> ignore


type SimulationTestFramework(messageSink: IMessageSink) =
    inherit XunitTestFramework(messageSink)

    override this.CreateExecutor (assemblyName: AssemblyName) : ITestFrameworkExecutor =
        new SimulationTestFrameworkExecutor(assemblyName, this.SourceInformationProvider, this.DiagnosticMessageSink)

type SimulationTestFrameworkTypeDiscoverer() =
    do()
with
    interface ITestFrameworkTypeDiscoverer with
        member _.GetTestFrameworkType(_attribute: IAttributeInfo) : Type =
            typeof<SimulationTestFramework>

/// Declare this attribute in your test assembly to enable graceful cleanup
[<TestFrameworkDiscoverer("LibLifeCycleTest.TestRunner.SimulationTestFrameworkTypeDiscoverer", "LibLifeCycleTest")>]
[<AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)>]
type SimulationTestFrameworkAttribute() =
    inherit Attribute()
    with
        interface ITestFrameworkAttribute
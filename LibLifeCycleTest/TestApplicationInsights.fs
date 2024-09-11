[<AutoOpen>]
module LibLifeCycleTest.TestApplicationInsights

open System.Collections.Concurrent
open LibLifeCycleCore
open LibLifeCycleHost.ApplicationInsights
open Microsoft.ApplicationInsights
open System.Threading.Tasks

let private testingOnlyPartitionIdToRootActivityParentId = ConcurrentDictionary<GrainPartition, string>()

type TestAppInsightsOperationTracker (telemetryClient: TelemetryClient) =
    inherit AppInsightsOperationTrackerBase(telemetryClient)
    override _.RootParentActivityId partition =
        match testingOnlyPartitionIdToRootActivityParentId.TryGetValue partition with
        | false, _ ->
            None
        | true, testRootActivityParentId ->
            Some testRootActivityParentId

    override _.OnActivityStarted partition activityId telemetry =
        // first operation for a partition will be marked as root for the whole simulation
        testingOnlyPartitionIdToRootActivityParentId.TryAdd (partition, activityId) |> ignore
        if isSimulatedTimeSet partition then
            telemetry.Properties.Add("SimulatedTime", (simulatedNowNoIncrement partition).ToString("yyyy-MM-dd HH:mm:ss.fffff"))

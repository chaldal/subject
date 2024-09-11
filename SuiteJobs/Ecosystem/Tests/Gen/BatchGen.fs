[<AutoOpen>]
module BatchGen

open System
open FsCheck
open LibLifeCycleTest
open SuiteJobs.Types


let genBatchId = gen {
    let guid = Guid.NewGuid()
    return guid.ToTinyUuid() |> BatchId
}

let genParallelBatchJobsToConstruct = simulation {
    let! count = Gen.choose (2, 4)
    let! now = Ecosystem.now
    let! jobsData = Gen.listOfLength count (genTypicalJobConstructorCommonDataWithSentOn now)
    let! jobIds = Gen.listOfLength count genJobId
    return BatchJobsToConstruct.Parallel (List.zip jobIds jobsData)
}


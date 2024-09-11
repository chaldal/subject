[<AutoOpen>]
module JobGen

open System
open FsCheck
open LibLifeCycleTest
open SuiteJobs.Types

let genJobId = gen {
    let guid = Guid.NewGuid()
    return guid.ToTinyUuid() |> JobId
}

let genTypicalJobConstructorCommonDataWithSentOn sentOn =
  gen {
    let! queueName =
      genUniqueLipsumWordCapitalized
      |> Gen.map NonemptyString.ofStringUnsafe

    let! displayName =
      genUniqueLipsumWordCapitalized
      |> Gen.map NonemptyString.ofStringUnsafe

    return
      {
          Payload =
              { Type = "NotDone"
                Method = "NotDone"
                Arguments = "[]"
                ParameterTypes = "[]"
                CustomContext = "" }
          DisplayName = displayName
          QueueName = queueName
          QueueSortOrder = 100us
          FailurePolicy = { MaybeAutoRetries = None }
          SentOn = sentOn
          CorrelationId = None
      }
  }

let genTypicalJobConstructorCommonData =
  simulation {
    let! now = Ecosystem.now
    let! common = genTypicalJobConstructorCommonDataWithSentOn now
    return common
  }
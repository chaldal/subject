[<AutoOpen>]
module LibLifeCycleCore.ClientTypes

// This is used to create multiple logical clusters within a single Orleans cluster
// and was put in place so we can allow tests to run in parallel without interfering with
// each other. This may have some uses in production in the future; for example, testing vs staging vs production
type GrainPartition = GrainPartition of System.Guid

let defaultGrainPartition = System.Guid.Empty |> GrainPartition

module internal LibLifeCycleHost.OrleansTransientErrorDetection

open System
open System.Threading.Tasks

#nowarn "0044" // rudiment usage of SubjectException

// TODO: see also SqlServerTransientErrorDetection.  Can we unify and simplify transient & permanent exception detection?

let wrapTransientExceptions (startTask: unit -> Task<'T>) : Task<'T> =
    backgroundTask {
        try
            let! x = startTask()
            return x
        with
        | :? Orleans.Runtime.OrleansException as ex ->
            return TransientSubjectException ("Orleans", ex.ToString()) |> raise

        | :? System.TimeoutException as ex ->
            return TransientSubjectException ("Orleans", ex.ToString()) |> raise

        | :? InvalidOperationException as ex when ex.Message.Contains "Grain directory is stopping" ->
            return TransientSubjectException ("Orleans", ex.ToString()) |> raise

        | ex ->
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
            return shouldNotReachHereBecause "line above throws"
    }
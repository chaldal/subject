module LibLifeCycleHost.Storage.SqlServer.SqlServerTransientErrorDetection

open System.Threading.Tasks
open CodecLib.StjCodecs
open LibLifeCycle

// TODO: see also OrleansTransientErrorDetection.  Can we unify and simplify transient & permanent exception detection?

let wrapTransientExceptions (startTask: unit -> Task<'T>) =
    backgroundTask {
        try
            let! x = startTask()
            return x
        with
        | :? PermanentSubjectException as ex ->
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
            return shouldNotReachHereBecause "line above throws"

        | :? CodecCheckException as ex ->
            // ugly hack to detect if it was just OutOfMemory - this is transient.
            if ex.Message.Contains "OutOfMemoryException" then
                return TransientSubjectException ("Sql", ex.ToString()) |> raise
            else
                // codec exceptions treated as permanent
                // TODO: do we really need to treat it as permanent?
                return PermanentSubjectException ("Sql", ex.ToString()) |> raise

        | :? Orleans.Storage.InconsistentStateException as ex ->
            // must rethrow InconsistentStateException as is, it tells Orleans to recycle the grain
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
            return shouldNotReachHereBecause "line above throws"

        | ex ->
            return TransientSubjectException ("Sql", ex.ToString()) |> raise
    }
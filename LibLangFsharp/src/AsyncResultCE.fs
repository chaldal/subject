namespace LibLangFsharp

// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
//
// This is adopted from https://github.com/demystifyfp/FsToolkit.ErrorHandling
// there's lots more in there. We should evaluate the stuff in there, and decide
// what we want to do with it — pull in as a library and re-export, or bring into
// our own code base only the pieces we need. Either way, currently I need
// this AsyncResult computation expression because of some otherwise unwieldy
// code, so just dumping it here and adjusting minimally to make it compile.

open System.Threading.Tasks
open System

[<AutoOpen>]
module AsyncResultCE =

  type AsyncResultBuilder() =

    member _.Return (value: 'T) : Async<Result<'T, 'Error>> =
      async.Return <| resultful.Return value

    member _.ReturnFrom
        (asyncResult: Async<Result<'T, 'Error>>)
        : Async<Result<'T, 'Error>> =
      asyncResult

    #if !FABLE_COMPILER
    member _.ReturnFrom
        (taskResult: Task<Result<'T, 'Error>>)
        : Async<Result<'T, 'Error>> =
      Async.AwaitTask taskResult
    #endif

    member _.ReturnFrom
        (result: Result<'T, 'Error>)
        : Async<Result<'T, 'Error>> =
      async.Return result

    member _.Zero () : Async<Result<unit, 'Error>> =
      async.Return <| resultful.Zero ()

    member _.Bind
        (asyncResult: Async<Result<'T, 'Error>>,
         binder: 'T -> Async<Result<'U, 'Error>>)
        : Async<Result<'U, 'Error>> =
      async {
        let! result = asyncResult
        match result with
        | Ok x -> return! binder x
        | Error x -> return Error x
      }

    #if !FABLE_COMPILER
    member this.Bind
        (taskResult: Task<Result<'T, 'Error>>,
         binder: 'T -> Async<Result<'U, 'Error>>)
        : Async<Result<'U, 'Error>> =
      this.Bind(Async.AwaitTask taskResult, binder)
    #endif

    member this.Bind
        (result: Result<'T, 'Error>, binder: 'T -> Async<Result<'U, 'Error>>)
        : Async<Result<'U, 'Error>> =
      this.Bind(this.ReturnFrom result, binder)

    member _.Delay
        (generator: unit -> Async<Result<'T, 'Error>>)
        : Async<Result<'T, 'Error>> =
      async.Delay generator

    member this.Combine
        (computation1: Async<Result<unit, 'Error>>,
         computation2: Async<Result<'U, 'Error>>)
        : Async<Result<'U, 'Error>> =
      this.Bind(computation1, fun () -> computation2)

    member _.TryWith
        (computation: Async<Result<'T, 'Error>>,
         handler: System.Exception -> Async<Result<'T, 'Error>>)
        : Async<Result<'T, 'Error>> =
      async.TryWith(computation, handler)

    member _.TryFinally
        (computation: Async<Result<'T, 'Error>>,
         compensation: unit -> unit)
        : Async<Result<'T, 'Error>> =
      async.TryFinally(computation, compensation)

    member _.Using
        (resource: 'T when 'T :> IDisposable,
         binder: 'T -> Async<Result<'U, 'Error>>)
        : Async<Result<'U, 'Error>> =
      async.Using(resource, binder)

    member this.While
        (guard: unit -> bool, computation: Async<Result<unit, 'Error>>)
        : Async<Result<unit, 'Error>> =
      if not <| guard () then this.Zero ()
      else this.Bind(computation, fun () -> this.While (guard, computation))

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Async<Result<unit, 'Error>>)
        : Async<Result<unit, 'Error>> =
      this.Using(sequence.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext,
          this.Delay(fun () -> binder enum.Current)))



[<AutoOpen>]
module AsyncResultCEExtensions =

  // Having Async<_> members as extensions gives them lower priority in
  // overload resolution between Async<_> and Async<Result<_,_>>.
  type AsyncResultBuilder with

    member _.ReturnFrom (async': Async<'T>) : Async<Result<'T, 'Error>> =
      async {
        let! x = async'
        return Ok x
      }
    #if !FABLE_COMPILER
    member _.ReturnFrom (task: Task<'T>) : Async<Result<'T, 'Error>> =
      async {
        let! x = Async.AwaitTask task
        return Ok x
      }

    member _.ReturnFrom (task: Task) : Async<Result<unit, 'Error>> =
      async {
        do! Async.AwaitTask task
        return resultful.Zero ()
      }
    #endif

    member this.Bind
        (async': Async<'T>, binder: 'T -> Async<Result<'U, 'Error>>)
        : Async<Result<'U, 'Error>> =
      let asyncResult = async {
        let! x = async'
        return Ok x
      }
      this.Bind(asyncResult, binder)


    #if !FABLE_COMPILER
    member this.Bind
        (task: Task<'T>, binder: 'T -> Async<Result<'U, 'Error>>)
        : Async<Result<'U, 'Error>> =
      this.Bind(Async.AwaitTask task, binder)

    member this.Bind
        (task: Task, binder: unit -> Async<Result<'T, 'Error>>)
        : Async<Result<'T, 'Error>> =
      this.Bind(Async.AwaitTask task, binder)
    #endif
  let asyncResult = AsyncResultBuilder()
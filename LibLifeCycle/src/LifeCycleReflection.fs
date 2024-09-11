module rec LibLifeCycle.LifeCycleReflection

open System.Reflection
open Microsoft.FSharp.Reflection
open System
open System.Threading.Tasks


// Monads can't really be introspected statically, so to get list of potentially valid actions for a given subject the transition monad
// needs to be ran through interpreter. However our underlying monad type is not some form of Free but a "hot" system Task i.e. not 100% pure.
// So to keep it as close to static introspection as possible, all impure dependencies are auto-stubbed so resultant transition Task is
// almost instantly completed and has no IO.

// Implementation makes following assumptions:
// - transition has zero implicit impure dependencies
// - all dependencies or passed inside 'Env in the form of Service<_> abstraction such as Service<Clock>
//   rather than some other interface or abstract class
// - 'LifeAction is a union type with normal F# type arguments
// - the very first thing transition does is checking whether this action allowed for
//     given subject in principle (i.e. at least for some of action arguments)
// - if action is not allowed in principle (i.e. regardless of action arguments) then transition returns
//     some distinct type of error that is different from other validation errors

let isActionAllowedForSubject
    (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (subject: 'Subject)
    (action: 'LifeAction)
    : bool =

    let (TransitionResult task) =
        lifeCycle.Invoke
            { new FullyTypedLifeCycleFunction<_, _, _, _, _, _, _> with
                member _.Invoke (lifeCycle: LifeCycle<_, _, _, _, _, _, _, _, _, _, 'Env>) =
                   let stubEnv = mkStubInstanceOfType' typeof<'Env>
                   lifeCycle.Transition (stubEnv :?> 'Env) subject action }

    try
        task.Wait() // should be sync & almost instant anyway, because everything is a stub
        match task.Result with
        | Ok _ ->
            // Note that updated subject is deliberately ignored.
            // **Do NOT attempt** to deduce destination states this way, it will be wrong: some actions lead to different states
            //   depending on action arguments, but here it's just one set of arguments which is also garbage (stub)
            true
        | Error TransitionNotAllowed ->
            false

        | Error (LifeCycleError _)
        | Error (LifeCycleException _) ->
            // that's right, if exception raised then action is still allowed,
            // the likely reason of exception is garbage arguments and service stubs
            true
    with
        | _ ->  true

let allowedActionsForSubject
    (lifeCycle: ILifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectId>)
    (subject: 'Subject)
    : list<UnionCase<'LifeAction>> =

    let actionCases = FSharpType.GetUnionCases typeof<'LifeAction>

    actionCases
    |> Seq.map (fun actionCase ->
        let actionArgs =
            actionCase.GetFields ()
            |> Array.map (fun pi -> pi.PropertyType |> mkStubInstanceOfType')
        FSharpValue.MakeUnion (actionCase, actionArgs) :?> 'LifeAction)
    |> Seq.filter (isActionAllowedForSubject lifeCycle subject)
    |> Seq.map UnionCase.ofCase
    |> List.ofSeq

let private mkStubInstanceOfType' (t: Type): obj =
    if FSharpType.IsFunction t
    then
        let (_, rt) = FSharpType.GetFunctionElements t
        FSharpValue.MakeFunction (t, fun _ -> mkStubInstanceOfType' rt)

    elif FSharpType.IsRecord t // normal class, make instance recursively
    then
        let args =
            FSharpType.GetRecordFields t
            |> Array.map (fun pi -> pi.PropertyType |> mkStubInstanceOfType')
        FSharpValue.MakeRecord (t, args)

    elif FSharpType.IsTuple t // size of tuple, create default ...
    then
        let args =
            FSharpType.GetTupleElements t
            |> Array.map mkStubInstanceOfType'
        FSharpValue.MakeTuple (args, t)

    elif FSharpType.IsUnion t // abstract type. create first union case ?
    then
        let case = (FSharpType.GetUnionCases t).[0]
        let args =
            case.GetFields ()
            |> Array.map (fun pi -> pi.PropertyType |> mkStubInstanceOfType')

        FSharpValue.MakeUnion (case, args)

    elif t = typeof<string>
    then
        ("" :> obj)

    elif t.IsValueType
    then
        Activator.CreateInstance (t)

    // special cases for tasks
    elif t = typeof<Task>
    then
        Task.CompletedTask :> obj

    elif t.IsConstructedGenericType && t.GetGenericTypeDefinition() = typeof<Task<obj>>.GetGenericTypeDefinition()
    then
        let taskValueType = t.GetGenericArguments().[0]
        let method = typeof<Task>.GetMethod("FromResult")
        let genericMethod = method.MakeGenericMethod (taskValueType)
        genericMethod.Invoke(null, [| mkStubInstanceOfType' taskValueType |])

    // special case for LibLifeCycle.Services.Service abstract class
    elif t.IsGenericType && t.GetGenericTypeDefinition() = typeof<Service<_>>.GetGenericTypeDefinition()
    then
        let serviceValueType = t.GetGenericArguments().[0]
        let stubServiceType = typeof<StubService<_>>.GetGenericTypeDefinition().MakeGenericType([| serviceValueType |])
        mkStubInstanceOfType' stubServiceType

    // special case for Map<,> - make empty map
    elif t.IsGenericType && t.GetGenericTypeDefinition() = typeof<Map<_, _>>.GetGenericTypeDefinition()
    then
        t.GetProperty("Empty", BindingFlags.Static ||| BindingFlags.NonPublic).GetValue(null)

    // special case for Set<> - make empty set
    elif t.IsGenericType && t.GetGenericTypeDefinition() = typeof<Set<_>>.GetGenericTypeDefinition()
    then
        t.GetProperty("Empty", BindingFlags.Static ||| BindingFlags.NonPublic).GetValue(null)

    elif t.IsInterface
    then
        failwithf "What kind of type is it? Interface: %A" t
    elif t.IsAbstract
    then
        failwithf "What kind of type is it? Abstract: %A" t
    elif t.IsClass
    then
        // just a reference class with constructor
        let ctor =
            t.GetConstructors (BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
            |> Seq.sortByDescending (fun x -> x.GetParameters().Length)
            |> Seq.head

        let args =
            ctor.GetParameters ()
            |> Array.map (fun p -> p.ParameterType |> mkStubInstanceOfType')
        ctor.Invoke args
    else
        failwithf "What kind of type is it? %A" t

let private mkStubInstanceOfType<'T>() : 'T = (mkStubInstanceOfType' typeof<'T>) :?> 'T

type private StubService<'Request when 'Request :> Request>() =
    interface Service<'Request> with
        override _.Name = Guid.NewGuid().ToString()
        override _.QueryAndReturnRequestResponse<'Response> (_: (ResponseChannel<'Response> -> 'Request)) : 'Request * System.Threading.Tasks.Task<'Response> =
            mkStubInstanceOfType<'Request * System.Threading.Tasks.Task<'Response>>()
        override this.Query<'Response> (_: (ResponseChannel<'Response> -> 'Request)) : System.Threading.Tasks.Task<'Response> =
            mkStubInstanceOfType<System.Threading.Tasks.Task<'Response>>()

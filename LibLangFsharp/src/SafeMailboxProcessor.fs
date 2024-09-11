module LibLangFsharp.SafeMailboxProcessor

[<RequireQualifiedAccess>]
type NextStep<'State> =
| Continue of 'State
| Shutdown

[<RequireQualifiedAccess>]
type StatelessNextStep =
| Continue
| Shutdown

// F#'s crappy tooling (lack of @tailrec annotation) combined with some non-trivial ways in
// which one can screw up tail recursion (do! instead of return!, try/with block) makes it
// necessary to write a safe wrapper for the mailbox processor.
//
// NOTE The `return! mainLoop` below is important, even though a do! is sufficient
// An F# bug causes do! to not convert to tail-recursion. If we don't "tail-recurse" this,
// it will lead to a memory leak (in regular recursion case it would have been a stack overflow,
// but with `async` in play it manifests as a memory leak).
let makeSafeMailboxProcessor<'Message, 'State> (processMessageRaw: 'State -> 'Message -> Async<NextStep<'State>>) (onError: 'State -> 'Message -> exn -> Async<NextStep<'State>>) (initialState: 'State) =
    let processMessage (state: 'State) (message: 'Message) : Async<NextStep<'State>> = async {
        let! result = processMessageRaw state message |> Async.TryCatch

        match result with
        | Ok nextStep -> return nextStep
        | Error error -> return! onError state message error
    }

    new MailboxProcessor<'Message>
        (fun inbox ->
            let rec mainLoop (state: 'State) = async {
                let! message = inbox.Receive()
                let! nextStep = processMessage state message
                match nextStep with
                | NextStep.Continue nextState -> return! mainLoop nextState
                | NextStep.Shutdown           -> ()
            }

            async {
                do! mainLoop initialState
            }
        )

let private mapStatelessToStatefulUnitNextStep (statelessNextStep: StatelessNextStep) : NextStep<unit> =
    match statelessNextStep with
    | StatelessNextStep.Continue -> NextStep.Continue ()
    | StatelessNextStep.Shutdown -> NextStep.Shutdown

let makeSafeStatelessMailboxProcessor<'Message> (processMessageRaw: 'Message -> Async<StatelessNextStep>) (onError: 'Message -> exn -> Async<StatelessNextStep>) =
    makeSafeMailboxProcessor<'Message, unit>
        (fun _ message       -> processMessageRaw message |> Async.Map mapStatelessToStatefulUnitNextStep)
        (fun _ message error -> onError message error     |> Async.Map mapStatelessToStatefulUnitNextStep)
        ()

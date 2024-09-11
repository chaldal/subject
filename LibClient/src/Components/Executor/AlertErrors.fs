[<AutoOpen>]
module LibClient.Components.Executor_AlertErrors

open Fable.React
open LibClient
open LibClient.Components

[<RequireQualifiedAccess>]
module private Actions =
    let processErrors (executorErrorsLazy: ExecutorErrorsLazy) : unit =
        match executorErrorsLazy() with
        | None -> Noop
        | Some errors ->
            let message =
                errors.Values
                |> Seq.map (fun e ->
                    e.Dismiss () // since we've effectively "read" the error
                    e.Message
                )
                |> String.concat "\n\n"

            JsInterop.runOnNextTick (fun () ->
                Action.alert message
            )

type LibClient.Components.Constructors.LC.Executor with
    [<Component>]
    static member AlertErrors(
            ``with``: MakeExecutor -> ReactElement,
            ?showTopLevelSpinnerForKeys: LC.Executor.ShowTopLevelSpinnerForKeys,
            ?key: string
        ) : ReactElement =
        key |> ignore

        LC.Executor.DisplayErrorsManually(
            content =
                (fun (executorRef, getErrors) ->
                    Actions.processErrors getErrors
                    ``with`` executorRef
                ),
            ?showTopLevelSpinnerForKeys = showTopLevelSpinnerForKeys
        )

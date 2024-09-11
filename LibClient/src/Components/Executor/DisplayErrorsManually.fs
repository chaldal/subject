[<AutoOpen>]
module LibClient.Components.Executor_DisplayErrorsManually

open Fable.React
open LibClient
open LibClient.Components
open LibLangFsharp
open ReactXP.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module private Styles =
    let everything =
        makeViewStyles {
            flex 1
        }

    let spinnerOverlay =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
            backgroundColor (Color.WhiteAlpha 0.5)
            JustifyContent.Center
            AlignItems.Center
        }

[<RequireQualifiedAccess>]
module private Actions =
    let makeExecutor
            (executorsHook: IStateHook<Map<UDActionKey, Executor>>)
            (errorsHook: IStateHook<Map<UDActionKey, UDActionError>>)
            (shouldBeActionableWhenDisplayingErrors: bool)
            (key: UDActionKey)
            : Executor =
        match executorsHook.current.TryFind key with
        | Some executor -> executor
        | None ->
            let rec executor =
                Executor.Actionable (fun action ->
                    let completionDeferred = Deferred()

                    // executors like this one are obtained inside the render function,
                    // but there's a chance that they can be used immediately, which would
                    // cause the setState on this component to be called from within the
                    // render function of another component, which react does not like.
                    // So we delay execution of the whole until the next tick.
                    JsInterop.runOnNextTick (fun () ->
                        async {
                            executorsHook.update (fun current -> current.AddOrUpdate (key, Executor.InProgress))

                            match! action() with
                            | Ok () ->
                                executorsHook.update (fun current -> current.Add (key, executor))
                            | Error message ->
                                let dismiss =
                                    fun () ->
                                        executorsHook.update (fun current -> current.Add (key, executor))
                                        errorsHook.update (fun current -> current.Remove key)

                                let error = {
                                    Message = message
                                    Dismiss = dismiss
                                }

                                executorsHook.update (fun current ->
                                    if shouldBeActionableWhenDisplayingErrors then
                                        current.Add (key, executor)
                                    else
                                        current.Add (key, Executor.Error error)
                                )
                                errorsHook.update (fun current -> current.AddOrUpdate (key, error))

                            completionDeferred.Resolve ()
                        }
                        |> startSafely
                    )

                    completionDeferred
                )

            JsInterop.runOnNextTick (fun () ->
                executorsHook.update (fun current -> current.Add (key, executor))
            )

            executor

    let executorErrorsLazy
            (errorsHook: IStateHook<Map<UDActionKey, UDActionError>>)
            (errorsExaminedOnLastRenderHook: IRefValue<bool>)
            ()
            : Option<NonemptyMap<UDActionKey, UDActionError>> =
        errorsExaminedOnLastRenderHook.current <- true
        NonemptyMap.ofMap errorsHook.current

[<RequireQualifiedAccess>]
module private Helpers =
    let shouldShowSpinner (showTopLevelSpinnerForKeys: LC.Executor.ShowTopLevelSpinnerForKeys) (executors: Map<UDActionKey, Executor>) : bool =
        let inProgressKeys = executors |> Map.filterValues (function Executor.InProgress -> true | _ -> false) |> Map.keys
        if inProgressKeys.IsNonempty then
            match showTopLevelSpinnerForKeys with
            | LC.Executor.ShowTopLevelSpinnerForKeys.All       -> true
            | LC.Executor.ShowTopLevelSpinnerForKeys.Some keys -> (Set.intersect keys inProgressKeys).IsNonempty
        else false

type LibClient.Components.Constructors.LC.Executor with
    [<Component>]
    static member DisplayErrorsManually(
            content: (MakeExecutor * ExecutorErrorsLazy) -> ReactElement,
            ?showTopLevelSpinnerForKeys: LC.Executor.ShowTopLevelSpinnerForKeys,
            ?shouldBeActionableWhenDisplayingErrors: bool
        ) : ReactElement =
        let shouldBeActionableWhenDisplayingErrors = defaultArg shouldBeActionableWhenDisplayingErrors true

        let errorsExaminedOnLastRenderHook = Hooks.useRef false
        let executorsHook = Hooks.useState<Map<UDActionKey, Executor>> Map.empty
        let errorsHook = Hooks.useState<Map<UDActionKey, UDActionError>> Map.empty

        errorsExaminedOnLastRenderHook.current <- false

        let result =
            match showTopLevelSpinnerForKeys with
            | None ->
                content
                    (
                        Actions.makeExecutor executorsHook errorsHook shouldBeActionableWhenDisplayingErrors,
                        Actions.executorErrorsLazy errorsHook errorsExaminedOnLastRenderHook
                    )
            | Some keys ->
                RX.View(
                    styles = [| Styles.everything |],
                    children =
                        elements {
                            content
                                (
                                    Actions.makeExecutor executorsHook errorsHook shouldBeActionableWhenDisplayingErrors,
                                    Actions.executorErrorsLazy errorsHook errorsExaminedOnLastRenderHook
                                )

                            if Helpers.shouldShowSpinner keys executorsHook.current then
                                RX.View(
                                    styles = [| Styles.spinnerOverlay |],
                                    children =
                                        elements {
                                            RX.ActivityIndicator(
                                                size = Size.Medium,
                                                color = "#cccccc"
                                            )
                                        }
                                )
                        }
                )

        if not errorsExaminedOnLastRenderHook.current then
            Log.Error "Using Executor.Base but not displaying errors. Make sure to call getErrors() and display the result, or use Executor.AlertOnError instead."

        result

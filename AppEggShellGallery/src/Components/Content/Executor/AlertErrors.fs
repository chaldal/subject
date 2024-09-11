[<AutoOpen>]
module AppEggShellGallery.Components.Content_With_Executor

open Fable.React
open LibClient
open LibClient.Components

let failAsynchronously (): Async<Result<unit, UDActionErrorMessage>> =
    async {
        do! Async.Sleep 1000
        return Error "Something went wrong"
    }

type Ui.Content.Executor with
    [<Component>]
    static member AlertErrors () : ReactElement =
        Ui.ComponentContent (
            displayName = "AlertErrors",
            isResponsive = false,
            samples = (
                element {
                    Ui.ComponentSampleGroup(
                        samples = (
                            element {
                                Ui.ComponentSample(
                                    heading = "Basic",
                                    visuals = (
                                        LC.Executor.AlertErrors(
                                            ``with`` =
                                                (fun makeExecutor ->
                                                    element {
                                                        LC.Text "Press the button below to asynchronously fail an operation."
                                                        LC.Button(
                                                            label = "Fail Asynchronously",
                                                            state =
                                                                ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (fun _ ->
                                                                    let executor = makeExecutor "some key"
                                                                    executor.MaybeExecute failAsynchronously)
                                                                )
                                                        )
                                                    }
                                                )
                                        )
                                    ),
                                    code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text "")
                                )
                            }
                        )
                    )
                }
            )
        )



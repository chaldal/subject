[<AutoOpen>]
module AppEggShellGallery.Components.Content_With_DataFlowControl

open System
open LibLangFsharp
open Fable.React
open LibClient
open LibClient.Components

type Ui.Content.With with
    [<Component>]
    static member DataFlowControl () : ReactElement =
        let deferred = Hooks.useState (Deferred<unit>())

        Ui.ComponentContent (
            displayName = "DataFlowControl",
            isResponsive = false,
            samples = (
                element {
                    Ui.ComponentSampleGroup(
                        samples = (
                            element {
                                Ui.ComponentSample(
                                    heading = "Immediate Propagation",
                                    visuals = (
                                        LC.With.Now(
                                            updateFrequency = TimeSpan.FromSeconds(5),
                                            ``with`` =
                                                fun now ->
                                                    LC.With.DataFlowControl(
                                                        data = now,
                                                        dataFlowPolicy = LC.With.DataFlowControlTypes.DataFlowPolicy.PropagateImmediately,
                                                        ``with`` =
                                                            fun data ->
                                                                element {
                                                                    LC.Text "Data:"
                                                                    LC.Timestamp (UniDateTime.Of data)
                                                                }
                                                    )
                                        )
                                    ),
                                    code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text "")
                                )

                                Ui.ComponentSample(
                                    heading = "Blocked Propagation",
                                    visuals = (
                                        LC.With.Now(
                                            updateFrequency = TimeSpan.FromSeconds(5),
                                            ``with`` =
                                                fun now ->
                                                    LC.With.DataFlowControl(
                                                        data = now,
                                                        dataFlowPolicy = LC.With.DataFlowControlTypes.DataFlowPolicy.Block,
                                                        ``with`` =
                                                            fun data ->
                                                                element {
                                                                    LC.Text "Data:"
                                                                    LC.Timestamp (UniDateTime.Of data)
                                                                }
                                                    )
                                        )
                                    ),
                                    code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text "")
                                )

                                Ui.ComponentSample(
                                    heading = "Confirmed Propagation",
                                    visuals = (
                                        LC.With.Now(
                                            updateFrequency = TimeSpan.FromSeconds(5),
                                            ``with`` =
                                                fun now ->
                                                    LC.With.DataFlowControl(
                                                        data = now,
                                                        dataFlowPolicy = LC.With.DataFlowControlTypes.DataFlowPolicy.PropagateWhenConfirmed ("Please confirm the propagation", "Confirm"),
                                                        ``with`` =
                                                            fun data ->
                                                                element {
                                                                    LC.Text "Data:"
                                                                    LC.Timestamp (UniDateTime.Of data)
                                                                }
                                                    )
                                        )
                                    ),
                                    code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text "")
                                )

                                Ui.ComponentSample(
                                    heading = "Resolved Propagation",
                                    visuals = (
                                        LC.With.Now(
                                            updateFrequency = TimeSpan.FromSeconds(5),
                                            ``with`` =
                                                fun now ->
                                                    element {
                                                        LC.With.DataFlowControl(
                                                            data = now,
                                                            dataFlowPolicy = LC.With.DataFlowControlTypes.DataFlowPolicy.PropagateWhenResolved deferred.current,
                                                            ``with`` =
                                                                fun data ->
                                                                    element {
                                                                        LC.Text "Data:"
                                                                        LC.Timestamp (UniDateTime.Of data)
                                                                    }
                                                        )

                                                        if deferred.current.IsPending then
                                                            LC.Button(
                                                                "Tap to resolve",
                                                                state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable (fun _ -> deferred.current.Resolve()))
                                                            )
                                                    }
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



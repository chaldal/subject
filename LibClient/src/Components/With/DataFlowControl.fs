[<AutoOpen>]
module LibClient.Components.With_DataFlowControl

open LibLangFsharp
open Fable.React
open ReactXP.Components
open ReactXP.Styles
open LibClient
open LibClient.Components

module LC =
    module With =
        // TODO: the "Types" suffix here was necessary to disambiguate between the module and the method, possibly because the method is generic (unclear)
        // would be good to investigate and clean up later.
        module DataFlowControlTypes =
            type DataFlowPolicy =
            | PropagateImmediately
            | Block
            | PropagateWhenConfirmed of Message: string * ButtonLabel: string
            | PropagateWhenResolved  of Deferred<unit>

open LC.With.DataFlowControlTypes

module private Styles =
    let confirmBlock =
        makeViewStyles {
            Position.Absolute
            top   0
            left  0
            right 0
            FlexDirection.Row
            JustifyContent.Center
            Overflow.VisibleForDropShadow
        }

    let confirmCard =
        makeViewStyles {
            FlexDirection.Row
            borderRadius 20
            backgroundColor Color.White
            paddingHV 16 8
            marginTop 10
            shadow (Color.BlackAlpha 0.3) 5 (0, 2)
        }

    let confirmMessage =
        makeTextStyles {
            marginRight 8
        }

type LibClient.Components.Constructors.LC.With with
    [<Component>]
    static member DataFlowControl<'T when 'T: equality> (data: 'T, dataFlowPolicy: DataFlowPolicy, ``with``: 'T -> ReactElement) : ReactElement =
        let latestPropagatedData = Hooks.useState data
        let latestReceivedData = Hooks.useState data

        Hooks.useEffect(
            dependencies = [| data; dataFlowPolicy |],
            effect =
                fun () ->
                    match dataFlowPolicy with
                    | PropagateWhenConfirmed _ | Block ->
                        latestReceivedData.update data
                    | PropagateImmediately ->
                        latestPropagatedData.update data
                        latestReceivedData.update data
                    | PropagateWhenResolved deferred ->
                        latestReceivedData.update data
                        async {
                            do! deferred.Value
                            latestPropagatedData.update latestReceivedData.current
                        } |> startSafely
        )

        element {
            ``with`` latestPropagatedData.current

            match dataFlowPolicy, latestReceivedData.current = latestPropagatedData.current with
            | (PropagateWhenConfirmed (message, buttonLabel), false) ->
                RX.View(
                    styles = [| Styles.confirmBlock |],
                    children =
                        elements {
                            RX.View(
                                styles = [| Styles.confirmCard |],
                                children =
                                    elements {
                                        LC.Text(
                                            value = message,
                                            styles = [| Styles.confirmMessage |]
                                        )
                                        LC.TextButton(
                                            label = buttonLabel,
                                            state =
                                                ButtonHighLevelState.LowLevel (
                                                    ButtonLowLevelState.Actionable (fun _ ->
                                                        latestPropagatedData.update latestReceivedData.current
                                                    )
                                                )
                                        )
                                    }
                            )
                        }
                )
            | _ ->
                noElement
        }

module LibClient.Components.AppShell.Context

open LibClient
open LibClient.Components
open Fable.React
open ReactXP.Components
open ReactXP.Styles

type Props = (* GenerateMakeFunction *) {

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    GlobalDataFlowControlData: Map<string, LibClient.Components.With.GlobalDataFlowControl.Context.DataFlowPolicyValue>
}

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
        }

type Context(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Context>("LibClient.Components.AppShell.Context", _initialProps, Actions, hasStyles = false)

    override this.GetInitialEstate (_initialProps: Props) : Estate = {
        GlobalDataFlowControlData = Map.empty
    }

    member this.UpdateGlobalDataFlowControlData (updater: Map<string, LibClient.Components.With.GlobalDataFlowControl.Context.DataFlowPolicyValue> -> Map<string, LibClient.Components.With.GlobalDataFlowControl.Context.DataFlowPolicyValue>) =
        this.SetEstate (fun estate _ -> { estate with GlobalDataFlowControlData = updater estate.GlobalDataFlowControlData })

    override this.Render () : ReactElement =
        let globalDataFlowControl: LibClient.Components.With.GlobalDataFlowControl.Context.Control = {
            Update = this.UpdateGlobalDataFlowControlData
            Data   = this.state.GlobalDataFlowControlData
        }

        let baseRenderResult = base.Render()

        LibClient.Components.With.GlobalDataFlowControl.Context.globalDataFlowControlContextProvider
            globalDataFlowControl
            [|
                RX.View(
                    styles = [| Styles.view |],
                    children =
                        elements {
                            LC.Executor.AlertErrors(
                                ``with`` =
                                    (fun makeExecutor -> (
                                        setGlobalExecutor makeExecutor
                                        globalExecutorContextProvider
                                            makeExecutor
                                            [|baseRenderResult|]
                                    )),
                                showTopLevelSpinnerForKeys = LC.Executor.ShowTopLevelSpinnerForKeys.All
                            )
                        }
                )
            |]

and Actions(_this: Context) =
    class end

let Make = makeConstructor<Context, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

[<AutoOpen>]
module LibClient.Components.With_GlobalDataFlowControl_Get

open Fable.React
open LibClient
open LibClient.Components

type LibClient.Components.Constructors.LC.With.GlobalDataFlowControl with
    [<Component>]
    static member Get(theKey: string, ``with``: LC.With.DataFlowControlTypes.DataFlowPolicy -> ReactElement) : ReactElement =
        LC.With.GlobalDataFlowControl.Context(
            fun control ->
                ``with`` (control.Get theKey)
        )
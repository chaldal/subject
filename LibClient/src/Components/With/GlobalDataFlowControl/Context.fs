[<AutoOpen>]
module LibClient.Components.With_GlobalDataFlowControl_Context

open Fable.React
open LibClient
open LibClient.Components.With.GlobalDataFlowControl.Context

type LibClient.Components.Constructors.LC.With.GlobalDataFlowControl with
    [<Component>]
    static member Context(``with``: Control -> ReactElement) : ReactElement =
        LC.With.Context(
            context = globalDataFlowControlContext,
            ``with`` = ``with``
        )
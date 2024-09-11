[<AutoOpen>]
module LibClient.Components.With_GlobalExecutor

open Fable.React
open LibClient
open LibClient.Components

type LC.With with
    [<Component>]
    static member GlobalExecutor (``with``: MakeExecutor -> ReactElement) : ReactElement =
        LC.With.Context(
            context = globalExecutorContext,
            ``with`` = ``with``
        )
[<AutoOpen>]
module LibClient.Components.With_Layout

open Fable.React
open LibClient
open LibClient.Components

type LC.With with
    [<Component>]
    static member Layout (``with``: ((* OnLayoutOption *) (Option<ReactXP.Types.ViewOnLayoutEvent -> unit>) * Option<LibClient.Output.Layout>) -> ReactElement, ?initialOnly: bool) : ReactElement =
        let initialOnly = defaultArg initialOnly false
        let valueState = Hooks.useState<Option<LibClient.Output.Layout>> None

        let onLayout (e: ReactXP.Types.ViewOnLayoutEvent) : unit =
            let newValue = {
                Width  = e.width
                Height = e.height
            }

            match (initialOnly, valueState.current) with
            | (false, None  )
            | (false, Some _)
            | (true,  None  ) ->
                valueState.update (Some newValue)
            | (true,  Some _) ->
                Noop

        let onLayoutOption =
            match (initialOnly, valueState.current) with
            | (true, Some _) -> None
            | _ -> Some onLayout

        ``with`` (onLayoutOption, valueState.current)

[<AutoOpen>]
module LibClient.Components.With_Context

open Fable.React
open LibClient
open LibClient.Components

type LC.With with
    [<Component>]
    static member Context (context: IContext<'T>, ``with``: 'T -> ReactElement, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>, ?xLegacyClassName: string) : ReactElement =
        xLegacyStyles |> ignore
        xLegacyClassName |> ignore

        let value = Hooks.useContext context
        Hooks.useMemo ((fun () -> ``with`` value), [|value; ``with``|])

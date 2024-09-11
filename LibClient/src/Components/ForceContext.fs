[<AutoOpen>]
module LibClient.Components.ForceContext

open Fable.React
open LibClient

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member ForceContext<'T>(context: IContext<'T>, value: 'T, children: ReactElements, ?key: string) : ReactElement =
        key |> ignore

        let contextProviderHook = Hooks.useRef (contextProvider context)
        contextProviderHook.current value children
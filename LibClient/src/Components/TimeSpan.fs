[<AutoOpen>]
module LibClient.Components.TimeSpan

open System
open Fable.React

open LibClient

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member TimeSpan(
            value: TimeSpan,
            ?shouldTruncateMillis: bool) : ReactElement =
        let shouldTruncateMillis = defaultArg shouldTruncateMillis true
        let value =
            if shouldTruncateMillis then
                value.TruncateMillis
            else
                value

        LC.Text(
            value.ToString()
        )

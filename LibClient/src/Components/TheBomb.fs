[<AutoOpen>]
module LibClient.Components.TheBomb

open Fable.React

open LibClient

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member TheBomb() : ReactElement =
        failwith "someone set up us the bomb!"
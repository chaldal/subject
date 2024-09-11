module LibClient.Components.TriStateful.Simple

open LibClient
open Fable.React

type Mode = LibClient.Components.TriStateful.Abstract.Mode

type Props = (* GenerateMakeFunction *) {
    Content: (* runAction *) (Async<Result<unit, string>> -> unit) -> ReactElement
}

type Simple(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Simple>("LibClient.Components.TriStateful.Simple", _initialProps, Actions, hasStyles = true)

and Actions(_this: Simple) =
    class end

let Make = makeConstructor<Simple, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

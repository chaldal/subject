module LibLifeCycleUi.Components.IndexQuery

open LibClient

type Props = (* GenerateMakeFunction *) {
    Something: int
}

type Estate = {
    SomeEphemeralValue: int
}

type IndexQuery(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, IndexQuery>("LibLifeCycleUi.Components.IndexQuery", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(initialProps: Props) = {
        SomeEphemeralValue = 42
    }

and Actions(_this: IndexQuery) =
    member _.DoSomething(value: string) : unit =
        Fable.Core.JS.console.log value

let Make = makeConstructor<IndexQuery, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

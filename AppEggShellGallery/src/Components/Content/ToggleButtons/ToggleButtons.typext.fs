module AppEggShellGallery.Components.Content.ToggleButtons

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Fruit =
| Mango
| Peach
| Banana

type Estate = {
    MaybeAtMostOneSelectedFruit:   Option<Fruit>
    MaybeExactlyOneSelectedFruit:  Option<Fruit>
    MaybeAtLeastOneSelectedFruits: Option<OrderedSet<Fruit>>
    MaybeAnySelectedFruits:        Option<OrderedSet<Fruit>>
}

type ToggleButtons(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, ToggleButtons>("AppEggShellGallery.Components.Content.ToggleButtons", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_: Props) : Estate = {
        MaybeAtMostOneSelectedFruit   = None
        MaybeExactlyOneSelectedFruit  = None
        MaybeAtLeastOneSelectedFruits = None
        MaybeAnySelectedFruits        = None
    }

and Actions(this: ToggleButtons) =
    member _.SetAtMostOneSelectedFruit (maybeFruit: Option<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAtMostOneSelectedFruit = maybeFruit })

    member _.SetExactlyOneSelectedFruit (fruit: Fruit) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeExactlyOneSelectedFruit = Some fruit })

    member _.SetAtLeastOneSelectedFruits (fruit: NonemptyOrderedSet<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAtLeastOneSelectedFruits = Some fruit.ToOrderedSet })

    member _.SetAnySelectedFruits (fruit: OrderedSet<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAnySelectedFruits = Some fruit })

let Make = makeConstructor<ToggleButtons, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

module AppEggShellGallery.Components.Content.Input.ChoiceList

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

type ChoiceList(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, ChoiceList>("AppEggShellGallery.Components.Content.Input.ChoiceList", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        MaybeAtMostOneSelectedFruit   = None
        MaybeExactlyOneSelectedFruit  = Some Mango
        MaybeAtLeastOneSelectedFruits = Some (OrderedSet.ofList [Mango;])
        MaybeAnySelectedFruits        = None
    }

and Actions(this: ChoiceList) =
    member _.SetAtMostOneSelectedFruit (maybeFruit: Option<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAtMostOneSelectedFruit = maybeFruit })

    member _.SetExactlyOneSelectedFruit (fruit: Fruit) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeExactlyOneSelectedFruit = Some fruit })

    member _.SetAtLeastOneSelectedFruits (fruit: NonemptyOrderedSet<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAtLeastOneSelectedFruits = Some fruit.ToOrderedSet })

    member _.SetAnySelectedFruits (fruit: OrderedSet<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAnySelectedFruits = Some fruit })

let Make = makeConstructor<ChoiceList, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

module AppEggShellGallery.Components.Content.Input.Picker

open AppEggShellGallery.Components.Content
open LibClient
open System

open LibClient.Components.Input.PickerModel

type Fruit =
| Apple
| Mango
| Banana
| Pear
with
    member this.GetName : NonemptyString =
        NonemptyString.ofLiteral (unionCaseName this)

let fruits: OrderedSet<Fruit> =
    [Apple; Mango; Banana; Pear]
    |> OrderedSet.ofList

let manyItems: OrderedSet<string> =
    "Lorem ipsum dolor sit amet consectetur adipiscing elit Sed iaculis neque nec ligula tempor aliquam eget vitae justo Sed vitae ex metus Vestibulum in turpis tempor rhoncus velit vel commodo turpis Integer aliquam vitae justo ac imperdiet Etiam eu lectus suscipit laoreet metus vitae volutpat elit Donec at mauris faucibus tristique enim non mattis turpis Donec eu pellentesque turpis ut vulputate nisi Quisque feugiat justo eu massa varius ullamcorper a in ex Ut auctor vulputate lorem quis ultricies erat porttitor ac Proin faucibus nibh at sapien efficitur non pellentesque est iaculis Duis imperdiet arcu sed elementum finibus Aliquam erat volutpat"
        .Split " "
    |> Array.toList
    |> OrderedSet.ofList

let fruitItemVisuals (fruit: Fruit) : PickerItemVisuals = {|
    Label = fruit.GetName.Value
|}

let fruitToFilterString (fruit: Fruit): string =
    fruit.GetName.Value

let fruitToFilterStringWithAdditionalText (fruit: Fruit) : string =
    [(Apple, "apel"); (Mango, "aam"); (Banana, "kola"); (Pear, "nashpati")]
    |> List.find (fun (item, _) -> item = fruit)
    |> fun (fruit: Fruit, searchText: string) -> sprintf "%s %s" fruit.GetName.Value searchText

let stringItemVisuals (item: string) : PickerItemVisuals = {|
    Label = item
|}

let fetchFruitsAllOnNoQuery (maybeQuery: Option<NonemptyString>) : Async<OrderedSet<Fruit>> =
    async {
        // imitate backend delay
        do! Async.Sleep (TimeSpan.FromMilliseconds 3000)
        let filteredFruit =
            match maybeQuery with
            | None -> fruits
            | Some query ->
                let queryLower = query.Value.ToLower()
                fruits |> OrderedSet.filter (fun fruit -> fruit.GetName.Value.ToLower().Contains queryLower)
        return filteredFruit
    }

let fetchFruitsEmptyOnNoQuery (maybeQuery: Option<NonemptyString>) : Async<OrderedSet<Fruit>> =
    async {
        // imitate backend delay
        do! Async.Sleep (TimeSpan.FromMilliseconds 3000)
        let filteredFruit =
            match maybeQuery with
            | None -> OrderedSet.empty
            | Some query ->
                let queryLower = query.Value.ToLower()
                fruits |> OrderedSet.filter (fun fruit -> fruit.GetName.Value.ToLower().Contains queryLower)
        return filteredFruit
    }

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    MaybeAtMostOneSelectedFruit:       Option<Fruit>
    MaybeExactlyOneSelectedFruit:      Option<Fruit>
    MaybeAtLeastOneSelectedFruits:     Option<OrderedSet<Fruit>>
    MaybeAnySelectedFruits:            Option<OrderedSet<Fruit>>
    MaybeSelectedFruitsWithSearchText: Option<OrderedSet<Fruit>>
    MaybeSelectedItems:                Option<OrderedSet<string>>
}

type Picker(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Picker>("AppEggShellGallery.Components.Content.Input.Picker", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_: Props) : Estate = {
        MaybeAtMostOneSelectedFruit       = None
        MaybeExactlyOneSelectedFruit      = None
        MaybeAtLeastOneSelectedFruits     = None
        MaybeAnySelectedFruits            = None
        MaybeSelectedFruitsWithSearchText = None
        MaybeSelectedItems                = None
    }

and Actions(this: Picker) =
    member _.SetAtMostOneSelectedFruit (maybeFruit: Option<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAtMostOneSelectedFruit = maybeFruit })

    member _.SetExactlyOneSelectedFruit (fruit: Fruit) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeExactlyOneSelectedFruit = Some fruit })

    member _.SetAtLeastOneSelectedFruits (fruit: NonemptyOrderedSet<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAtLeastOneSelectedFruits = Some fruit.ToOrderedSet })

    member _.SetAnySelectedFruits (fruit: OrderedSet<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeAnySelectedFruits = Some fruit })

    member _.SetSelectedFruitsWithSearchText (fruit: NonemptyOrderedSet<Fruit>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeSelectedFruitsWithSearchText = Some fruit.ToOrderedSet })

    member _.SetMaybeSelectedItems (maybeItem: OrderedSet<string>) : unit =
        this.SetEstate(fun estate _ -> { estate with MaybeSelectedItems = Some maybeItem })

let Make = makeConstructor<Picker, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

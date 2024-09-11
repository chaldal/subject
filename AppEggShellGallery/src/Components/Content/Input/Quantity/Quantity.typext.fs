module AppEggShellGallery.Components.Content.Input.Quantity

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, Option<PositiveInteger>>
}

type Quantity(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Quantity>("AppEggShellGallery.Components.Content.Input.Quantity", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", 1 |> PositiveInteger.ofLiteral |> Some)
            ("B", 1 |> PositiveInteger.ofLiteral |> Some)
            ("C", 5 |> PositiveInteger.ofLiteral |> Some)
            ("D", None)
            ("E", 6 |> PositiveInteger.ofLiteral |> Some)
        ]
    }

and Actions(this: Quantity) =
    member _.OnChangeOption (key: string) (value: Option<PositiveInteger>) (_e: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

    member _.OnChange (key: string) (value: PositiveInteger) (_e: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, Some value) })

let Make = makeConstructor<Quantity, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

module AppEggShellGallery.Components.Content.Input.Decimal

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LibClient.Components.Input.Decimal.Value>
}

type Decimal(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Decimal>("AppEggShellGallery.Components.Content.Input.Decimal", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", "3.14"    )
            ("B", "thirteen")
            ("C", "-13"     )
            ("D", ""        )
            ("E", "12"      )
            ("F", "11"      )
        ] |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LibClient.Components.Input.Decimal.parse)
    }

and Actions(this: Decimal) =
    member _.OnChange (key: string) (value: LibClient.Components.Input.Decimal.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<Decimal, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
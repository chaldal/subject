module AppEggShellGallery.Components.Content.Input.PositiveDecimal

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LibClient.Components.Input.PositiveDecimal.Value>
}

type PositiveDecimal(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, PositiveDecimal>("AppEggShellGallery.Components.Content.Input.PositiveDecimal", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", "3.14"    )
            ("B", "thirteen")
            ("C", "-13"     )
            ("D", ""        )
            ("E", "12"      )
            ("F", "11"      )
            ("G", "0"      )
        ] |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LibClient.Components.Input.PositiveDecimal.parse)
    }

and Actions(this: PositiveDecimal) =
    member _.OnChange (key: string) (value: LibClient.Components.Input.PositiveDecimal.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<PositiveDecimal, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

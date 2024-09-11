module AppEggShellGallery.Components.Content.Input.UnsignedDecimal

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LibClient.Components.Input.UnsignedDecimal.Value>
}

type UnsignedDecimal(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, UnsignedDecimal>("AppEggShellGallery.Components.Content.Input.UnsignedDecimal", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", "3.14"    )
            ("B", "thirteen")
            ("C", "-13"     )
            ("D", ""        )
            ("E", "12"      )
            ("F", "11"      )
        ] |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LibClient.Components.Input.UnsignedDecimal.parse)
    }

and Actions(this: UnsignedDecimal) =
    member _.OnChange (key: string) (value: LibClient.Components.Input.UnsignedDecimal.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<UnsignedDecimal, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
module AppEggShellGallery.Components.Content.Input.PositiveInteger

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LibClient.Components.Input.PositiveInteger.Value>
}

type PositiveInteger(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, PositiveInteger>("AppEggShellGallery.Components.Content.Input.PositiveInteger", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", "314"     )
            ("B", "thirteen")
            ("C", "-13"     )
            ("D", ""        )
            ("E", "12"      )
            ("F", "11"      )
        ] |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LibClient.Components.Input.PositiveInteger.parse)
    }

and Actions(this: PositiveInteger) =
    member _.OnChange (key: string) (value: LibClient.Components.Input.PositiveInteger.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<PositiveInteger, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
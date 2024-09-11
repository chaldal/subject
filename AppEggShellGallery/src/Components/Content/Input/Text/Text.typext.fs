module AppEggShellGallery.Components.Content.Input.Text

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, Option<NonemptyString>>
}

type Text(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Text>("AppEggShellGallery.Components.Content.Input.Text", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", "Momo"                )
            ("B", "Mango\nBanana\nApple")
            ("C", ""                    )
            ("D", "Table"               )
            ("E", "Chair"               )
            ("F1", ""                   )
            ("F2", "13.42"              )
            ("G1", ""                   )
            ("G2", "6"                  )
        ] |> Map.map (fun _ v -> NonemptyString.ofString v)
    }

and Actions(this: Text) =
    member _.OnChange (key: string) (value: Option<NonemptyString>) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<Text, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
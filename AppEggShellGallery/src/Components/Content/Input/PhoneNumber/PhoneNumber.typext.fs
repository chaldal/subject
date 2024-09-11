module AppEggShellGallery.Components.Content.Input.PhoneNumber

open LibClient
open LibClient.Components

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LC.Input.PhoneNumberTypes.Value>
}

type PhoneNumber(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, PhoneNumber>("AppEggShellGallery.Components.Content.Input.PhoneNumber", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", "+880-155-5575-868")
            ("B", "+1-555-9389-489"  )
            ("C", "-13"              )
            ("D", "abc"              )
            ("E", ""                 )
        ] |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LC.Input.PhoneNumberTypes.parse)
    }

and Actions(this: PhoneNumber) =
    member _.OnChange (key: string) (value: LC.Input.PhoneNumberTypes.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<PhoneNumber, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
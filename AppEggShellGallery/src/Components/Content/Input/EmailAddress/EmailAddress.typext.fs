module AppEggShellGallery.Components.Content.Input.EmailAddress

open LibClient
open LibClient.Components

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LC.Input.EmailAddressTypes.Value>
}

type EmailAddress(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, EmailAddress>("AppEggShellGallery.Components.Content.Input.EmailAddress", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", "someone@somewhere.com")
            ("B", "invalid"              )
            ("C", "also invalid"         )
            ("D", ""                     )
        ] |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LC.Input.EmailAddressTypes.parse)
    }

and Actions(this: EmailAddress) =
    member _.OnChange (key: string) (value: LC.Input.EmailAddressTypes.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<EmailAddress, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
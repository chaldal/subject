module AppEggShellGallery.Components.Content.Input.LocalTime

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LibClient.Components.Input.LocalTime.Value>
}

type LocalTime(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, LocalTime>("AppEggShellGallery.Components.Content.Input.LocalTime", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", LibClient.Components.Input.LocalTime.empty)
        ]
    }

and Actions(this: LocalTime) =
    member _.OnChange (key: string) (value: LibClient.Components.Input.LocalTime.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<LocalTime, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
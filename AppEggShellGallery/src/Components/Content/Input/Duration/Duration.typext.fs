module AppEggShellGallery.Components.Content.Input.Duration

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LibClient.Components.Input.Duration.Value>
}

type Duration(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Duration>("AppEggShellGallery.Components.Content.Input.Duration", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", LibClient.Components.Input.Duration.empty)
        ]
    }

and Actions(this: Duration) =
    member _.OnChange (key: string) (value: LibClient.Components.Input.Duration.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<Duration, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
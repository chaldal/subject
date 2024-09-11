module AppEggShellGallery.Components.Content.Scrim

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    IsScrimVisible: bool
}

type Scrim(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Scrim>("AppEggShellGallery.Components.Content.Scrim", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(initialProps: Props) : Estate = {
        IsScrimVisible = true
    }

and Actions(this: Scrim) =
    member _.Toggle (e: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ -> { estate with IsScrimVisible = not estate.IsScrimVisible })

let Make = makeConstructor<Scrim, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

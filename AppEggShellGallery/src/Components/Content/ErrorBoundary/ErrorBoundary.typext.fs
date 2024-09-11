module AppEggShellGallery.Components.Content.ErrorBoundary

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    ShowTheBomb: Set<string>
}

type ErrorBoundary(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, ErrorBoundary>("AppEggShellGallery.Components.Content.ErrorBoundary", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        ShowTheBomb = Set.empty
    }

and Actions(this: ErrorBoundary) =
    member _.SetTheBomb (key: string) (value: bool) (_e: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ -> { estate with ShowTheBomb = if value then estate.ShowTheBomb.Add key else estate.ShowTheBomb.Remove key })

let Make = makeConstructor<ErrorBoundary, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

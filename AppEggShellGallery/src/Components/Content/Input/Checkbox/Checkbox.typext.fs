module AppEggShellGallery.Components.Content.Input.Checkbox

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    A: Option<bool>
    B: Option<bool>
}

type Checkbox(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Checkbox>("AppEggShellGallery.Components.Content.Input.Checkbox", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        A = None
        B = Some true
    }

and Actions(this: Checkbox) =
    member _.OnChangeA (value: bool) : unit =
        this.SetEstate (fun estate _ -> { estate with A = Some value })

    member _.OnChangeB (value: bool) : unit =
        this.SetEstate (fun estate _ -> { estate with B = Some value })

let Make = makeConstructor<Checkbox, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

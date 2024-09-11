module AppEggShellGallery.Components.Content.Thumbs

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Selected: Set<int>
}

type Thumbs(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Thumbs>("AppEggShellGallery.Components.Content.Thumbs", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Selected = Set.ofList [2; 4]
    }

and Actions(this: Thumbs) =
    member _.OnPress (i: int) (_index: uint32) (_e: ReactEvent.Action) : unit =
        this.setState (fun estate _ -> { estate with Selected = estate.Selected.Toggle i })

let Make = makeConstructor<Thumbs, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

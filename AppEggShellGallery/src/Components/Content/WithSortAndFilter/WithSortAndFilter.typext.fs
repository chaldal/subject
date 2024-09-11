module AppEggShellGallery.Components.Content.WithSortAndFilter

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    SomeEphemeralValue: int
}

type WithSortAndFilter(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, WithSortAndFilter>("AppEggShellGallery.Components.Content.WithSortAndFilter", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        SomeEphemeralValue = 42
    }

and Actions(_this: WithSortAndFilter) =
    class end

let Make = makeConstructor<WithSortAndFilter, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

module AppEggShellGallery.Components.Content.Input.Image

open LibLifeCycleTypes.File
open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    MaybeFiles: Result<list<File>, string>
}

type Image(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Image>("AppEggShellGallery.Components.Content.Input.Image", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        MaybeFiles = Ok []
    }

and Actions(this: Image) =
    member _.OnChange (files: Result<list<File>, string>) : unit =
        this.SetEstate (fun estate _ ->
            { estate with MaybeFiles = files }
        )

let Make = makeConstructor<Image, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

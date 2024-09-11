module AppEggShellGallery.Components.Content.Input.File

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    MaybeFile: Result<list<LibLifeCycleTypes.File.File>, string>
}

type File(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, File>("AppEggShellGallery.Components.Content.Input.File", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        MaybeFile = Ok []
    }

and Actions(this: File) =
    member _.OnChange (fileData: Result<list<LibLifeCycleTypes.File.File>, string>) : unit =
        this.SetEstate (fun estate _ ->
            { estate with MaybeFile = fileData }
        )

let Make = makeConstructor<File, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

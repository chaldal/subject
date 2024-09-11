module AppEggShellGallery.Components.Content.AsyncData

open LibLang
open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = EmptyRecordType

type AsyncData(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, AsyncData>("AppEggShellGallery.Components.Content.AsyncData", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = EmptyRecord

and Actions(_this: AsyncData) =
    class end

let Make = makeConstructor<AsyncData, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

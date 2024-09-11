module AppEggShellGallery.Components.Route.Design

open LibLang
open LibClient
open AppEggShellGallery.Navigation

type Props = (* GenerateMakeFunction *) {
    PstoreKey: string
    Item:      DesignItem
}

type Estate = EmptyRecordType

type Pstate = {
    SomeValue: int
}


type Design(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Design>("AppEggShellGallery.Components.Route.Design", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = {
        SomeValue = 42
    }

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: Design) =
    class end

let Make = makeConstructor<Design,_,_>

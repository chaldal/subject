module AppEggShellGallery.Components.Route.HowTo

open LibLang
open LibClient
open AppEggShellGallery.Navigation

type Props = (* GenerateMakeFunction *) {
    PstoreKey: string
    Item:      HowToItem
}

type Estate = EmptyRecordType

type Pstate = {
    SomeValue: int
}


type HowTo(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, HowTo>("AppEggShellGallery.Components.Route.HowTo", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = {
        SomeValue = 42
    }

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: HowTo) =
    class end

let Make = makeConstructor<HowTo,_,_>

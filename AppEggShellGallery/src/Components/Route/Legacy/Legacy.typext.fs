module AppEggShellGallery.Components.Route.Legacy

open LibLang
open LibClient
open AppEggShellGallery.Navigation

type Props = (* GenerateMakeFunction *) {
    PstoreKey: string
    Item:      LegacyItem
}

type Estate = EmptyRecordType

type Pstate = {
    SomeValue: int
}


type Legacy(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Legacy>("AppEggShellGallery.Components.Route.Legacy", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = {
        SomeValue = 42
    }

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: Legacy) =
    class end

let Make = makeConstructor<Legacy,_,_>

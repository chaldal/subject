module AppEggShellGallery.Components.Route.Home

open LibClient.Responsive
open LibLang
open LibClient
open AppEggShellGallery.Navigation

type Props = (* GenerateMakeFunction *) {
    PstoreKey: string
}

type Estate = EmptyRecordType
type Pstate = EmptyRecordType

type Home(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Home>("AppEggShellGallery.Components.Route.Home", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: Home) =
    class end

let Make = makeConstructor<Home,_,_>

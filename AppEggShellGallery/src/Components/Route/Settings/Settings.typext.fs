module AppEggShellGallery.Components.Route.Settings

open LibLang
open LibClient

type Props = (* GenerateMakeFunction *) {
    PstoreKey: string
}
type Estate = EmptyRecordType
type Pstate = EmptyRecordType

type Settings(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Settings>("AppEggShellGallery.Components.Route.Settings", initialProps.PstoreKey, initialProps, NoActions, hasStyles = false)

    override _.GetDefaultPstate(_initialProps: Props) : Pstate = EmptyRecord

    override _.GetInitialEstate(_initialProps: Props) : Estate = EmptyRecord

and Actions = unit

let Make = makeConstructor<Settings,_,_>

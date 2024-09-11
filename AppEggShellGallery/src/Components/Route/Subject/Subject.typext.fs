module AppEggShellGallery.Components.Route.Subject

open LibLang
open LibClient

type Props = (* GenerateMakeFunction *) {
    PstoreKey:   string
    MarkdownUrl: string
}

type Estate = EmptyRecordType

type Pstate =  {
    SomeValue: int
}


type Subject(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Subject>("AppEggShellGallery.Components.Route.Subject", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = {
        SomeValue = 42
    }

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: Subject) =
    class end

let Make = makeConstructor<Subject,_,_>

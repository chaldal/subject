module AppEggShellGallery.Components.Route.Docs

open LibLang
open LibClient
open LibClient.Responsive

type Props = (* GenerateMakeFunction *) {
    PstoreKey:   string
    MarkdownUrl: string
}

type Estate = EmptyRecordType

type Pstate = {
    SomeValue: int
}


type Docs(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Docs>("AppEggShellGallery.Components.Route.Docs", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = {
        SomeValue = 42
    }

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: Docs) =
    class end

let Make = makeConstructor<Docs,_,_>

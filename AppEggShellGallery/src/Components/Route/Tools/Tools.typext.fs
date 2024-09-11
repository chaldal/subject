module AppEggShellGallery.Components.Route.Tools

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


type Tools(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Tools>("AppEggShellGallery.Components.Route.Tools", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = {
        SomeValue = 42
    }

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

and Actions(_this: Tools) =
    class end

let Make = makeConstructor<Tools,_,_>

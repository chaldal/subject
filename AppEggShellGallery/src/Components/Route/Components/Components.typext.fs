module AppEggShellGallery.Components.Route.Components

open LibLang
open LibClient
open LibClient.Responsive
open AppEggShellGallery.Navigation

type Props = (* GenerateMakeFunction *) {
    PstoreKey: string
    SampleVisualsScreenSize: ScreenSize
    Content:                 ComponentItem
}

type Estate = EmptyRecordType

type Pstate = {
    SomeValue: int
}


type Components(initialProps) =
    inherit PstatefulComponent<Props, Estate, Pstate, Actions, Components>("AppEggShellGallery.Components.Route.Components", initialProps.PstoreKey, initialProps, Actions, hasStyles = true)

    override _.GetDefaultPstate(_initialProps: Props) = {
        SomeValue = 42
    }

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

    override this.Render () : ReactElement =
        AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContextProvider this.props.SampleVisualsScreenSize [|(base.Render())|]

and Actions(_this: Components) =
    class end

let Make = makeConstructor<Components,_,_>

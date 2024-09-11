[<AutoOpen>]
module AppPerformancePlayground.Components.Route_Bananas

open LibClient
open LibClient.Components
open LibRouter.Components
open Fable.React

type Ui.Route with
    [<Component>]
    static member Bananas () : ReactElement = element {
        LR.Route (scroll = ScrollView.Vertical, children = [|
            LC.Section.Padded (elements {
                LC.Text "Bananas!"
            })
        |])
    }

and private Styles =
    class end

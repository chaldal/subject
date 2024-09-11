[<AutoOpen>]
module LibUiAdmin.Components.InfoMessageRoute

open Fable.React
open ReactXP.Styles
open LibClient
open LibClient.Components
open LibRouter.Components

type UiAdmin with
    [<Component>]
    static member InfoMessageRoute (message: string) : ReactElement =
        LR.Route ([|
            LC.Section.Padded (styles = [|Styles.Section|], children = [|
                LC.InfoMessage (
                    message = message,
                    level = InfoMessage.Info
                )
            |])
        |],
        scroll = Scroll.Vertical
        )

and private Styles() =
    static member val Section = makeViewStyles {
        paddingTop 100
    }
[<AutoOpen>]
module LibClient.Components.FlexFiller

open Fable.React

open LibClient
open ReactXP.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        makeViewStyles {
            flex 1
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member FlexFiller() : ReactElement =
        RX.View(
            styles = [| Styles.view |],
            children = [| |]
        )
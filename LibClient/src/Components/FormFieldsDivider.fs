[<AutoOpen>]
module LibClient.Components.FormFieldsDivider

open Fable.React
open LibClient
open ReactXP.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        makeViewStyles {
            borderTop    1 (Color.Grey "cc")
            borderStyle  BorderStyle.Dashed
            marginTop    10
            marginBottom 10
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member FormFieldsDivider() : ReactElement =
        RX.View(
            styles = [| Styles.view |],
            children = [||]
        )
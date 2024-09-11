[<AutoOpen>]
module LibClient.Components.Pre

open Fable.React

open LibClient

open ReactXP.Styles

[<RequireQualifiedAccess>]
module private Styles =
    let text =
        makeTextStyles {
            fontFamily "monospace"
            fontSize 12
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Pre(
            text: string,
            ?styles: array<TextStyles>,
            ?key: string
        ) : ReactElement =
        key |> ignore

        LC.Text(
            value = text,
            selectable = true,
            numberOfLines = 1,
            styles =
                [|
                    Styles.text
                    yield! (styles |> Option.defaultValue [||])
                |]
        )
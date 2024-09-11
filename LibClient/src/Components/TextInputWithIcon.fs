[<AutoOpen>]
module LibClient.Components.TextInputWithIcon

open Fable.React

open LibClient

open ReactXP.Styles
open ReactXP.Components

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        makeViewStyles {
            FlexDirection.Row
            AlignItems.Center
        }

    let icon =
        makeViewStyles {
            flex        0
            marginRight 12
        }

    let textInput =
        makeViewStyles {
            flex    1
            padding 0
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member TextInputWithIcon(
            icon: int -> LibClient.Icons.Icon,
            ?iconSize: int,
            ?placeholder: string,
            ?placeholderTextColor: Color,
            ?onChangeText: string -> unit,
            ?styles: array<ViewStyles>
        ) : ReactElement =
        let iconSize = defaultArg iconSize 26

        RX.View(
            styles =
                [|
                    Styles.view
                    yield! (styles |> Option.defaultValue [||])
                |],
            children =
                elements {
                    RX.View(
                        styles = [| Styles.icon |],
                        children =
                            elements {
                                icon iconSize
                            }
                    )

                    RX.TextInput(
                        styles = [| Styles.textInput |],
                        ?placeholder = placeholder,
                        ?placeholderTextColor = (placeholderTextColor |> Option.map (fun c -> c.ToReactXPString)),
                        ?onChangeText = onChangeText
                    )
                }
        )
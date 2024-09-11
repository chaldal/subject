[<AutoOpen>]
module LibClient.Components.TwoWayScrollable

open Fable.React

open LibClient

open ReactXP.Styles
open ReactXP.Components

[<RequireQualifiedAccess>]
module private Styles =
    let horizontalScrollView =
        makeScrollViewStyles {
            FlexDirection.Column
            flex 1
            Overflow.Visible
        }

    let horizontalScrollViewContent =
        makeViewStyles {
            Position.Absolute
            top 0
            bottom 0
        }

    let verticalScrollView =
        makeScrollViewStyles {
            FlexDirection.Column
            flex 1
            Overflow.Visible
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member TwoWayScrollable(
            both: ReactElement,
            ?horizontalOnly: ReactElement,
            ?styles: array<ScrollViewStyles>
        ) : ReactElement =
        let horizontalOnly = defaultArg horizontalOnly noElement

        RX.ScrollView(
            horizontal = true,
            vertical = false,
            styles =
                [|
                    Styles.horizontalScrollView

                    yield! (styles |> Option.defaultValue [||])
                |],
            children =
                elements {
                    RX.View(
                        styles = [| Styles.horizontalScrollViewContent |],
                        children =
                            elements {
                                horizontalOnly

                                RX.ScrollView(
                                    horizontal = false,
                                    vertical = true,
                                    styles = [| Styles.verticalScrollView |],
                                    children =
                                        elements {
                                            both
                                        }
                                )
                            }
                    )
                }
        )

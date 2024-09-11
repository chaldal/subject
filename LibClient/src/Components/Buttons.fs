[<AutoOpen>]
module LibClient.Components.Buttons

open Fable.React

open LibClient

open ReactXP.Styles
open ReactXP.Components

type Align = HorizontalAlignment

let Center = Align.Center
let Left = Align.Left
let Right = Align.Right

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        ViewStyles.Memoize (fun gapValue ->
            makeViewStyles {
                gap gapValue
                FlexDirection.Row
                AlignItems.Center
            }
        )

    let alignLeft =
        makeViewStyles {
            JustifyContent.FlexStart
        }

    let alignRight =
        makeViewStyles {
            JustifyContent.FlexEnd
        }

    let alignCenter =
        makeViewStyles {
            JustifyContent.Center
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Buttons(children: ReactElements, ?align: Align, ?gap: int, ?styles: array<ViewStyles>) : ReactElement =
        let gap = defaultArg gap 0
        let align = defaultArg align Align.Center
        let styles = defaultArg styles Array.empty

        RX.View(
            children = children,
            styles =
                [|
                    yield Styles.view gap

                    yield
                        match align with
                        | Align.Left -> Styles.alignLeft
                        | Align.Right -> Styles.alignRight
                        | Align.Center -> Styles.alignCenter

                    yield! styles
                |]
        )


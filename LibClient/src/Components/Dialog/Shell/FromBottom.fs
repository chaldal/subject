[<AutoOpen>]
module LibClient.Components.Dialog_Shell_FromBottom

open Fable.React

open ReactXP.LegacyStyles.RulesRestricted
open ReactXP.Styles
open ReactXP.Components

open LibClient
open LibClient.Components.Dialog.Base


type CanClose = LibClient.Components.Dialog.Base.CanClose
let When      = CanClose.When
let Never     = CanClose.Never

type CloseAction = LibClient.Components.Dialog.Base.CloseAction
let OnEscape      = CloseAction.OnEscape
let OnBackground  = CloseAction.OnBackground
let OnCloseButton = CloseAction.OnCloseButton

module private Styles =
    let wrapper = makeViewStyles {
        Position.Absolute
        FlexDirection.ColumnReverseZindexHack
        trbl 0 0 0 0
    }

    let children = makeViewStyles {
        flex 1
    }

    let scrollViewWrapper = ViewStyles.Memoize (fun maybeMinHeight -> makeViewStyles {
        flex 1
        JustifyContent.FlexEnd

        match maybeMinHeight with
        | Some minimumHeight -> minHeight minimumHeight
        | None -> ()
    })

    let scrollViewChildren = makeViewStyles {
        flex                 1
        backgroundColor      Color.White
        borderTopLeftRadius  12
        borderTopRightRadius 12
        Overflow.Hidden
    }

    let heading = makeTextStyles {
        flex   0
        margin 20
        AlignSelf.Center
    }

type LibClient.Components.Constructors.LC.Dialog.Shell with
    [<Component>]
    static member FromBottom (canClose: CanClose, children: ReactElements, ?heading: string, ?bottomSection: ReactElement) : ReactElement =
        let bottomSection = defaultArg bottomSection nothing

        LC.Dialog.Base (
            contentPosition = ContentPosition.Free,
            canClose = canClose,
            children = [|
                RX.View (styles = [|Styles.wrapper|], children = [|
                    // Reversed to make drop shadow work
                    bottomSection

                    LC.With.Layout (fun (onLayoutOption, maybeLayout) -> element {
                        let maybeMinHeight =
                            maybeLayout
                            |> Option.map (fun layout -> layout.Height)

                        RX.ScrollView (
                            ?onLayout = onLayoutOption,
                            vertical = true,
                            children = [|
                                RX.View (
                                    styles = [| Styles.scrollViewWrapper maybeMinHeight |],
                                    children = [|
                                        RX.View (children = [|
                                            RX.View (styles = [|Styles.scrollViewChildren|], children = elements {
                                                match heading with
                                                | None -> nothing
                                                | Some headingText ->
                                                    LC.Heading (
                                                        styles = [| Styles.heading |],
                                                        children = [|
                                                            LC.UiText headingText
                                                        |]
                                                    )

                                                children
                                            })
                                        |])
                                    |]
                                )
                            |]
                        )
                    })
                |])
            |]
        )
[<AutoOpen>]
module LibClient.Components.Dialog_Shell_FromRight

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
        right 0
        top 0
        heightPercent 100
        backgroundColor Color.White
        widthPercent 40
        minWidth 500
    }

    let children = makeViewStyles {
        flex 1
    }

    let scrollViewStyle = makeScrollViewStyles {
        // heightPercent 100
        backgroundColor Color.White
        // widthPercent 50
        // minWidth 300
    }

    let scrollViewWrapper = makeViewStyles {
        flex 1
        JustifyContent.FlexEnd
    }

    let scrollViewChildren = makeViewStyles {
        flex                 1
        backgroundColor      Color.White
        Overflow.Hidden
    }

    let heading = makeTextStyles {
        flex   0
        margin 20
        AlignSelf.Center
    }

type LibClient.Components.Constructors.LC.Dialog.Shell with
    [<Component>]
    static member FromRight (canClose: CanClose, children: ReactElements, ?heading: string, ?bottomSection: ReactElement) : ReactElement =
        let bottomSection = defaultArg bottomSection nothing

        LC.Dialog.Base (
            contentPosition = ContentPosition.Free,
            canClose = canClose,
            children = [|
                RX.View (styles = [|Styles.wrapper|], children = [|
                    // Reversed to make drop shadow work
                    bottomSection

                    RX.ScrollView (
                        vertical = true,
                        children = [|
                            RX.View (
                                styles = [| Styles.scrollViewWrapper |],
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
                |])
            |]
        )
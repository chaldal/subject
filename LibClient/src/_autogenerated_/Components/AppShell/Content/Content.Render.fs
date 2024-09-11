module LibClient.Components.AppShell.ContentRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibClient.Components

open LibClient
open LibClient.RenderHelpers
open LibClient.Icons
open LibClient.Chars
open LibClient.ColorModule
open LibClient.LocalImages
open LibClient.Responsive

open LibClient.Components.AppShell.Content
open ReactXP.LegacyStyles


let render(children: array<ReactElement>, props: LibClient.Components.AppShell.Content.Props, estate: LibClient.Components.AppShell.Content.Estate, pstate: LibClient.Components.AppShell.Content.Pstate, actions: LibClient.Components.AppShell.Content.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.ErrorBoundary"
    LibClient.Components.Constructors.LC.ErrorBoundary(
        catch = (actions.OnError),
        ``try`` =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "LibClient.Components.Responsive"
                    LibClient.Components.Constructors.LC.Responsive(
                        desktop =
                            (fun (_) ->
                                    (castAsElementAckingKeysWarning [|
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = "safe-insets-view"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            useSafeInsets = (true),
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                            children =
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "view"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                (
                                                                    if (not (props.TopStatus = noElement)) then
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "top-status-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    props.TopStatus
                                                                                |]
                                                                        )
                                                                    else noElement
                                                                )
                                                                (
                                                                    if (not (props.TopNav = noElement)) then
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "top-nav-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    props.TopNav
                                                                                |]
                                                                        )
                                                                    else noElement
                                                                )
                                                                match (props.DesktopSidebarStyle) with
                                                                | DesktopSidebarStyle.Fixed ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "sidebar-and-content-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "sidebar-block desktop"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                props.Sidebar
                                                                                            |]
                                                                                    )
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "content-block"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                props.Content
                                                                                            |]
                                                                                    )
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "top-nav-shadow"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |]
                                                                | DesktopSidebarStyle.Popup ->
                                                                    [|
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "sidebar-and-content-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "content-block"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                        children =
                                                                                            [|
                                                                                                props.Content
                                                                                            |]
                                                                                    )
                                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                                    let __currClass = "top-nav-shadow"
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    ReactXP.Components.Constructors.RX.View(
                                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                                    )
                                                                                |]
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Popup"
                                                                        LibClient.Components.Constructors.LC.Popup(
                                                                            connector = (estate.SidebarPopupConnector),
                                                                            render =
                                                                                (fun (_) ->
                                                                                        (castAsElementAckingKeysWarning [|
                                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                                            let __currClass = "sidebar-popup-wrapper"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                                children =
                                                                                                    [|
                                                                                                        props.Sidebar
                                                                                                    |]
                                                                                            )
                                                                                        |])
                                                                                )
                                                                        )
                                                                    |]
                                                                |> castAsElementAckingKeysWarning
                                                                (
                                                                    if (not (props.BottomNav = noElement)) then
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "bottom-nav-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    props.BottomNav
                                                                                |]
                                                                        )
                                                                    else noElement
                                                                )
                                                            |]
                                                    )
                                                    props.Dialogs
                                                |]
                                        )
                                    |])
                            ),
                        handheld =
                            (fun (_) ->
                                    (castAsElementAckingKeysWarning [|
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = "safe-insets-view"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            useSafeInsets = (true),
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                            children =
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "view"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                (
                                                                    if (not (props.TopStatus = noElement)) then
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "top-status-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    props.TopStatus
                                                                                |]
                                                                        )
                                                                    else noElement
                                                                )
                                                                (
                                                                    if (not (props.TopNav = noElement)) then
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "top-nav-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    props.TopNav
                                                                                |]
                                                                        )
                                                                    else noElement
                                                                )
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "content-block"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                    children =
                                                                        [|
                                                                            props.Content
                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                            let __currClass = "top-nav-shadow"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                            )
                                                                        |]
                                                                )
                                                                (
                                                                    if (not (props.BottomNav = noElement)) then
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "bottom-nav-block"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    props.BottomNav
                                                                                |]
                                                                        )
                                                                    else noElement
                                                                )
                                                                (
                                                                    if (not (props.Sidebar = noElement)) then
                                                                        let __parentFQN = Some "LibClient.Components.With.Layout"
                                                                        LibClient.Components.Constructors.LC.With.Layout(
                                                                            ``with`` =
                                                                                (fun (onLayoutOption, maybeLayout) ->
                                                                                        (castAsElementAckingKeysWarning [|
                                                                                            let __parentFQN = Some "LibClient.Components.Scrim"
                                                                                            LibClient.Components.Constructors.LC.Scrim(
                                                                                                ?onPanHorizontal = (estate.MaybeSidebarDraggable |> Option.map (fun draggable -> draggable.OnPanHorizontal)),
                                                                                                onPress = (setSidebarVisibility false),
                                                                                                isVisible = (estate.IsSidebarScrimVisible),
                                                                                                styles = ([| ContentStyles.Styles.scrim |])
                                                                                            )
                                                                                            (
                                                                                                let width = maybeLayout |> Option.map (fun l -> l.Width) |> Option.getOrElse 300
                                                                                                let __parentFQN = Some "LibClient.Components.Draggable"
                                                                                                let __currClass = "sidebar-draggable"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                let __dynamicStyles: List<ReactXP.LegacyStyles.RuleFunctionReturnedStyleRules> = [ReactXP.LegacyStyles.RulesBasic.width width]
                                                                                                let __currStyles = __currStyles @ (ReactXP.LegacyStyles.Designtime.processDynamicStyles __dynamicStyles)
                                                                                                LibClient.Components.Constructors.LC.Draggable(
                                                                                                    ref = (actions.Bound.RefSidebarDraggable),
                                                                                                    onChange = (actions.OnSidebarDraggableChange),
                                                                                                    baseOffset = ((-width + 10, 0)),
                                                                                                    right = ({| ForwardThreshold = 30; Offset = width - 10; BackwardThreshold = 50 |}),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                                                                            let __currClass = "sidebar-wrapper"
                                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                            ReactXP.Components.Constructors.RX.View(
                                                                                                                ?onLayout = (onLayoutOption),
                                                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                                                children =
                                                                                                                    [|
                                                                                                                        props.Sidebar
                                                                                                                    |]
                                                                                                            )
                                                                                                        |]
                                                                                                )
                                                                                            )
                                                                                        |])
                                                                                )
                                                                        )
                                                                    else noElement
                                                                )
                                                                props.Dialogs
                                                            |]
                                                    )
                                                |]
                                        )
                                    |])
                            )
                    )
                |])
    )

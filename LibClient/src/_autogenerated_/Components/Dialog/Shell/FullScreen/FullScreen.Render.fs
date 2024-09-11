module LibClient.Components.Dialog.Shell.FullScreenRender

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

open LibClient.Components.Dialog.Shell.FullScreen
open ReactXP.LegacyStyles


let render(children: array<ReactElement>, props: LibClient.Components.Dialog.Shell.FullScreen.Props, estate: LibClient.Components.Dialog.Shell.FullScreen.Estate, pstate: LibClient.Components.Dialog.Shell.FullScreen.Pstate, actions: LibClient.Components.Dialog.Shell.FullScreen.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Dialog.Base"
    LibClient.Components.Constructors.LC.Dialog.Base(
        canClose = (LibClient.Components.Dialog.Base.Never),
        contentPosition = (LibClient.Components.Dialog.Base.Free),
        children =
            [|
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "wrapper"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            (*  Reversed to make drop shadow of TopNav work  *)
                            (
                                (props.BottomSection)
                                |> Option.map
                                    (fun bottomSection ->
                                        (castAsElementAckingKeysWarning [|
                                            bottomSection
                                        |])
                                    )
                                |> Option.getOrElse noElement
                            )
                            let __parentFQN = Some "LibClient.Components.With.Layout"
                            LibClient.Components.Constructors.LC.With.Layout(
                                ``with`` =
                                    (fun (onLayoutOption, maybeLayout) ->
                                            (castAsElementAckingKeysWarning [|
                                                (
                                                    if (not (props.Scroll = NoScroll)) then
                                                        let __parentFQN = Some "ReactXP.Components.ScrollView"
                                                        ReactXP.Components.Constructors.RX.ScrollView(
                                                            vertical = (props.Scroll = Both || props.Scroll = Vertical),
                                                            horizontal = (props.Scroll = Both || props.Scroll = Horizontal),
                                                            ?onLayout = (onLayoutOption),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "scroll-view-children"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    let __dynamicStyles: List<ReactXP.LegacyStyles.RuleFunctionReturnedStyleRules> = maybeLayout |> Option.map (fun l -> [minHeight l.Height]) |> Option.getOrElse [height 0]
                                                                    let __currStyles = __currStyles @ (ReactXP.LegacyStyles.Designtime.processDynamicStyles __dynamicStyles)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        children = (children),
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                    )
                                                                |]
                                                        )
                                                    else noElement
                                                )
                                            |])
                                    )
                            )
                            (
                                if (props.Scroll = NoScroll) then
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "children"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                castAsElement children
                                            |]
                                    )
                                else noElement
                            )
                            (
                                if (props.Heading.IsSome || (match props.BackButton with | Yes _ -> true | _ -> false)) then
                                    let __parentFQN = Some "LibClient.Components.Legacy.TopNav.Base"
                                    let __currClass = "top-nav"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Legacy.TopNav.Base(
                                        center = (LibClient.Components.Legacy.TopNav.Base.Heading (props.Heading |> Option.getOrElse "")),
                                        left =
                                                (castAsElementAckingKeysWarning [|
                                                    match (props.BackButton.OnPressOption) with
                                                    | None ->
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.Legacy.TopNav.Filler"
                                                            LibClient.Components.Constructors.LC.Legacy.TopNav.Filler()
                                                        |]
                                                    | Some onPress ->
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.Legacy.TopNav.IconButton"
                                                            LibClient.Components.Constructors.LC.Legacy.TopNav.IconButton(
                                                                state = (LC.Legacy.TopNav.IconButtonTypes.Actionable onPress),
                                                                icon = (Icon.Back),
                                                                theme = (FullScreenStyles.Styles.iconButtonTheme)
                                                            )
                                                        |]
                                                    |> castAsElementAckingKeysWarning
                                                |]),
                                        right =
                                                (castAsElementAckingKeysWarning [|
                                                    match (props.HeaderRight) with
                                                    | Some headerRight ->
                                                        [|
                                                            headerRight
                                                        |]
                                                    | None ->
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.Legacy.TopNav.Filler"
                                                            LibClient.Components.Constructors.LC.Legacy.TopNav.Filler()
                                                        |]
                                                    |> castAsElementAckingKeysWarning
                                                |]),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                else noElement
                            )
                        |]
                )
            |]
    )

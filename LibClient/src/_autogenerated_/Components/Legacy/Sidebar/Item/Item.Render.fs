module LibClient.Components.Legacy.Sidebar.ItemRender

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

open LibClient.Components.Legacy.Sidebar.Item



let render(children: array<ReactElement>, props: LibClient.Components.Legacy.Sidebar.Item.Props, estate: LibClient.Components.Legacy.Sidebar.Item.Estate, pstate: LibClient.Components.Legacy.Sidebar.Item.Pstate, actions: LibClient.Components.Legacy.Sidebar.Item.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "item ", (unionCaseName props.Value), " ", (TopLevelBlockClass), "")) + System.String.Format(" {0}", (if (props.IsSelected) then "selected" else ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        onPress = (actions.OnPress),
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "content"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            match (props.Value) with
                            | Primary (label, maybeIcon, right) ->
                                [|
                                    (
                                        (maybeIcon)
                                        |> Option.map
                                            (fun icon ->
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "icon icon-left"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            icon 22
                                                        |]
                                                )
                                            )
                                        |> Option.getOrElse noElement
                                    )
                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                    let __currClass = (System.String.Format("{0}{1}{2}", "text ", (unionCaseName props.Value), ""))
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                            |]
                                    )
                                    match (right) with
                                    | Some (Right.Count count) ->
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "count"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                        let __currClass = "count-text"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        LibClient.Components.Constructors.LC.LegacyUiText(
                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", count))
                                                                |]
                                                        )
                                                    |]
                                            )
                                        |]
                                    | Some (Right.Icon icon) ->
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "icon icon-right"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        icon 22
                                                    |]
                                            )
                                        |]
                                    | None ->
                                        [||]
                                    |> castAsElementAckingKeysWarning
                                |]
                            | Secondary label ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                    let __currClass = "text"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                            |]
                                    )
                                |]
                            |> castAsElementAckingKeysWarning
                        |]
                )
            |]
    )

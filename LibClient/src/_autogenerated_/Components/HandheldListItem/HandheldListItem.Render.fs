module LibClient.Components.HandheldListItemRender

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

open LibClient.Components.HandheldListItem



let render(children: array<ReactElement>, props: LibClient.Components.HandheldListItem.Props, estate: LibClient.Components.HandheldListItem.Estate, pstate: LibClient.Components.HandheldListItem.Pstate, actions: LibClient.Components.HandheldListItem.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = "view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?onPress = (match props.State with | Actionable onPress -> Some (fun e -> e.stopPropagation(); onPress e) | _ -> None),
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                (
                    (props.LeftIcon)
                    |> Option.map
                        (fun leftIcon ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "left-icon"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        leftIcon 20
                                    |]
                            )
                        )
                    |> Option.getOrElse noElement
                )
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "label"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            match (props.Label) with
                            | Children ->
                                [|
                                    castAsElement children
                                |]
                            | Text text ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                    let __currClass = "label-text"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.LegacyText(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", text))
                                            |]
                                    )
                                |]
                            |> castAsElementAckingKeysWarning
                        |]
                )
                (
                    (props.Right)
                    |> Option.map
                        (fun right ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "right"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        match (right) with
                                        | Number number ->
                                            [|
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "number"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", number))
                                                        |]
                                                )
                                            |]
                                        | Icon icon ->
                                            [|
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "right-icon"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            icon 32
                                                        |]
                                                )
                                            |]
                                        | NumberAndIcon (number, icon) ->
                                            [|
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "number"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", number))
                                                        |]
                                                )
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "right-icon"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            icon 32
                                                        |]
                                                )
                                            |]
                                        |> castAsElementAckingKeysWarning
                                    |]
                            )
                        )
                    |> Option.getOrElse noElement
                )
            |]
    )

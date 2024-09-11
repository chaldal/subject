module LibClient.Components.VerticallyScrollableRender

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

open LibClient.Components.VerticallyScrollable



let render(children: array<ReactElement>, props: LibClient.Components.VerticallyScrollable.Props, estate: LibClient.Components.VerticallyScrollable.Estate, pstate: LibClient.Components.VerticallyScrollable.Pstate, actions: LibClient.Components.VerticallyScrollable.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles =
            (
                let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                match props.styles with
                | Some styles ->
                    Array.append __currProcessedStyles styles |> Some
                | None -> Some __currProcessedStyles
            ),
        children =
            [|
                (
                    (props.FixedTop)
                    |> Option.map
                        (fun fixedTop ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "top"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        fixedTop
                                    |]
                            )
                        )
                    |> Option.getOrElse noElement
                )
                (
                    (props.ScrollableMiddle)
                    |> Option.map
                        (fun scrollableMiddle ->
                            let __parentFQN = Some "ReactXP.Components.ScrollView"
                            let __currClass = "middle"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.ScrollView(
                                vertical = (true),
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None),
                                children =
                                    [|
                                        scrollableMiddle
                                    |]
                            )
                        )
                    |> Option.getOrElse noElement
                )
                (
                    (props.FixedBottom)
                    |> Option.map
                        (fun fixedBottom ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "bottom"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        fixedBottom
                                    |]
                            )
                        )
                    |> Option.getOrElse noElement
                )
            |]
    )

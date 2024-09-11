module LibClient.Components.BadgeRender

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

open LibClient.Components.Badge



let render(children: array<ReactElement>, props: LibClient.Components.Badge.Props, estate: LibClient.Components.Badge.Estate, pstate: LibClient.Components.Badge.Pstate, actions: LibClient.Components.Badge.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    match (props.Badge) with
    | Badge.Count count ->
        [|
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
                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                        let __currClass = "text"
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
    | Badge.Text text ->
        [|
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
                        let __parentFQN = Some "LibClient.Components.LegacyUiText"
                        let __currClass = "text"
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        LibClient.Components.Constructors.LC.LegacyUiText(
                            numberOfLines = (1),
                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                            children =
                                [|
                                    makeTextNode2 __parentFQN (System.String.Format("{0}", text))
                                |]
                        )
                    |]
            )
        |]
    | Badge.NoContent ->
        [|
            let __parentFQN = Some "ReactXP.Components.View"
            let __currClass = (System.String.Format("{0}{1}{2}", "no-content ", (TopLevelBlockClass), ""))
            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
            ReactXP.Components.Constructors.RX.View(
                ?styles =
                    (
                        let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                        match props.styles with
                        | Some styles ->
                            Array.append __currProcessedStyles styles |> Some
                        | None -> Some __currProcessedStyles
                    )
            )
        |]
    |> castAsElementAckingKeysWarning

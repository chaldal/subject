module LibClient.Components.Input.DayOfTheWeekRender

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

open LibClient.Components.Input.DayOfTheWeek



let render(children: array<ReactElement>, props: LibClient.Components.Input.DayOfTheWeek.Props, estate: LibClient.Components.Input.DayOfTheWeek.Estate, pstate: LibClient.Components.Input.DayOfTheWeek.Pstate, actions: LibClient.Components.Input.DayOfTheWeek.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                match (props.Label) with
                | Some label ->
                    [|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = "label"
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                    let __currClass = "label-text" + System.String.Format(" {0}", (if (props.Validity.IsInvalid) then "invalid" else ""))
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.LegacyText(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                            |]
                                    )
                                |]
                        )
                    |]
                | None ->
                    [|
                        (
                            if (props.Validity = Missing) then
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "label invalid"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "label-text invalid"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN "Required"
                                                    |]
                                            )
                                        |]
                                )
                            else noElement
                        )
                    |]
                |> castAsElementAckingKeysWarning
                let __parentFQN = Some "ReactXP.Components.ScrollView"
                ReactXP.Components.Constructors.RX.ScrollView(
                    horizontal = (true),
                    children =
                        [|
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "days"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        (
                                            (days)
                                            |> Seq.map
                                                (fun day ->
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "day" + System.String.Format(" {0}", (if (props.Mode.IsSelected day) then "selected" else ""))
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                let __currClass = "day-label" + System.String.Format(" {0}", (if (props.Mode.IsSelected day) then "selected" else ""))
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.LegacyText(
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", day.ToString().Substring(0, 3)))
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.TapCapture"
                                                                LibClient.Components.Constructors.LC.TapCapture(
                                                                    onPress = (props.Mode.OnPress day)
                                                                )
                                                            |]
                                                    )
                                                )
                                            |> Array.ofSeq |> castAsElement
                                        )
                                    |]
                            )
                        |]
                )
                (
                    (props.Validity.InvalidReason)
                    |> Option.map
                        (fun reason ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            ReactXP.Components.Constructors.RX.View(
                                children =
                                    [|
                                        let __parentFQN = Some "LibClient.Components.LegacyText"
                                        let __currClass = "invalid-reason"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.LegacyText(
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                            children =
                                                [|
                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", reason))
                                                |]
                                        )
                                    |]
                            )
                        )
                    |> Option.getOrElse noElement
                )
            |]
    )

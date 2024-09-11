module LibClient.Components.DateSelectorRender

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

open LibClient.Components.DateSelector



let render(children: array<ReactElement>, props: LibClient.Components.DateSelector.Props, estate: LibClient.Components.DateSelector.Estate, pstate: LibClient.Components.DateSelector.Pstate, actions: LibClient.Components.DateSelector.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "header"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            match (props.MaybeSelected) with
                            | Some selectedDate ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.Timestamp"
                                    LibClient.Components.Constructors.LC.Timestamp(
                                        offset = (DateTimeExtensions.bdTzOffset),
                                        format = ("ddd, MMM dd"),
                                        value = (LibClient.Components.Timestamp.UniDateTime.Of selectedDate),
                                        styles = ([| DateSelectorStyles.Styles.headerText |])
                                    )
                                |]
                            | None ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                    let __currClass = "header-text"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN "Select Day"
                                            |]
                                    )
                                |]
                            |> castAsElementAckingKeysWarning
                        |]
                )
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "navigation-controls"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            let __parentFQN = Some "LibClient.Components.Timestamp"
                            LibClient.Components.Constructors.LC.Timestamp(
                                offset = (DateTimeExtensions.bdTzOffset),
                                format = ("MMMM yyyy"),
                                value = (LibClient.Components.Timestamp.UniDateTime.Of estate.CurrentMonthFirstDay),
                                styles = ([| DateSelectorStyles.Styles.navigationControlsText |])
                            )
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "arrow-container"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        let __parentFQN = Some "LibClient.Components.IconButton"
                                        LibClient.Components.Constructors.LC.IconButton(
                                            state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel (if isPreviousMonthOutsideSelectableRange estate.CurrentMonthFirstDay props.MinDate then LC.IconButton.Disabled else LC.IconButton.Actionable actions.PrevMonth)),
                                            icon = (Icon.ChevronLeft),
                                            theme = (DateSelectorStyles.Styles.iconButtonTheme)
                                        )
                                        let __parentFQN = Some "LibClient.Components.IconButton"
                                        LibClient.Components.Constructors.LC.IconButton(
                                            state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel (if isNextMonthOutsideSelectableRange estate.CurrentMonthFirstDay props.MaxDate then LC.IconButton.Disabled else LC.IconButton.Actionable actions.NextMonth)),
                                            icon = (Icon.ChevronRight),
                                            theme = (DateSelectorStyles.Styles.iconButtonTheme)
                                        )
                                    |]
                            )
                        |]
                )
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "weekday-headers-row"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            (
                                (["S"; "M"; "T"; "W"; "T"; "F"; "S"])
                                |> Seq.map
                                    (fun day ->
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = "weekday-header"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                            children =
                                                [|
                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                    let __currClass = "weekday-header-text"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                        children =
                                                            [|
                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", day))
                                                            |]
                                                    )
                                                |]
                                        )
                                    )
                                |> Array.ofSeq |> castAsElement
                            )
                        |]
                )
                (
                    (estate.Rows)
                    |> Seq.map
                        (fun row ->
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "row"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        (
                                            (row)
                                            |> Seq.map
                                                (fun day ->
                                                    (
                                                        let isOtherMonth = not (day.Month = estate.CurrentMonthFirstDay.Month)
                                                        (castAsElementAckingKeysWarning [|
                                                            (
                                                                if (not isOtherMonth) then
                                                                    (
                                                                        let canSelect = not (isOutsideSelectableRange day props.MinDate props.MaxDate) && canSelectDate day props.CanSelectDate
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "day" + System.String.Format(" {0}", (if (Some day = props.MaybeSelected) then "selected" else ""))
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            onPress = (fun _ -> if canSelect then props.OnChange day else Noop),
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    let __currClass = "day-text" + System.String.Format(" {0} {1}", (if (day = estate.CurrentDate) then "today" else ""), (if (not canSelect) then "disabled" else ""))
                                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", day.Day))
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                        )
                                                                    )
                                                                else noElement
                                                            )
                                                            (
                                                                if (isOtherMonth) then
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "day other-month"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                    )
                                                                else noElement
                                                            )
                                                        |])
                                                    )
                                                )
                                            |> Array.ofSeq |> castAsElement
                                        )
                                    |]
                            )
                        )
                    |> Array.ofSeq |> castAsElement
                )
            |]
    )

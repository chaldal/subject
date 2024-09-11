module LibClient.Components.CarouselRender

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

open LibClient.Components.Carousel



let render(children: array<ReactElement>, props: LibClient.Components.Carousel.Props, estate: LibClient.Components.Carousel.Estate, pstate: LibClient.Components.Carousel.Pstate, actions: LibClient.Components.Carousel.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.Layout"
    LibClient.Components.Constructors.LC.With.Layout(
        ``with`` =
            (fun (onLayoutOption, maybeLayout) ->
                    (castAsElementAckingKeysWarning [|
                        (
                            let currIndex = estate.CurrIndex
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?onLayout = (onLayoutOption),
                                onKeyPress = (actions.OnKeyPress),
                                autoFocus = (props.RequestFocusOnMount),
                                tabIndex = (props.TabIndex),
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
                                            ([(estate.MaybePrevIndex |> Option.getOrElse currIndex) .. (estate.MaybeNextIndex |> Option.getOrElse currIndex)])
                                            |> Seq.map
                                                (fun index ->
                                                    (
                                                        let maybeWidth = maybeLayout |> Option.map (fun l -> l.Width)
                                                        let __parentFQN = Some "ReactXP.Components.AnimatableView"
                                                        let __currClass = "slide"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        let __dynamicAniStyles: List<ReactXP.LegacyStyles.RuntimeStyles> = LibClient.Components.CarouselStyles.slide maybeWidth index currIndex
                                                        let __currStyles = __currStyles @ (ReactXP.LegacyStyles.Designtime.processDynamicAniStyles __dynamicAniStyles)
                                                        ReactXP.Components.Constructors.RX.AnimatableView(
                                                            key = (System.String.Format("{0}{1}{2}", "slide", (index), "")),
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.AnimatableView" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    props.Slide index
                                                                |]
                                                        )
                                                    )
                                                )
                                            |> Array.ofSeq |> castAsElement
                                        )
                                        (
                                            if (props.Count.Value > 1) then
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "side left"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                (
                                                                    (estate.MaybePrevIndex)
                                                                    |> Option.map
                                                                        (fun prevIndex ->
                                                                            let __parentFQN = Some "LibClient.Components.IconButton"
                                                                            let __currClass = "icon-button"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            LibClient.Components.Constructors.LC.IconButton(
                                                                                state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LibClient.Components.IconButton.Actionable (actions.GoToIndex prevIndex))),
                                                                                icon = (Icon.ChevronLeft),
                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                            )
                                                                        )
                                                                    |> Option.getOrElse noElement
                                                                )
                                                            |]
                                                    )
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "dots-container"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "dots"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                    children =
                                                                        [|
                                                                            (
                                                                                ({0 .. props.Count.Value - 1})
                                                                                |> Seq.map
                                                                                    (fun i ->
                                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                                        let __currClass = "dot" + System.String.Format(" {0}", (if (i = currIndex) then "current" else ""))
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        ReactXP.Components.Constructors.RX.View(
                                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                                        )
                                                                                    )
                                                                                |> Array.ofSeq |> castAsElement
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "side right"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                (
                                                                    (estate.MaybeNextIndex)
                                                                    |> Option.map
                                                                        (fun nextIndex ->
                                                                            let __parentFQN = Some "LibClient.Components.IconButton"
                                                                            let __currClass = "icon-button"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            LibClient.Components.Constructors.LC.IconButton(
                                                                                state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LibClient.Components.IconButton.Actionable (actions.GoToIndex nextIndex))),
                                                                                icon = (Icon.ChevronRight),
                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                            )
                                                                        )
                                                                    |> Option.getOrElse noElement
                                                                )
                                                            |]
                                                    )
                                                |])
                                            else noElement
                                        )
                                    |]
                            )
                        )
                    |])
            )
    )

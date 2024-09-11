module LibClient.Components.ImageCardRender

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

open LibClient.Components.ImageCard



let render(children: array<ReactElement>, props: LibClient.Components.ImageCard.Props, estate: LibClient.Components.ImageCard.Estate, pstate: LibClient.Components.ImageCard.Pstate, actions: LibClient.Components.ImageCard.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.Layout"
    LibClient.Components.Constructors.LC.With.Layout(
        initialOnly = (true),
        ``with`` =
            (fun (onLayoutOption, maybeLayout) ->
                    (castAsElementAckingKeysWarning [|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = (System.String.Format("{0}{1}{2}{3}{4}{5}{6}", "view corners-", (props.Corners), " style-", (props.Style), " ", (TopLevelBlockClass), ""))
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?onLayout = (onLayoutOption),
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
                                    let __parentFQN = Some "ReactXP.Components.Image"
                                    let __currClass = "image"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.Image(
                                        size = (ReactXP.Components.Image.FromParentLayout maybeLayout),
                                        resizeMode = (ReactXP.Components.Image.Cover),
                                        source = (props.Source),
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Image" __currStyles |> Some) else None)
                                    )
                                    (
                                        (props.Label)
                                        |> Option.map
                                            (fun label ->
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "label-block"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            (
                                                                if (label.UseScrim) then
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "scrim"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                    )
                                                                else noElement
                                                            )
                                                            match (label) with
                                                            | Text (text, _) ->
                                                                [|
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "label-text-wrapper"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                        children =
                                                                            [|
                                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                let __currClass = "label-text"
                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                    ?styles = (props.labelStyles),
                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                    children =
                                                                                        [|
                                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", text))
                                                                                        |]
                                                                                )
                                                                            |]
                                                                    )
                                                                |]
                                                            | Children _ ->
                                                                [|
                                                                    castAsElement children
                                                                |]
                                                            |> castAsElementAckingKeysWarning
                                                        |]
                                                )
                                            )
                                        |> Option.getOrElse noElement
                                    )
                                    (
                                        (props.OnPress)
                                        |> Option.map
                                            (fun onPress ->
                                                let __parentFQN = Some "LibClient.Components.TapCapture"
                                                LibClient.Components.Constructors.LC.TapCapture(
                                                    onPress = (onPress)
                                                )
                                            )
                                        |> Option.getOrElse noElement
                                    )
                                |]
                        )
                    |])
            )
    )

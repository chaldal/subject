module LibClient.Components.Legacy.TopNav.BaseRender

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

open LibClient.Components.Legacy.TopNav.Base



let render(children: array<ReactElement>, props: LibClient.Components.Legacy.TopNav.Base.Props, estate: LibClient.Components.Legacy.TopNav.Base.Estate, pstate: LibClient.Components.Legacy.TopNav.Base.Pstate, actions: LibClient.Components.Legacy.TopNav.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = "view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "left"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            (
                                if (not (props.Left = noElement)) then
                                    (castAsElementAckingKeysWarning [|
                                        props.Left
                                    |])
                                else noElement
                            )
                            (
                                if (props.Left = noElement) then
                                    let __parentFQN = Some "LibClient.Components.Legacy.TopNav.Filler"
                                    LibClient.Components.Constructors.LC.Legacy.TopNav.Filler()
                                else noElement
                            )
                        |]
                )
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "center"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            match (props.Center) with
                            | Children ->
                                [|
                                    castAsElement children
                                |]
                            | Heading text ->
                                [|
                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                    let __currClass = "heading"
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
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "right"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            (
                                if (not (props.Right = noElement)) then
                                    (castAsElementAckingKeysWarning [|
                                        props.Right
                                    |])
                                else noElement
                            )
                            (
                                if (props.Right = noElement) then
                                    let __parentFQN = Some "LibClient.Components.Legacy.TopNav.Filler"
                                    LibClient.Components.Constructors.LC.Legacy.TopNav.Filler()
                                else noElement
                            )
                        |]
                )
            |]
    )

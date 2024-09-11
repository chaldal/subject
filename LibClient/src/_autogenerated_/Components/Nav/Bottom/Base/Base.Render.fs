module LibClient.Components.Nav.Bottom.BaseRender

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

open LibClient.Components.Nav.Bottom.Base



let render(children: array<ReactElement>, props: LibClient.Components.Nav.Bottom.Base.Props, estate: LibClient.Components.Nav.Bottom.Base.Estate, pstate: LibClient.Components.Nav.Bottom.Base.Pstate, actions: LibClient.Components.Nav.Bottom.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Responsive"
    LibClient.Components.Constructors.LC.Responsive(
        desktop =
            (fun (screenSize: ScreenSize) ->
                    (castAsElementAckingKeysWarning [|
                        match (props.Desktop()) with
                        | value when isNoElementOrEmptyFragmentOrEmptyArray value ->
                            [|
                                noElement
                            |]
                        | value ->
                            [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "view ", (TopLevelBlockClass), " ", (screenSize.Class), ""))
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
                                            value
                                        |]
                                )
                            |]
                        |> castAsElementAckingKeysWarning
                    |])
            ),
        handheld =
            (fun (screenSize: ScreenSize) ->
                    (castAsElementAckingKeysWarning [|
                        match (props.Handheld()) with
                        | value when isNoElementOrEmptyFragmentOrEmptyArray value ->
                            [|
                                noElement
                            |]
                        | value ->
                            [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "view ", (TopLevelBlockClass), " ", (screenSize.Class), ""))
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            value
                                        |]
                                )
                            |]
                        |> castAsElementAckingKeysWarning
                    |])
            )
    )

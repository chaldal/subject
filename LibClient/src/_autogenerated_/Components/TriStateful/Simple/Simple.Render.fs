module LibClient.Components.TriStateful.SimpleRender

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

open LibClient.Components.TriStateful.Simple
open LibClient


let render(children: array<ReactElement>, props: LibClient.Components.TriStateful.Simple.Props, estate: LibClient.Components.TriStateful.Simple.Estate, pstate: LibClient.Components.TriStateful.Simple.Pstate, actions: LibClient.Components.TriStateful.Simple.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.TriStateful.Abstract"
    LibClient.Components.Constructors.LC.TriStateful.Abstract(
        content =
            (fun (mode, runAction, reset) ->
                    (castAsElementAckingKeysWarning [|
                        props.Content runAction
                        match (mode) with
                        | Mode.Initial ->
                            [||]
                            (*  nothing  *)
                        | Mode.InProgress ->
                            [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "shield"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.ActivityIndicator"
                                            ReactXP.Components.Constructors.RX.ActivityIndicator(
                                                size = (ReactXP.Components.ActivityIndicator.Size.Small),
                                                color = ("#aaaaaa")
                                            )
                                        |]
                                )
                            |]
                        | Mode.Error message ->
                            [|
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "shield error"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "message"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", message))
                                                    |]
                                            )
                                            let __parentFQN = Some "LibClient.Components.Button"
                                            LibClient.Components.Constructors.LC.Button(
                                                state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable reset)),
                                                label = ("Ok")
                                            )
                                        |]
                                )
                            |]
                        |> castAsElementAckingKeysWarning
                    |])
            )
    )

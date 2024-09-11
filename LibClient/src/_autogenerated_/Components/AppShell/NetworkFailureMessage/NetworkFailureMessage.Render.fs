module LibClient.Components.AppShell.NetworkFailureMessageRender

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

open LibClient.Components.AppShell.NetworkFailureMessage



let render(children: array<ReactElement>, props: LibClient.Components.AppShell.NetworkFailureMessage.Props, estate: LibClient.Components.AppShell.NetworkFailureMessage.Estate, pstate: LibClient.Components.AppShell.NetworkFailureMessage.Pstate, actions: LibClient.Components.AppShell.NetworkFailureMessage.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                let __parentFQN = Some "LibClient.Components.Heading"
                LibClient.Components.Constructors.LC.Heading(
                    children =
                        [|
                            makeTextNode2 __parentFQN "Network Unreachable!"
                        |]
                )
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "second-line"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            makeTextNode2 __parentFQN "It seems like you may have lost network connectivity, please check and try again."
                        |]
                )
            |]
    )

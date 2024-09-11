module LibClient.Components.InfoMessageRender

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

open LibClient.Components.InfoMessage



let render(children: array<ReactElement>, props: LibClient.Components.InfoMessage.Props, estate: LibClient.Components.InfoMessage.Estate, pstate: LibClient.Components.InfoMessage.Pstate, actions: LibClient.Components.InfoMessage.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                let __parentFQN = Some "LibClient.Components.LegacyText"
                let __currClass = (System.String.Format("{0}{1}{2}", "text ", (props.Level), ""))
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                LibClient.Components.Constructors.LC.LegacyText(
                    ?styles = (props.styles),
                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                    children =
                        [|
                            makeTextNode2 __parentFQN (System.String.Format("{0}", props.Message))
                        |]
                )
            |]
    )

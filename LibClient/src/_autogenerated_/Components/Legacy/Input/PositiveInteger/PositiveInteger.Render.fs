module LibClient.Components.Legacy.Input.PositiveIntegerRender

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

open LibClient.Components.Legacy.Input.PositiveInteger



let render(children: array<ReactElement>, props: LibClient.Components.Legacy.Input.PositiveInteger.Props, estate: LibClient.Components.Legacy.Input.PositiveInteger.Estate, pstate: LibClient.Components.Legacy.Input.PositiveInteger.Pstate, actions: LibClient.Components.Legacy.Input.PositiveInteger.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                let __parentFQN = Some "ReactXP.Components.TextInput"
                ReactXP.Components.Constructors.RX.TextInput(
                    ?onKeyPress = (props.OnKeyPress),
                    onChangeText = (actions.OnChange),
                    value = (estate.CurrInput),
                    ref = (actions.Bound.RefTextInput)
                )
            |]
    )

module LibClient.Components.DraggableRender

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

open LibClient.Components.Draggable



let render(children: array<ReactElement>, props: LibClient.Components.Draggable.Props, estate: LibClient.Components.Draggable.Estate, pstate: LibClient.Components.Draggable.Pstate, actions: LibClient.Components.Draggable.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let gestureView = (
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "ReactXP.Components.GestureView"
                    let __currClass = (System.String.Format("{0}{1}{2}", "gesture-view ", (TopLevelBlockClass), ""))
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.GestureView(
                        ?onPanVertical = (if props.Up.IsSome || props.Down.IsSome then Some actions.OnPanVertical else None),
                        ?onPanHorizontal = (if props.Left.IsSome || props.Right.IsSome then Some actions.OnPanHorizontal else None),
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.GestureView" __currStyles |> Some) else None),
                        children =
                            [|
                                castAsElement children
                            |]
                    )
                |])
        )
        contentsView gestureView props estate
    |])

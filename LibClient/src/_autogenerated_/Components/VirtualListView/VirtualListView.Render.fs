module LibClient.Components.VirtualListViewRender

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

open LibClient.Components.VirtualListView



let render(children: array<ReactElement>, props: LibClient.Components.VirtualListView.Props<'Item>, estate: LibClient.Components.VirtualListView.Estate<'Item>, pstate: LibClient.Components.VirtualListView.Pstate, actions: LibClient.Components.VirtualListView.Actions<'Item>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.VirtualListView"
    let __currClass = (System.String.Format("{0}", TopLevelBlockClass))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.VirtualListView(
        renderItem = (actions.Bound.Render),
        scrollEventThrottle = (10),
        itemList = (props.Items |> Seq.map VirtualListItem.toRX |> Array.ofSeq),
        ?onScroll = (match props.RestoreScroll with No -> None | _ -> Some actions.OnScroll),
        ref = (actions.Bound.RefVirtualListView),
        ?styles =
            (
                let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                match props.styles with
                | Some styles ->
                    Array.append __currProcessedStyles styles |> Some
                | None -> Some __currProcessedStyles
            )
    )

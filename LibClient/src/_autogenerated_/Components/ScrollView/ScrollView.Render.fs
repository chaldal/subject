module LibClient.Components.ScrollViewRender

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

open LibClient.Components.ScrollView



let render(children: array<ReactElement>, props: LibClient.Components.ScrollView.Props, estate: LibClient.Components.ScrollView.Estate, pstate: LibClient.Components.ScrollView.Pstate, actions: LibClient.Components.ScrollView.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.ScrollView"
    ReactXP.Components.Constructors.RX.ScrollView(
        ref = (actions.RefScrollView),
        showsVerticalScrollIndicator = (props.ShowsVerticalScrollIndicatorOnNative),
        showsHorizontalScrollIndicator = (props.ShowsHorizontalScrollIndicatorOnNative),
        ?onScroll = (match props.RestoreScroll with No -> props.OnScroll | _ -> Some actions.OnScroll),
        ?onLayout = (props.OnLayout),
        vertical = (props.Scroll = Both || props.Scroll = Vertical),
        horizontal = (props.Scroll = Both || props.Scroll = Horizontal),
        children =
            [|
                (*  Note, onContentSizeChange on the ScrollView doesn't actually work as
         expected, it's called once, and does not get called again when async
         content is loaded and the content height changes. So we instead use
         a wrapping div with onLayout, which works as expected.  *)
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "inner-div"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    children = (children),
                    onLayout = (actions.OnContentLayout),
                    ?styles =
                        (
                            let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                            match props.styles with
                            | Some styles ->
                                Array.append __currProcessedStyles styles |> Some
                            | None -> Some __currProcessedStyles
                        )
                )
            |]
    )

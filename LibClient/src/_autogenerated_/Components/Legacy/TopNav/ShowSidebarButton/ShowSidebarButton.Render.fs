module LibClient.Components.Legacy.TopNav.ShowSidebarButtonRender

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

open LibClient.Components.Legacy.TopNav.ShowSidebarButton



let render(children: array<ReactElement>, props: LibClient.Components.Legacy.TopNav.ShowSidebarButton.Props, estate: LibClient.Components.Legacy.TopNav.ShowSidebarButton.Estate, pstate: LibClient.Components.Legacy.TopNav.ShowSidebarButton.Pstate, actions: LibClient.Components.Legacy.TopNav.ShowSidebarButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Responsive"
    let button =
            (castAsElementAckingKeysWarning [|
                let __parentFQN = Some "LibClient.Components.Legacy.TopNav.IconButton"
                LibClient.Components.Constructors.LC.Legacy.TopNav.IconButton(
                    state = (LC.Legacy.TopNav.IconButtonTypes.Actionable (LibClient.Components.AppShell.Content.setSidebarVisibility true)),
                    icon = (Icon.Menu)
                )
            |])
    LibClient.Components.Constructors.LC.Responsive(
        desktop =
            (fun (_) ->
                    (castAsElementAckingKeysWarning [|
                        if props.OnlyOnHandheld then noElement else button
                    |])
            ),
        handheld =
            (fun (_) ->
                    (castAsElementAckingKeysWarning [|
                        button
                    |])
            )
    )

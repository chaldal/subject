module LibClient.Components.Sidebar.WithCloseRender

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

open LibClient.Components.Sidebar.WithClose



let render(children: array<ReactElement>, props: LibClient.Components.Sidebar.WithClose.Props, estate: LibClient.Components.Sidebar.WithClose.Estate, pstate: LibClient.Components.Sidebar.WithClose.Pstate, actions: LibClient.Components.Sidebar.WithClose.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    props.With (LibClient.Components.AppShell.Content.setSidebarVisibility false)

module LibClient.Components.Nav.Top.ShowSidebarButtonRender

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

open LibClient.Components.Nav.Top.ShowSidebarButton



let render(children: array<ReactElement>, props: LibClient.Components.Nav.Top.ShowSidebarButton.Props, estate: LibClient.Components.Nav.Top.ShowSidebarButton.Estate, pstate: LibClient.Components.Nav.Top.ShowSidebarButton.Pstate, actions: LibClient.Components.Nav.Top.ShowSidebarButton.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let menuIcon = match props.MenuIcon with Some icon -> icon | None -> Icon.Menu
        (castAsElementAckingKeysWarning [|
            match (props.Badge) with
            | Some badge ->
                [|
                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                    let __currClass = "topnav-item"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                        state = (LibClient.Components.Nav.Top.Item.Actionable (LibClient.Components.AppShell.Content.toggleSidebarVisibility)),
                        style = (LibClient.Components.Nav.Top.Item.Style.With(icon = menuIcon, badge = badge)),
                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                    )
                |]
            | None ->
                [|
                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                    let __currClass = "topnav-item"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                        state = (LibClient.Components.Nav.Top.Item.Actionable (LibClient.Components.AppShell.Content.setSidebarVisibility true)),
                        style = (LibClient.Components.Nav.Top.Item.iconOnly menuIcon),
                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                    )
                |]
            |> castAsElementAckingKeysWarning
        |])
    )

module AppEggShellGallery.Components.TopNavRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ThirdParty.Map.Components
open ReactXP.Components
open ThirdParty.Recharts.Components
open ThirdParty.Showdown.Components
open ThirdParty.SyntaxHighlighter.Components
open LibUiAdmin.Components
open AppEggShellGallery.Components

open LibLang
open LibClient
open LibClient.Services.Subscription
open LibClient.RenderHelpers
open LibClient.Chars
open LibClient.ColorModule
open LibClient.Responsive
open AppEggShellGallery.RenderHelpers
open AppEggShellGallery.Navigation
open AppEggShellGallery.LocalImages
open AppEggShellGallery.Icons
open AppEggShellGallery.AppServices
open AppEggShellGallery

open AppEggShellGallery.Components.TopNav



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.TopNav.Props, estate: AppEggShellGallery.Components.TopNav.Estate, pstate: AppEggShellGallery.Components.TopNav.Pstate, actions: AppEggShellGallery.Components.TopNav.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibRouter.Components.With.CurrentRoute"
    LibRouter.Components.Constructors.LR.With.CurrentRoute(
        spec = (routesSpec()),
        fn =
            (fun (maybeCurrentRoute) ->
                    (castAsElementAckingKeysWarning [|
                        (
                            let maybeCurrentActualRoute = nav.CurrentActualRoute maybeCurrentRoute
                            let __parentFQN = Some "LibClient.Components.Nav.Top.Base"
                            LibClient.Components.Constructors.LC.Nav.Top.Base(
                                desktop =
                                    (fun (_) ->
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "logo"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            let __parentFQN = Some "LibClient.Components.Icon"
                                                            let __currClass = "logo-icon"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            LibClient.Components.Constructors.LC.Icon(
                                                                icon = (Icon.EggShell),
                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                            )
                                                            let __parentFQN = Some "LibClient.Components.TapCapture"
                                                            LibClient.Components.Constructors.LC.TapCapture(
                                                                onPress = (nav.Go (maybeCurrentRoute, Home))
                                                            )
                                                        |]
                                                )
                                                match (maybeCurrentActualRoute) with
                                                | Some (Docs       _) ->
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                            text = ("EggShell — Docs")
                                                        )
                                                    |]
                                                | Some (Tools      _) ->
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                            text = ("EggShell — Tools")
                                                        )
                                                    |]
                                                | Some (HowTo      _) ->
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                            text = ("EggShell — How To")
                                                        )
                                                    |]
                                                | Some (Subject    _) ->
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                            text = ("EggShell — Subject")
                                                        )
                                                    |]
                                                | Some (Design     _) ->
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                            text = ("EggShell — Design")
                                                        )
                                                    |]
                                                | Some (Components _) ->
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                            text = ("EggShell — Components")
                                                        )
                                                    |]
                                                | _                   ->
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                        LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                            text = ("EggShell")
                                                        )
                                                    |]
                                                |> castAsElementAckingKeysWarning
                                                (
                                                    let sampleVisualsScreenSize = nav.CurrentSampleVisualsScreenSizeOrDefault maybeCurrentRoute
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = "sample-visuals-screen-size"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                let __currClass = "label"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                LibClient.Components.Constructors.LC.LegacyUiText(
                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN "Visuals"
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.ToggleButtons"
                                                                LibClient.Components.Constructors.LC.ToggleButtons(
                                                                    value = (LC.ToggleButtons.ExactlyOne (Some sampleVisualsScreenSize, (fun value -> nav.SetSampleVisualsScreenSize maybeCurrentRoute value; Telemetry.TrackEvent "TopNavScreenSizeToggleButtonPressed" ([("Value", value.ToString() |> box)] |> Map.ofList)))),
                                                                    buttons =
                                                                        (fun (group) ->
                                                                                (castAsElementAckingKeysWarning [|
                                                                                    let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                                    LibClient.Components.Constructors.LC.ToggleButton(
                                                                                        value = (ScreenSize.Desktop),
                                                                                        style = (LC.ToggleButton.Label "Desktop"),
                                                                                        group = (group),
                                                                                        position = (LC.ToggleButton.Position.First)
                                                                                    )
                                                                                    let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                                    LibClient.Components.Constructors.LC.ToggleButton(
                                                                                        value = (ScreenSize.Handheld),
                                                                                        style = (LC.ToggleButton.Label "Handheld"),
                                                                                        group = (group),
                                                                                        position = (LC.ToggleButton.Position.Last)
                                                                                    )
                                                                                |])
                                                                        )
                                                                )
                                                            |]
                                                    )
                                                )
                                                (
                                                    let go = nav.Go (maybeCurrentRoute, Docs "index.md")        
                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                        state = (match maybeCurrentActualRoute with Some (Docs _)       -> LibClient.Components.Nav.Top.Item.SelectedActionable go | _ -> LibClient.Components.Nav.Top.Item.Actionable go),
                                                        style = (LibClient.Components.Nav.Top.Item.labelOnly "Docs")
                                                    )
                                                )
                                                (
                                                    let go = nav.Go (maybeCurrentRoute, Tools "tools/index.md") 
                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                        state = (match maybeCurrentActualRoute with Some (Tools _)      -> LibClient.Components.Nav.Top.Item.SelectedActionable go | _ -> LibClient.Components.Nav.Top.Item.Actionable go),
                                                        style = (LibClient.Components.Nav.Top.Item.labelOnly "Tools")
                                                    )
                                                )
                                                (
                                                    let go = nav.Go (maybeCurrentRoute, Components Index)       
                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                        state = (match maybeCurrentActualRoute with Some (Components _) -> LibClient.Components.Nav.Top.Item.SelectedActionable go | _ -> LibClient.Components.Nav.Top.Item.Actionable go),
                                                        style = (LibClient.Components.Nav.Top.Item.labelOnly "Components")
                                                    )
                                                )
                                                (
                                                    let go = nav.Go (maybeCurrentRoute, HowTo (HowToItem.Markdown "how-to/index.md"))
                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                        state = (match maybeCurrentActualRoute with Some (HowTo _)      -> LibClient.Components.Nav.Top.Item.SelectedActionable go | _ -> LibClient.Components.Nav.Top.Item.Actionable go),
                                                        style = (LibClient.Components.Nav.Top.Item.labelOnly "How To")
                                                    )
                                                )
                                                (
                                                    let go = nav.Go (maybeCurrentRoute, Subject "subject/index.md")
                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                        state = (match maybeCurrentActualRoute with Some (Subject _)    -> LibClient.Components.Nav.Top.Item.SelectedActionable go | _ -> LibClient.Components.Nav.Top.Item.Actionable go),
                                                        style = (LibClient.Components.Nav.Top.Item.labelOnly "Subject")
                                                    )
                                                )
                                                (
                                                    let go = nav.Go (maybeCurrentRoute, Design (DesignItem.Markdown "design/index.md"))
                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                        state = (match maybeCurrentActualRoute with Some (Design _)     -> LibClient.Components.Nav.Top.Item.SelectedActionable go | _ -> LibClient.Components.Nav.Top.Item.Actionable go),
                                                        style = (LibClient.Components.Nav.Top.Item.labelOnly "Design")
                                                    )
                                                )
                                                (
                                                    let go = nav.Go (maybeCurrentRoute, Legacy (LegacyItem.Markdown "legacy/index.md"))
                                                    let __parentFQN = Some "LibClient.Components.Nav.Top.Item"
                                                    LibClient.Components.Constructors.LC.Nav.Top.Item(
                                                        state = (match maybeCurrentActualRoute with Some (Legacy _)     -> LibClient.Components.Nav.Top.Item.SelectedActionable go | _ -> LibClient.Components.Nav.Top.Item.Actionable go),
                                                        style = (LibClient.Components.Nav.Top.Item.labelOnly "Legacy")
                                                    )
                                                )
                                            |])
                                    ),
                                handheld =
                                    (fun (_) ->
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "LibRouter.Components.Nav.Top.BackButton"
                                                LibRouter.Components.Constructors.LR.Nav.Top.BackButton()
                                                let __parentFQN = Some "LibClient.Components.Nav.Top.Heading"
                                                LibClient.Components.Constructors.LC.Nav.Top.Heading(
                                                    text = ("EggShell")
                                                )
                                                let __parentFQN = Some "LibClient.Components.Nav.Top.ShowSidebarButton"
                                                LibClient.Components.Constructors.LC.Nav.Top.ShowSidebarButton()
                                            |])
                                    )
                            )
                        )
                    |])
            )
    )

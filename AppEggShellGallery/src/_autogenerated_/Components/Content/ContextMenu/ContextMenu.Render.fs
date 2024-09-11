module AppEggShellGallery.Components.Content.ContextMenuRender

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

open AppEggShellGallery.Components.Content.ContextMenu
open LibClient.ContextMenus


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ContextMenu.Props, estate: AppEggShellGallery.Components.Content.ContextMenu.Estate, pstate: AppEggShellGallery.Components.Content.ContextMenu.Pstate, actions: AppEggShellGallery.Components.Content.ContextMenu.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        isResponsive = (true),
        displayName = ("Context Menu"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "We support buttons, cautionary buttons, dividers, and headings.\n        You can add items conditionally or using a match statement."
                |]),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                    <LC.Button
                     Label='""Handheld Context Menu""'
                     State='^LowLevel (~Actionable (fun e -> ContextMenu.Open menuItems ScreenSize.Handheld e.MaybeSource NoopFn e))'
                     rt-let='
                         menuItems := [
                             Heading (sprintf ""Shopping Cart (%i)"" cart.ItemCount)
                             match politenessLevel with
                             | Polite   -> ContextMenuItem.Button (""Continue shopping, please"", ignore )
                             | Impolite -> ContextMenuItem.Button (""Continue shopping"",         ignore )
                             | Mean     -> ContextMenuItem.Button (""Buy more dammit!"",          ignore )
                             ContextMenuItem.Button (""Save Cart"", ignore )
                             if not cart.IsEmpty then ContextMenuItem.Button (""Checkout"", ignore )
                             Divider
                             ButtonCautionary (""Empty Cart"", ignore )
                         ]
                     '/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        children =
                                            [|
                                                (
                                                    let menuItems = (
                                                        [
                                                                                     Heading (sprintf "Shopping Cart (%i)" cart.ItemCount)
                                                                                     match politenessLevel with
                                                                                     | Polite   -> ContextMenuItem.Button ("Continue shopping, please", ignore )
                                                                                     | Impolite -> ContextMenuItem.Button ("Continue shopping",         ignore )
                                                                                     | Mean     -> ContextMenuItem.Button ("Buy more dammit!",          ignore )
                                                                                     ContextMenuItem.Button ("Save Cart", ignore )
                                                                                     if not cart.IsEmpty then ContextMenuItem.Button ("Checkout", ignore )
                                                                                     Divider
                                                                                     ButtonCautionary ("Empty Cart", ignore )
                                                                                 ]
                                                                             
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                    LibClient.Components.Constructors.LC.Button(
                                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (fun e -> ContextMenu.Open menuItems ScreenSize.Handheld e.MaybeSource NoopFn e))),
                                                        label = ("Handheld Context Menu")
                                                    )
                                                )
                                            |]
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                    <LC.Button
                     Label='""Desktop Context Menu""'
                     State='^LowLevel (~Actionable (fun e -> ContextMenu.Open menuItems ScreenSize.Desktop e.MaybeSource NoopFn e))'
                     rt-let='
                         menuItems := [
                             Heading (sprintf ""Shopping Cart (%i)"" cart.ItemCount)
                             match politenessLevel with
                             | Polite   -> ContextMenuItem.Button (""Continue shopping, please"", ignore )
                             | Impolite -> ContextMenuItem.Button (""Continue shopping"",         ignore )
                             | Mean     -> ContextMenuItem.Button (""Buy more dammit!"",          ignore )
                             ContextMenuItem.Button (""Save Cart"", ignore )
                             if not cart.IsEmpty then ContextMenuItem.Button (""Checkout"", ignore )
                             Divider
                             ButtonCautionary (""Empty Cart"", ignore )
                         ]
                     '/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Buttons"
                                    LibClient.Components.Constructors.LC.Buttons(
                                        children =
                                            [|
                                                (
                                                    let menuItems = (
                                                        [
                                                                                     Heading (sprintf "Shopping Cart (%i)" cart.ItemCount)
                                                                                     match politenessLevel with
                                                                                     | Polite   -> ContextMenuItem.Button ("Continue shopping, please", ignore )
                                                                                     | Impolite -> ContextMenuItem.Button ("Continue shopping",         ignore )
                                                                                     | Mean     -> ContextMenuItem.Button ("Buy more dammit!",          ignore )
                                                                                     ContextMenuItem.Button ("Save Cart", ignore )
                                                                                     if not cart.IsEmpty then ContextMenuItem.Button ("Checkout", ignore )
                                                                                     Divider
                                                                                     ButtonCautionary ("Empty Cart", ignore )
                                                                                 ]
                                                                             
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                    LibClient.Components.Constructors.LC.Button(
                                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (fun e -> ContextMenu.Open menuItems ScreenSize.Desktop e.MaybeSource NoopFn e))),
                                                        label = ("Desktop Context Menu")
                                                    )
                                                )
                                            |]
                                    )
                                |])
                    )
                |])
    )

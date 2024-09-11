module AppEggShellGallery.Components.Content.ErrorBoundaryRender

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

open AppEggShellGallery.Components.Content.ErrorBoundary



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ErrorBoundary.Props, estate: AppEggShellGallery.Components.Content.ErrorBoundary.Estate, pstate: AppEggShellGallery.Components.Content.ErrorBoundary.Pstate, actions: AppEggShellGallery.Components.Content.ErrorBoundary.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.ErrorBoundary"),
        displayName = ("ErrorBoundary"),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        heading = ("Without an error boundary (there is one at the top of the AppShell.Content)"),
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                <text>Press the button below to cause an error to be throw in the render method.</text>
                <LC.Button
                 Label='""The Bomb""'
                 State='^LowLevel (~Actionable (actions.SetTheBomb ""A"" true))'/>
                <LC.TheBomb rt-fs='true' rt-if='estate.ShowTheBomb.Contains ""A""'/>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                    LibClient.Components.Constructors.LC.LegacyText(
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN "Press the button below to cause an error to be throw in the render method."
                                            |]
                                    )
                                    let __parentFQN = Some "LibClient.Components.Button"
                                    LibClient.Components.Constructors.LC.Button(
                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (actions.SetTheBomb "A" true))),
                                        label = ("The Bomb")
                                    )
                                    (
                                        if (estate.ShowTheBomb.Contains "A") then
                                            let __parentFQN = Some "LibClient.Components.TheBomb"
                                            LibClient.Components.Constructors.LC.TheBomb()
                                        else noElement
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        heading = ("With an error boundary"),
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                <LC.ErrorBoundary rt-fs='true' rt-prop-children='Try'>
                    <text>This is the try content.</text>
                    <text>Press the button below to cause an error to be throw in the render method.</text>
                    <LC.Button
                     Label='""The Bomb""'
                     State='^LowLevel (~Actionable (actions.SetTheBomb ""B"" true))'/>
                    <LC.TheBomb rt-fs='true' rt-if='estate.ShowTheBomb.Contains ""B""'/>

                    <rt-prop name='Catch(error, retry)'>
                        <text>We caught an error!</text>
                        <text>{error}</text>
                        <LC.Button
                         Label='""Reset""'
                         Level='~Secondary'
                         State='^LowLevel (~Actionable (fun e -> actions.SetTheBomb ""B"" false e; retry ()))'/>
                    </rt-prop>
                </LC.ErrorBoundary>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.ErrorBoundary"
                                    LibClient.Components.Constructors.LC.ErrorBoundary(
                                        catch =
                                            (fun (error: System.Exception, retry) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.LegacyText"
                                                        LibClient.Components.Constructors.LC.LegacyText(
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN "We caught an error!"
                                                                |]
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.LegacyText"
                                                        LibClient.Components.Constructors.LC.LegacyText(
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", error))
                                                                |]
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.Button"
                                                        LibClient.Components.Constructors.LC.Button(
                                                            state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (fun e -> actions.SetTheBomb "B" false e; retry ()))),
                                                            level = (LibClient.Components.Button.Secondary),
                                                            label = ("Reset")
                                                        )
                                                    |])
                                            ),
                                        ``try`` =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                        children =
                                                            [|
                                                                makeTextNode2 __parentFQN "This is the try content."
                                                            |]
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                        children =
                                                            [|
                                                                makeTextNode2 __parentFQN "Press the button below to cause an error to be throw in the render method."
                                                            |]
                                                    )
                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                    LibClient.Components.Constructors.LC.Button(
                                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable (actions.SetTheBomb "B" true))),
                                                        label = ("The Bomb")
                                                    )
                                                    (
                                                        if (estate.ShowTheBomb.Contains "B") then
                                                            let __parentFQN = Some "LibClient.Components.TheBomb"
                                                            LibClient.Components.Constructors.LC.TheBomb()
                                                        else noElement
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )

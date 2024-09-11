module AppEggShellGallery.Components.Content.ToggleButtonsRender

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

open AppEggShellGallery.Components.Content.ToggleButtons



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ToggleButtons.Props, estate: AppEggShellGallery.Components.Content.ToggleButtons.Estate, pstate: AppEggShellGallery.Components.Content.ToggleButtons.Pstate, actions: AppEggShellGallery.Components.Content.ToggleButtons.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        displayName = ("ToggleButtons"),
        props =
            (
                AppEggShellGallery.Components.ComponentContent.Manual
                    (
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.ToggleButtons"),
                                    heading = ("ToggleButtons")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.ToggleButton"),
                                    heading = ("ToggleButton")
                                )
                            |])
                    )
            ),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Basics"),
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
                    <LC.ToggleButtons rt-fs='true'
                     Value='LC.ToggleButtons.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)'
                     rt-prop-children='Buttons(group)'>
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Label ""Mango""'  Value='Fruit.Mango' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Label ""Peach""'  Value='Fruit.Peach' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Label ""Banana""' Value='Fruit.Banana'/>
                    </LC.ToggleButtons>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ToggleButtons"
                                                    LibClient.Components.Constructors.LC.ToggleButtons(
                                                        value = (LC.ToggleButtons.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)),
                                                        buttons =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Mango),
                                                                            style = (LC.ToggleButton.Label "Mango"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Peach),
                                                                            style = (LC.ToggleButton.Label "Peach"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Banana),
                                                                            style = (LC.ToggleButton.Label "Banana"),
                                                                            group = (group)
                                                                        )
                                                                    |])
                                                            )
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
                    <LC.ToggleButtons rt-fs='true'
                     Value='LC.ToggleButtons.ExactlyOne (estate.MaybeExactlyOneSelectedFruit, actions.SetExactlyOneSelectedFruit)'
                     rt-prop-children='Buttons(group)'>
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Icon Icon.Home'     Value='Fruit.Mango' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Icon Icon.Calendar' Value='Fruit.Peach' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Icon Icon.Menu'     Value='Fruit.Banana'/>
                    </LC.ToggleButtons>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ToggleButtons"
                                                    LibClient.Components.Constructors.LC.ToggleButtons(
                                                        value = (LC.ToggleButtons.ExactlyOne (estate.MaybeExactlyOneSelectedFruit, actions.SetExactlyOneSelectedFruit)),
                                                        buttons =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Mango),
                                                                            style = (LC.ToggleButton.Icon Icon.Home),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Peach),
                                                                            style = (LC.ToggleButton.Icon Icon.Calendar),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Banana),
                                                                            style = (LC.ToggleButton.Icon Icon.Menu),
                                                                            group = (group)
                                                                        )
                                                                    |])
                                                            )
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
                    <LC.ToggleButtons rt-fs='true'
                     Value='LC.ToggleButtons.AtLeastOne (estate.MaybeAtLeastOneSelectedFruits, actions.SetAtLeastOneSelectedFruits)'
                     rt-prop-children='Buttons(group)'>
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.LabelAndIcon (""Mango"", Icon.Home)'     Value='Fruit.Mango' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.LabelAndIcon (""Peach"", Icon.Calendar)' Value='Fruit.Peach' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.LabelAndIcon (""Banana"", Icon.Menu)'    Value='Fruit.Banana'/>
                    </LC.ToggleButtons>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ToggleButtons"
                                                    LibClient.Components.Constructors.LC.ToggleButtons(
                                                        value = (LC.ToggleButtons.AtLeastOne (estate.MaybeAtLeastOneSelectedFruits, actions.SetAtLeastOneSelectedFruits)),
                                                        buttons =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Mango),
                                                                            style = (LC.ToggleButton.LabelAndIcon ("Mango", Icon.Home)),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Peach),
                                                                            style = (LC.ToggleButton.LabelAndIcon ("Peach", Icon.Calendar)),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Banana),
                                                                            style = (LC.ToggleButton.LabelAndIcon ("Banana", Icon.Menu)),
                                                                            group = (group)
                                                                        )
                                                                    |])
                                                            )
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
                    <LC.ToggleButtons rt-fs='true'
                     Value='LC.ToggleButtons.Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)'
                     rt-prop-children='Buttons(group)'>
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Label ""Mango""'  Value='Fruit.Mango' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Label ""Peach""'  Value='Fruit.Peach' />
                        <LC.ToggleButton rt-fs='true' Group='group' Style='LC.ToggleButton.Label ""Banana""' Value='Fruit.Banana'/>
                    </LC.ToggleButtons>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ToggleButtons"
                                                    LibClient.Components.Constructors.LC.ToggleButtons(
                                                        value = (LC.ToggleButtons.Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)),
                                                        buttons =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Mango),
                                                                            style = (LC.ToggleButton.Label "Mango"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Peach),
                                                                            style = (LC.ToggleButton.Label "Peach"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.ToggleButton"
                                                                        LibClient.Components.Constructors.LC.ToggleButton(
                                                                            value = (Fruit.Banana),
                                                                            style = (LC.ToggleButton.Label "Banana"),
                                                                            group = (group)
                                                                        )
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )

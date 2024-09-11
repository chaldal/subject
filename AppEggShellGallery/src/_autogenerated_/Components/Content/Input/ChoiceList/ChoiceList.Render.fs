module AppEggShellGallery.Components.Content.Input.ChoiceListRender

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

open AppEggShellGallery.Components.Content.Input.ChoiceList



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.ChoiceList.Props, estate: AppEggShellGallery.Components.Content.Input.ChoiceList.Estate, pstate: AppEggShellGallery.Components.Content.Input.ChoiceList.Pstate, actions: AppEggShellGallery.Components.Content.Input.ChoiceList.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        displayName = ("Input.ChoiceList"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "For 'AtMostOne' and 'ExactlyOne', Radio Button is rendered, and for 'AtLeastOne' and 'Any' we render checkboxes."
                |]),
        props =
            (
                AppEggShellGallery.Components.ComponentContent.Manual
                    (
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Input.ChoiceList"),
                                    heading = ("ChoiceList")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("LibClient.Components.Input.ChoiceListItem"),
                                    heading = ("ChoiceListItem")
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
                    <LC.Input.ChoiceList
                     Value='~AtLeastOne (estate.MaybeAtLeastOneSelectedFruits, actions.SetAtLeastOneSelectedFruits)'
                     Validity='Valid'
                     rt-prop-children='Items(group)'>
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Mango""'  Value='Fruit.Mango' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Peach""'  Value='Fruit.Peach' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Banana""' Value='Fruit.Banana'/>
                    </LC.Input.ChoiceList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.ChoiceList"
                                                    LibClient.Components.Constructors.LC.Input.ChoiceList(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.ChoiceList.AtLeastOne (estate.MaybeAtLeastOneSelectedFruits, actions.SetAtLeastOneSelectedFruits)),
                                                        items =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Mango),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Mango"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Peach),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Peach"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Banana),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Banana"),
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
                    <LC.Input.ChoiceList
                     Value='~Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)'
                     Validity='Valid'
                     rt-prop-children='Items(group)'>
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Mango""'  Value='Fruit.Mango' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Peach""'  Value='Fruit.Peach' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Banana""' Value='Fruit.Banana'/>
                    </LC.Input.ChoiceList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.ChoiceList"
                                                    LibClient.Components.Constructors.LC.Input.ChoiceList(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.ChoiceList.Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)),
                                                        items =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Mango),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Mango"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Peach),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Peach"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Banana),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Banana"),
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
                    <LC.Input.ChoiceList
                     Value='~AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)'
                     Validity='Valid'
                     rt-prop-children='Items(group)'>
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Mango""'  Value='Fruit.Mango' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Peach""'  Value='Fruit.Peach' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Banana""' Value='Fruit.Banana'/>
                    </LC.Input.ChoiceList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.ChoiceList"
                                                    LibClient.Components.Constructors.LC.Input.ChoiceList(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.ChoiceList.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)),
                                                        items =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Mango),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Mango"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Peach),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Peach"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Banana),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Banana"),
                                                                            group = (group)
                                                                        )
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Children Based Label"),
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
                    <LC.Input.ChoiceList
                     Value='~ExactlyOne (estate.MaybeExactlyOneSelectedFruit, actions.SetExactlyOneSelectedFruit)'
                     Validity='Valid'
                     rt-prop-children='Items(group)'>
                        <LC.Input.ChoiceListItem Group='group' Value='Fruit.Mango'>
                            <uitext>Mango</uitext>
                        </LC.Input.ChoiceListItem>
                        <LC.Input.ChoiceListItem Group='group' Value='Fruit.Peach'>
                            <uitext>Peach</uitext>
                        </LC.Input.ChoiceListItem>
                        <LC.Input.ChoiceListItem Group='group' Value='Fruit.Banana'>
                            <uitext>Banana</uitext>
                        </LC.Input.ChoiceListItem>
                    </LC.Input.ChoiceList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.ChoiceList"
                                                    LibClient.Components.Constructors.LC.Input.ChoiceList(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.ChoiceList.ExactlyOne (estate.MaybeExactlyOneSelectedFruit, actions.SetExactlyOneSelectedFruit)),
                                                        items =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Mango),
                                                                            group = (group),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN "Mango"
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Peach),
                                                                            group = (group),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN "Peach"
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Banana),
                                                                            group = (group),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "LibClient.Components.LegacyUiText"
                                                                                    LibClient.Components.Constructors.LC.LegacyUiText(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN "Banana"
                                                                                            |]
                                                                                    )
                                                                                |]
                                                                        )
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Validation (Hardcoded, not dynamic)"),
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
                    <LC.Input.ChoiceList
                     Value='~AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)'
                     Validity='Invalid ""Not a fruit""'
                     rt-prop-children='Items(group)'>
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Mango""'  Value='Fruit.Mango' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Peach""'  Value='Fruit.Peach' />
                        <LC.Input.ChoiceListItem Group='group' Label='~String ""Banana""' Value='Fruit.Banana'/>
                    </LC.Input.ChoiceList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.ChoiceList"
                                                    LibClient.Components.Constructors.LC.Input.ChoiceList(
                                                        validity = (Invalid "Not a fruit"),
                                                        value = (LibClient.Components.Input.ChoiceList.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)),
                                                        items =
                                                            (fun (group) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Mango),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Mango"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Peach),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Peach"),
                                                                            group = (group)
                                                                        )
                                                                        let __parentFQN = Some "LibClient.Components.Input.ChoiceListItem"
                                                                        LibClient.Components.Constructors.LC.Input.ChoiceListItem(
                                                                            value = (Fruit.Banana),
                                                                            label = (LibClient.Components.Input.ChoiceListItem.String "Banana"),
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

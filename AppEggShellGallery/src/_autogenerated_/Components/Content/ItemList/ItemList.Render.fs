module AppEggShellGallery.Components.Content.ItemListRender

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

open AppEggShellGallery.Components.Content.ItemList



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ItemList.Props, estate: AppEggShellGallery.Components.Content.ItemList.Estate, pstate: AppEggShellGallery.Components.Content.ItemList.Pstate, actions: AppEggShellGallery.Components.Content.ItemList.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.ItemList"),
        isResponsive = (true),
        displayName = ("ItemList"),
        notes =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "LibClient.Components.LegacyText"
                    LibClient.Components.Constructors.LC.LegacyText(
                        children =
                            [|
                                makeTextNode2 __parentFQN "Never forget to have a proper message for empty lists by using this component."
                            |]
                    )
                    let __parentFQN = Some "LibClient.Components.LegacyText"
                    LibClient.Components.Constructors.LC.LegacyText(
                        children =
                            [|
                                makeTextNode2 __parentFQN "Base types and values used in the examples:"
                            |]
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.Code"
                    AppEggShellGallery.Components.Constructors.Ui.Code(
                        language = (AppEggShellGallery.Components.Code.Fsharp),
                        children =
                            [|
                                @"
            type Fruit = {
                Name:  string
                Color: Color
            }

            let fruits = [
                { Name = ""Mango"";    Color = Color.Hex ""#ff9000"" }
                { Name = ""Kiwi"";     Color = Color.Hex ""#1d6308"" }
                { Name = ""Raspbery""; Color = Color.Hex ""#b90041"" }
                { Name = ""Apple"";    Color = Color.Hex ""#76b902"" }
            ]
        "
                                |> makeTextNode2 __parentFQN
                            |]
                    )
                |]),
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
                    <LC.ItemList rt-fs='true' Items='fruits' Style='~Responsive HorizontalAlignment.Center' rt-prop-children='WhenNonempty(fruits)'>
                        <LC.Legacy.Card rt-map='fruit := fruits'>
                            {fruit.Name}
                        </LC.Legacy.Card>
                    </LC.ItemList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Responsive HorizontalAlignment.Center),
                                                        items = (fruits),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                    <LC.ItemList rt-fs='true' Items='fruits' Style='~Responsive HorizontalAlignment.Right' rt-prop-children='WhenNonempty(fruits)'>
                        <LC.Legacy.Card rt-map='fruit := fruits'>
                            {fruit.Name}
                        </LC.Legacy.Card>
                    </LC.ItemList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Responsive HorizontalAlignment.Right),
                                                        items = (fruits),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                    <LC.ItemList rt-fs='true' Items='fruits' Style='~Responsive HorizontalAlignment.Left' rt-prop-children='WhenNonempty(fruits)'>
                        <LC.Legacy.Card rt-map='fruit := fruits'>
                            {fruit.Name}
                        </LC.Legacy.Card>
                    </LC.ItemList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Responsive HorizontalAlignment.Left),
                                                        items = (fruits),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                    <LC.ItemList rt-fs='true' Items='fruits' Style='~Raw' rt-prop-children='WhenNonempty(fruits)'>
                        <LC.Legacy.Card rt-map='fruit := fruits'>
                            {fruit.Name}
                        </LC.Legacy.Card>
                    </LC.ItemList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Raw),
                                                        items = (fruits),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                    <LC.ItemList rt-fs='true' Items='[]' Style='~Raw' rt-prop-children='WhenNonempty(fruits)'>
                        <LC.Legacy.Card rt-map='fruit := fruits'>
                            {fruit.Name}
                        </LC.Legacy.Card>
                    </LC.ItemList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Raw),
                                                        items = ([]),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                    <LC.ItemList rt-fs='true' Items='[]' WhenEmpty='~Message ""Fruitless""' Style='~Raw' rt-prop-children='WhenNonempty(fruits)'>
                        <LC.Legacy.Card rt-map='fruit := fruits'>
                            {fruit.Name}
                        </LC.Legacy.Card>
                    </LC.ItemList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Raw),
                                                        whenEmpty = (LibClient.Components.ItemList.Message "Fruitless"),
                                                        items = ([]),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                                                AppEggShellGallery.Components.ComponentSample.Children
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                                    children =
                                                                        [|
                                                                            @"
                        <LC.ItemList rt-fs='true' Items='[]' Style='~Raw' rt-prop-children='WhenNonempty(fruits)'>
                            <LC.Legacy.Card rt-map='fruit := fruits'>
                                {fruit.Name}
                            </LC.Legacy.Card>
                            <rt-prop name='WhenEmpty |> ~Children'>
                                <div class='empty-message'><text class='empty-message-text'>No Fruit!</text></div>
                            </rt-prop>
                        </LC.ItemList>
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                                    heading = ("Styles"),
                                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                                    children =
                                                                        [|
                                                                            @"
                        ""empty-message"" => [
                            paddingVertical 20
                        ]

                        ""empty-message-text"" => [
                            TextAlign.Center
                            color    colors.Caution.Main
                            fontSize 18
                        ]
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Raw),
                                                        items = ([]),
                                                        whenEmpty =
                                                            (
                                                                LibClient.Components.ItemList.Children
                                                                    (
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                                let __currClass = "empty-message"
                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                ReactXP.Components.Constructors.RX.View(
                                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                    children =
                                                                                        [|
                                                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                                            let __currClass = "empty-message-text"
                                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                            LibClient.Components.Constructors.LC.LegacyText(
                                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                children =
                                                                                                    [|
                                                                                                        makeTextNode2 __parentFQN "No Fruit!"
                                                                                                    |]
                                                                                            )
                                                                                        |]
                                                                                )
                                                                            |])
                                                                    )
                                                            ),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                        heading = ("SeeAll"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.Children
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                                    children =
                                                                        [|
                                                                            @"
                        <LC.ItemList rt-fs='true' Items='fruits' Style='~Horizontal' SeeAll='~SeeAll.Default Actions.greet' rt-prop-children='WhenNonempty(fruits)' theme='fun theme -> { theme with SeeAll = { Height = 70; MarginVertical = 0} }'>
                            <LC.Legacy.Card rt-map='fruit := fruits'>
                                {fruit.Name}
                            </LC.Legacy.Card>
                        </LC.ItemList>
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        theme = (fun theme -> { theme with SeeAll = { Height = 70; MarginVertical = 0} }),
                                                        seeAll = (LibClient.Components.ItemList.SeeAll.Default Actions.greet),
                                                        style = (LibClient.Components.ItemList.Horizontal),
                                                        items = (fruits),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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
                    <LC.ItemList rt-fs='true' Items='fruits' Style='~Horizontal' rt-prop-children='WhenNonempty(fruits)'>
                        <LC.Legacy.Card rt-map='fruit := fruits'>
                            {fruit.Name}
                        </LC.Legacy.Card>
                        <rt-prop name='SeeAll |> ~SeeAll.Children'>
                            <div class='custom-see-all'>
                                <LC.TextButton rt-fs='true' Label='""See More""' State='^LowLevel(LC.TextButton.Actionable Actions.greet)'/>
                            </div>
                        </rt-prop>
                    </LC.ItemList>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ItemList"
                                                    LibClient.Components.Constructors.LC.ItemList(
                                                        style = (LibClient.Components.ItemList.Horizontal),
                                                        items = (fruits),
                                                        seeAll =
                                                            (
                                                                LibClient.Components.ItemList.SeeAll.Children
                                                                    (
                                                                            (castAsElementAckingKeysWarning [|
                                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                                let __currClass = "custom-see-all"
                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                ReactXP.Components.Constructors.RX.View(
                                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                                    children =
                                                                                        [|
                                                                                            let __parentFQN = Some "LibClient.Components.TextButton"
                                                                                            LibClient.Components.Constructors.LC.TextButton(
                                                                                                state = (LibClient.Components.TextButton.PropStateFactory.MakeLowLevel(LC.TextButton.Actionable Actions.greet)),
                                                                                                label = ("See More")
                                                                                            )
                                                                                        |]
                                                                                )
                                                                            |])
                                                                    )
                                                            ),
                                                        whenNonempty =
                                                            (fun (fruits) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        (
                                                                            (fruits)
                                                                            |> Seq.map
                                                                                (fun fruit ->
                                                                                    let __parentFQN = Some "LibClient.Components.Legacy.Card"
                                                                                    LibClient.Components.Constructors.LC.Legacy.Card(
                                                                                        children =
                                                                                            [|
                                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", fruit.Name))
                                                                                            |]
                                                                                    )
                                                                                )
                                                                            |> Array.ofSeq |> castAsElement
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

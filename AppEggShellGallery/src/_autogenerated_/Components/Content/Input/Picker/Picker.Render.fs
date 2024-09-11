module AppEggShellGallery.Components.Content.Input.PickerRender

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

open AppEggShellGallery.Components.Content.Input.Picker
open LibClient


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.Picker.Props, estate: AppEggShellGallery.Components.Content.Input.Picker.Estate, pstate: AppEggShellGallery.Components.Content.Input.Picker.Pstate, actions: AppEggShellGallery.Components.Content.Input.Picker.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.Picker"),
        isResponsive = (true),
        displayName = ("Input.Picker"),
        notes =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.Code"
                    AppEggShellGallery.Components.Constructors.Ui.Code(
                        heading = ("Relevant setup code"),
                        language = (AppEggShellGallery.Components.Code.Fsharp),
                        children =
                            [|
                                @"
            type Fruit =
            | Apple
            | Mango
            | Banana
            | Pear
            with
                member this.GetName : NonemptyString =
                    NonemptyString.ofLiteral (unionCaseName this)

            let fruits: OrderedSet<Fruit> =
                [Apple; Mango; Banana; Pear]
                |> OrderedSet.ofList

            let fruitItemVisuals (fruit: Fruit) : PickerItemVisuals = {|
                Label = fruit.GetName.Value
            |}

            let fruitToFilterString (fruit: Fruit): string =
                fruit.GetName.Value

            type Estate = {
                MaybeSelectedFruits: Option<OrderedSet<Fruit>>
            }

            override _.GetInitialEstate(_: Props) : Estate = {
                MaybeSelectedFruits = None
            }

            member _.SetMaybeSelectedFruits (value: OrderedSet<Fruit>) : unit =
                this.SetEstate(fun estate _ -> { estate with MaybeSelectedFruits = Some value })
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
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Static (fruits, fruitToFilterString)'
                     ItemView='~Default fruitItemVisuals'
                     Value='~AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterString)),
                                                        label = ("Fruit")
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
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Static (fruits, fruitToFilterString)'
                     ItemView='~Default fruitItemVisuals'
                     Value='~ExactlyOne (estate.MaybeExactlyOneSelectedFruit, actions.SetExactlyOneSelectedFruit)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.ExactlyOne (estate.MaybeExactlyOneSelectedFruit, actions.SetExactlyOneSelectedFruit)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterString)),
                                                        label = ("Fruit")
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
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Static (fruits, fruitToFilterString)'
                     ItemView='~Default fruitItemVisuals'
                     Value='~AtLeastOne (estate.MaybeAtLeastOneSelectedFruits, actions.SetAtLeastOneSelectedFruits)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.AtLeastOne (estate.MaybeAtLeastOneSelectedFruits, actions.SetAtLeastOneSelectedFruits)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterString)),
                                                        label = ("Fruit")
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
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Static (fruits, fruitToFilterString)'
                     ItemView='~Default fruitItemVisuals'
                     Value='~Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterString)),
                                                        label = ("Fruit")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Custom Rendering"),
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
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Static (fruits, fruitToFilterString)'
                     ItemView='~Custom render'
                     Value='~AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)'
                     Validity='Valid'>
                        <rt-outer-let name='render(item: Fruit)'>
                            <text>{item.GetName.Value.ToUpper()}</text>
                            <text>{item.GetName.Value.ToLower()}</text>
                        </rt-outer-let>
                    </LC.Input.Picker>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    let render(item: Fruit) =
                                                            (castAsElementAckingKeysWarning [|
                                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                LibClient.Components.Constructors.LC.LegacyText(
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", item.GetName.Value.ToUpper()))
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                LibClient.Components.Constructors.LC.LegacyText(
                                                                    children =
                                                                        [|
                                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", item.GetName.Value.ToLower()))
                                                                        |]
                                                                )
                                                            |])
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)),
                                                        itemView = (LibClient.Components.Input.Picker.Custom render),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterString)),
                                                        label = ("Fruit")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Async items"),
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
                                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                                    children =
                                                                        [|
                                                                            @"
                        let fetchFruitsAllOnNoQuery (maybeQuery: Option<NonemptyString>) : Async<OrderedSet<Fruit>> =
                            async {
                                // imitate backend delay
                                do! Async.Sleep (TimeSpan.FromMilliseconds 3000)
                                let filteredFruit =
                                    match maybeQuery with
                                    | None -> fruits
                                    | Some query ->
                                        let queryLower = query.Value.ToLower()
                                        fruits |> OrderedSet.filter (fun fruit -> fruit.GetName.Value.ToLower().Contains queryLower)
                                return filteredFruit
                            }
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                                    children =
                                                                        [|
                                                                            @"
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Async fetchFruitsAllOnNoQuery'
                     ItemView='~Default fruitItemVisuals'
                     Value='~AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)'
                     Validity='Valid'/>
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                            |])
                                                    )
                                            ),
                                        notes =
                                                (castAsElementAckingKeysWarning [|
                                                    makeTextNode2 __parentFQN "When the async returns ALL items when no query is entered"
                                                |]),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Async fetchFruitsAllOnNoQuery),
                                                        label = ("Fruit")
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
                                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                                    children =
                                                                        [|
                                                                            @"
                        let fetchFruitsEmptyOnNoQuery (maybeQuery: Option<NonemptyString>) : Async<OrderedSet<Fruit>> =
                            async {
                                // imitate backend delay
                                do! Async.Sleep (TimeSpan.FromMilliseconds 3000)
                                let filteredFruit =
                                    match maybeQuery with
                                    | None -> OrderedSet.empty
                                    | Some query ->
                                        let queryLower = query.Value.ToLower()
                                        fruits |> OrderedSet.filter (fun fruit -> fruit.GetName.Value.ToLower().Contains queryLower)
                                return filteredFruit
                            }
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                                    children =
                                                                        [|
                                                                            @"
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Async fetchFruitsEmptyOnNoQuery'
                     ItemView='~Default fruitItemVisuals'
                     Value='~AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)'
                     Validity='Valid'/>
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                            |])
                                                    )
                                            ),
                                        notes =
                                                (castAsElementAckingKeysWarning [|
                                                    makeTextNode2 __parentFQN "When the async returns NO items when no query is entered"
                                                |]),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.AtMostOne (estate.MaybeAtMostOneSelectedFruit, actions.SetAtMostOneSelectedFruit)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Async fetchFruitsEmptyOnNoQuery),
                                                        label = ("Fruit")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("With additional search text"),
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
                                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                                    children =
                                                                        [|
                                                                            @"
                        let fruitToFilterStringWithAdditionalText (fruit: Fruit) : string =
                            [(Apple, ""apel""); (Mango, ""aam""); (Banana, ""kola""); (Pear, ""nashpati"")]
                            |> List.find (fun (item, _) -> item = fruit)
                            |> fun (fruit: Fruit, searchText: string) -> sprintf ""%s %s"" fruit.GetName.Value searchText
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                                    children =
                                                                        [|
                                                                            @"
                        <LC.Input.Picker
                         Label='""Fruit""'
                         Items='~Static (fruits, fruitToFilterStringWithAdditionalText)'
                         ItemView='~Default fruitItemVisuals'
                         Value='~AtLeastOne (estate.MaybeSelectedFruitsWithSearchText, actions.SetSelectedFruitsWithSearchText)'
                         Validity='Valid'/>
                    "
                                                                            |> makeTextNode2 __parentFQN
                                                                        |]
                                                                )
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.AtLeastOne (estate.MaybeSelectedFruitsWithSearchText, actions.SetSelectedFruitsWithSearchText)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterStringWithAdditionalText)),
                                                        label = ("Fruit")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Many choices"),
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
                    <LC.Input.Picker
                     Label='""Many Choices""'
                     Items='~Static (manyItems, identity)'
                     ItemView='~Default stringItemVisuals'
                     Value='~Any (estate.MaybeSelectedItems, actions.SetMaybeSelectedItems)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Valid),
                                                        value = (LibClient.Components.Input.Picker.Any (estate.MaybeSelectedItems, actions.SetMaybeSelectedItems)),
                                                        itemView = (LibClient.Components.Input.Picker.Default stringItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (manyItems, identity)),
                                                        label = ("Many Choices")
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Validation (hardcoded, not dynamic)"),
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
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Static (fruits, fruitToFilterString)'
                     ItemView='~Default fruitItemVisuals'
                     Value='~Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)'
                     Validity='Missing'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Missing),
                                                        value = (LibClient.Components.Input.Picker.Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterString)),
                                                        label = ("Fruit")
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
                    <LC.Input.Picker
                     Label='""Fruit""'
                     Items='~Static (fruits, fruitToFilterString)'
                     ItemView='~Default fruitItemVisuals'
                     Value='~Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)'
                     Validity='Invalid ""Not a fruit""'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.Picker"
                                                    LibClient.Components.Constructors.LC.Input.Picker(
                                                        validity = (Invalid "Not a fruit"),
                                                        value = (LibClient.Components.Input.Picker.Any (estate.MaybeAnySelectedFruits, actions.SetAnySelectedFruits)),
                                                        itemView = (LibClient.Components.Input.Picker.Default fruitItemVisuals),
                                                        items = (LibClient.Components.Input.Picker.Static (fruits, fruitToFilterString)),
                                                        label = ("Fruit")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )

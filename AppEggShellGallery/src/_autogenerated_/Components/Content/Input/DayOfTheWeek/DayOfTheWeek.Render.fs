module AppEggShellGallery.Components.Content.Input.DayOfTheWeekRender

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

open AppEggShellGallery.Components.Content.Input.DayOfTheWeek



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Input.DayOfTheWeek.Props, estate: AppEggShellGallery.Components.Content.Input.DayOfTheWeek.Estate, pstate: AppEggShellGallery.Components.Content.Input.DayOfTheWeek.Pstate, actions: AppEggShellGallery.Components.Content.Input.DayOfTheWeek.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Input.DayOfTheWeek"),
        displayName = ("Input.DayOfTheWeek"),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Modes"),
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
                    <LC.Input.DayOfTheWeek
                     Label='""Select at most one day""'
                     Mode='~MaybeOne (estate.MaybeOne, actions.SetValue)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.DayOfTheWeek"
                                                    LibClient.Components.Constructors.LC.Input.DayOfTheWeek(
                                                        validity = (Valid),
                                                        mode = (LibClient.Components.Input.DayOfTheWeek.MaybeOne (estate.MaybeOne, actions.SetValue)),
                                                        label = ("Select at most one day")
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
                    <LC.Input.DayOfTheWeek
                     Label='""Select exactly one day""'
                     Mode='~One (estate.One, actions.SetValue)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.DayOfTheWeek"
                                                    LibClient.Components.Constructors.LC.Input.DayOfTheWeek(
                                                        validity = (Valid),
                                                        mode = (LibClient.Components.Input.DayOfTheWeek.One (estate.One, actions.SetValue)),
                                                        label = ("Select exactly one day")
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
                    <LC.Input.DayOfTheWeek
                     Label='""Select some days""'
                     Mode='~Set (estate.Set, actions.SetValue)'
                     Validity='Valid'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.DayOfTheWeek"
                                                    LibClient.Components.Constructors.LC.Input.DayOfTheWeek(
                                                        validity = (Valid),
                                                        mode = (LibClient.Components.Input.DayOfTheWeek.Set (estate.Set, actions.SetValue)),
                                                        label = ("Select some days")
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
                    <LC.Input.DayOfTheWeek
                     Label='""Select some days""'
                     Validity='Missing'
                     Mode='~Set (Set.empty, ignore)'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.DayOfTheWeek"
                                                    LibClient.Components.Constructors.LC.Input.DayOfTheWeek(
                                                        mode = (LibClient.Components.Input.DayOfTheWeek.Set (Set.empty, ignore)),
                                                        validity = (Missing),
                                                        label = ("Select some days")
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
                    <!-- NO LABEL CASE -->
                    <LC.Input.DayOfTheWeek
                     Validity='Missing'
                     Mode='~Set (Set.empty, ignore)'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.DayOfTheWeek"
                                                    LibClient.Components.Constructors.LC.Input.DayOfTheWeek(
                                                        mode = (LibClient.Components.Input.DayOfTheWeek.Set (Set.empty, ignore)),
                                                        validity = (Missing)
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
                    <LC.Input.DayOfTheWeek
                     Label='""Select some days""'
                     Validity='Invalid ""You chose poorly!""'
                     Mode='~Set (Set.empty, ignore)'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.DayOfTheWeek"
                                                    LibClient.Components.Constructors.LC.Input.DayOfTheWeek(
                                                        mode = (LibClient.Components.Input.DayOfTheWeek.Set (Set.empty, ignore)),
                                                        validity = (Invalid "You chose poorly!"),
                                                        label = ("Select some days")
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
                    <!-- NO LABEL CASE -->
                    <LC.Input.DayOfTheWeek
                     Validity='Invalid ""You chose poorly!""'
                     Mode='~Set (Set.empty, ignore)'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.Input.DayOfTheWeek"
                                                    LibClient.Components.Constructors.LC.Input.DayOfTheWeek(
                                                        mode = (LibClient.Components.Input.DayOfTheWeek.Set (Set.empty, ignore)),
                                                        validity = (Invalid "You chose poorly!")
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )

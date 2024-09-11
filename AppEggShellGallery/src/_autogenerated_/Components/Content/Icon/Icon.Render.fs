module AppEggShellGallery.Components.Content.IconRender

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

open AppEggShellGallery.Components.Content.Icon



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Icon.Props, estate: AppEggShellGallery.Components.Content.Icon.Estate, pstate: AppEggShellGallery.Components.Content.Icon.Pstate, actions: AppEggShellGallery.Components.Content.Icon.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Icon"),
        displayName = ("Icon"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "By default, setting colors on icons is done directly though a parameter to the SVG, which\n        makes icon colors the only visual aspect of the app outside of the styles system.\n        This component allows us to overcome this limitation, and unify all styling."
                |]),
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
                    <LC.Icon rt-fs='true' Icon='Icon.Megaphone'/>
                    <LC.Icon Icon='Icon.Send' class='a'/>
                    <LC.Icon Icon='Icon.Home' class='b'/>
                    {=LC.Icon (icon = Icon.Send, styles = [| Styles.c |])}
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
                    ""a"" => [
                        color    (Color.Hex ""#fa6e1b"")
                        fontSize 50
                    ]

                    ""b"" => [
                        color (Color.Hex ""#9b48db"")
                    ]


                    type Styles =
                        static member c = makeTextStyles {
                            fontSize 60
                            color Color.DevPink
                        }
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Icon"
                                    LibClient.Components.Constructors.LC.Icon(
                                        icon = (Icon.Megaphone)
                                    )
                                    let __parentFQN = Some "LibClient.Components.Icon"
                                    let __currClass = "a"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Icon(
                                        icon = (Icon.Send),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                    let __parentFQN = Some "LibClient.Components.Icon"
                                    let __currClass = "b"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Icon(
                                        icon = (Icon.Home),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                    LC.Icon (icon = Icon.Send, styles = [| Styles.c |])
                                    LC.Icon (icon = Icon.Home, styles = [| Styles.c |])
                                |])
                    )
                |])
    )

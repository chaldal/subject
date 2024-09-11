module AppEggShellGallery.Components.ColorVariantsRender

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

open AppEggShellGallery.Components.ColorVariants



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.ColorVariants.Props, estate: AppEggShellGallery.Components.ColorVariants.Estate, pstate: AppEggShellGallery.Components.ColorVariants.Pstate, actions: AppEggShellGallery.Components.ColorVariants.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let value = props.Value
        let main = value.Main
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
            children =
                [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B900 = main),
                        color = (value.B900),
                        name = ("900")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B800 = main),
                        color = (value.B800),
                        name = ("800")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B700 = main),
                        color = (value.B700),
                        name = ("700")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B600 = main),
                        color = (value.B600),
                        name = ("600")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B500 = main),
                        color = (value.B500),
                        name = ("500")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B400 = main),
                        color = (value.B400),
                        name = ("400")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B300 = main),
                        color = (value.B300),
                        name = ("300")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B200 = main),
                        color = (value.B200),
                        name = ("200")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B100 = main),
                        color = (value.B100),
                        name = ("100")
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.Base"
                    AppEggShellGallery.Components.Constructors.Ui.ColorVariant.Base(
                        isMain = (value.B050 = main),
                        color = (value.B050),
                        name = ("050")
                    )
                |]
        )
    )

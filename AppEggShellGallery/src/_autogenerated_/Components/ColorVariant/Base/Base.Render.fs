module AppEggShellGallery.Components.ColorVariant.BaseRender

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

open AppEggShellGallery.Components.ColorVariant.Base



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.ColorVariant.Base.Props, estate: AppEggShellGallery.Components.ColorVariant.Base.Estate, pstate: AppEggShellGallery.Components.ColorVariant.Base.Pstate, actions: AppEggShellGallery.Components.ColorVariant.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
                let __parentFQN = Some "AppEggShellGallery.Components.ColorVariant.ColorBlock"
                AppEggShellGallery.Components.Constructors.Ui.ColorVariant.ColorBlock(
                    color = (props.Color),
                    isMain = (props.IsMain)
                )
                let __parentFQN = Some "LibClient.Components.LegacyUiText"
                LibClient.Components.Constructors.LC.LegacyUiText(
                    children =
                        [|
                            makeTextNode2 __parentFQN (System.String.Format("{0}", props.Name))
                        |]
                )
            |]
    )

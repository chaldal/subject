module AppEggShellGallery.Components.Route.SettingsRender

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

open AppEggShellGallery.Components.Route.Settings



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Route.Settings.Props, estate: AppEggShellGallery.Components.Route.Settings.Estate, pstate: AppEggShellGallery.Components.Route.Settings.Pstate, actions: AppEggShellGallery.Components.Route.Settings.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibRouter.Components.Route"
    LibRouter.Components.Constructors.LR.Route(
        scroll = (LibRouter.Components.Route.Vertical),
        children =
            [|
                makeTextNode2 __parentFQN "Settings screen, in theory. Nothing to actually see here."
            |]
    )

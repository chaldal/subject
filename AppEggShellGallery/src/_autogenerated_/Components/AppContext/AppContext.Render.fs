module AppEggShellGallery.Components.AppContextRender

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

open AppEggShellGallery.Components.AppContext



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.AppContext.Props, estate: AppEggShellGallery.Components.AppContext.Estate, pstate: AppEggShellGallery.Components.AppContext.Pstate, actions: AppEggShellGallery.Components.AppContext.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibRouter.Components.NavigationRouter"
    LibRouter.Components.Constructors.LR.NavigationRouter(
        queue = (navigationQueue),
        navigationState = (navigationState),
        spec = (routesSpec()),
        child =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "LibClient.Components.AppShell.Context"
                    LibClient.Components.Constructors.LC.AppShell.Context(
                        children = (children)
                    )
                |])
    )

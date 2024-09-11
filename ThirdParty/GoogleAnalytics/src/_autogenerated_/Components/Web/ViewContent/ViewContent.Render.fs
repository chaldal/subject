module ThirdParty.GoogleAnalytics.Components.Web.ViewContentRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard

open LibClient.RenderResultModule

open LibClient.Components
open ReactXP.Components
open ThirdParty.GoogleAnalytics.Components



open ThirdParty.GoogleAnalytics.Components.Web.ViewContent



let render(props: ThirdParty.GoogleAnalytics.Components.Web.ViewContent.Props, estate: ThirdParty.GoogleAnalytics.Components.Web.ViewContent.Estate, pstate: ThirdParty.GoogleAnalytics.Components.Web.ViewContent.Pstate, actions: ThirdParty.GoogleAnalytics.Components.Web.ViewContent.Actions, __componentStyles: ReactXP.Styles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.Styles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (fun (__sibI, __sibC, __pFQN) ->
        let __parentFQN = Some "ReactXP.Components.View"
        ReactXP.Components.View.Make
            (
                let __currExplicitProps = (ReactXP.Components.TypeExtensions.TEs.MakeViewProps())
                let __currClass = (nthChildClasses __sibI __sibC)
                let __currStyles = (ReactXP.Styles.Runtime.findApplicableStyles __mergedStyles __currClass)
                let __implicitProps = [
                    if __currClass <> "" then ("ClassName", __currClass :> obj)
                    if (not __currStyles.IsEmpty) then ("style", ReactXP.Styles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles)
                ]
                (ReactXP.Styles.Runtime.injectImplicitProps __implicitProps __currExplicitProps)
            )
            []
    ) |> RenderResult.Make
    |> RenderResult.ToRawElementsSingle __parentFQN |> RenderResult.ToSingleElement
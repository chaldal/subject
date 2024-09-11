module AppEggShellGallery.Components.Content.ColorVariantsRender

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

open AppEggShellGallery.Components.Content.ColorVariants



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ColorVariants.Props, estate: AppEggShellGallery.Components.Content.ColorVariants.Estate, pstate: AppEggShellGallery.Components.Content.ColorVariants.Pstate, actions: AppEggShellGallery.Components.Content.ColorVariants.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let __parentFQN = Some "LibClient.Components.Heading"
        LibClient.Components.Constructors.LC.Heading(
            children =
                [|
                    makeTextNode2 __parentFQN "Color Variants"
                |]
        )
        let __parentFQN = Some "ReactXP.Components.ScrollView"
        ReactXP.Components.Constructors.RX.ScrollView(
            horizontal = (true),
            children =
                [|
                    (
                        (AppEggShellGallery.ScrapedData.Colors.colorVariantsData)
                        |> Seq.map
                            (fun (colorName, colorVariantFn) ->
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "color-variant-container"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            ReactXP.Components.Constructors.RX.View(
                                                children =
                                                    [|
                                                        let __parentFQN = Some "LibClient.Components.Heading"
                                                        LibClient.Components.Constructors.LC.Heading(
                                                            level = (LibClient.Components.Heading.Secondary),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", colorName))
                                                                |]
                                                        )
                                                        let __parentFQN = Some "AppEggShellGallery.Components.ColorVariants"
                                                        let __currClass = "variant"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        AppEggShellGallery.Components.Constructors.Ui.ColorVariants(
                                                            value = (colorVariantFn),
                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                        )
                                                    |]
                                            )
                                        |]
                                )
                            )
                        |> Array.ofSeq |> castAsElement
                    )
                |]
        )
    |])

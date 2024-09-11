module AppEggShellGallery.Components.ComponentSampleGroupRender

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

open AppEggShellGallery.Components.ComponentSampleGroup



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.ComponentSampleGroup.Props, estate: AppEggShellGallery.Components.ComponentSampleGroup.Estate, pstate: AppEggShellGallery.Components.ComponentSampleGroup.Pstate, actions: AppEggShellGallery.Components.ComponentSampleGroup.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        #if EGGSHELL_PLATFORM_IS_WEB
        (
            if ((props.Heading = None) && (props.Notes = noElement)) then
                FRS.tr
                    [(FRP.ClassName ("csg-vertical-padding"))]
                    ([|
                        FRS.td
                            [(FRP.ColSpan ((2)))]
                            [||]
                    |])
            else noElement
        )
        (
            (props.Heading)
            |> Option.map
                (fun heading ->
                    FRS.tr
                        [(FRP.ClassName ("csg-heading"))]
                        ([|
                            FRS.td
                                [(FRP.ColSpan ((2)))]
                                ([|
                                    let __parentFQN = Some "LibClient.Components.Heading"
                                    let __currClass = "heading"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Heading(
                                        level = (LibClient.Components.Heading.Tertiary),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN (System.String.Format("{0}", heading))
                                            |]
                                    )
                                |])
                        |])
                )
            |> Option.getOrElse noElement
        )
        (
            if (not (props.Notes = noElement)) then
                FRS.tr
                    [(FRP.ClassName ("csg-notes"))]
                    ([|
                        FRS.td
                            [(FRP.ColSpan ((2)))]
                            ([|
                                props.Notes
                            |])
                    |])
            else noElement
        )
        props.Samples
        FRS.tr
            [(FRP.ClassName ("csg-vertical-padding"))]
            ([|
                FRS.td
                    [(FRP.ColSpan ((2)))]
                    [||]
            |])
        #else
        (
            (props.Heading)
            |> Option.map
                (fun heading ->
                    let __parentFQN = Some "LibClient.Components.Heading"
                    let __currClass = "heading"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    LibClient.Components.Constructors.LC.Heading(
                        level = (LibClient.Components.Heading.Tertiary),
                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                        children =
                            [|
                                makeTextNode2 __parentFQN (System.String.Format("{0}", heading))
                            |]
                    )
                )
            |> Option.getOrElse noElement
        )
        props.Notes
        props.Samples
        #endif
    |])

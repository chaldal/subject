module AppEggShellGallery.Components.ComponentPropsRender

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

open AppEggShellGallery.Components.ComponentProps
open LibRenderDSL.Types


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.ComponentProps.Props, estate: AppEggShellGallery.Components.ComponentProps.Estate, pstate: AppEggShellGallery.Components.ComponentProps.Pstate, actions: AppEggShellGallery.Components.ComponentProps.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = "view"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
        children =
            [|
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
                let __parentFQN = Some "ReactXP.Components.View"
                let __currClass = "content" + System.String.Format(" {0}", (if (props.Heading.IsSome) then "indented" else ""))
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.View(
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                    children =
                        [|
                            (
                                (props.Data.MaybeScrapeErrors)
                                |> Option.map
                                    (fun errors ->
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        ReactXP.Components.Constructors.RX.View(
                                            children =
                                                [|
                                                    (
                                                        (NonemptyList.toList errors)
                                                        |> Seq.map
                                                            (fun error ->
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "error"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                            let __currClass = "error-text"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            LibClient.Components.Constructors.LC.LegacyText(
                                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                children =
                                                                                    [|
                                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", error))
                                                                                    |]
                                                                            )
                                                                        |]
                                                                )
                                                            )
                                                        |> Array.ofSeq |> castAsElement
                                                    )
                                                |]
                                        )
                                    )
                                |> Option.getOrElse noElement
                            )
                            match (props.Data.FieldsWithoutKey) with
                            | Ok [] ->
                                [|
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    ReactXP.Components.Constructors.RX.View(
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                let __currClass = "meta-content"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                LibClient.Components.Constructors.LC.LegacyText(
                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                    children =
                                                        [|
                                                            makeTextNode2 __parentFQN "No props"
                                                        |]
                                                )
                                            |]
                                    )
                                |]
                            | Ok fields ->
                                [|
                                    #if EGGSHELL_PLATFORM_IS_WEB
                                    FRS.table
                                        [(FRP.ClassName ("aesg-ComponentProps-table dom-user-select-text"))]
                                        ([|
                                            FRS.thead
                                                []
                                                ([|
                                                    FRS.tr
                                                        []
                                                        ([|
                                                            FRS.th
                                                                []
                                                                ([|
                                                                    makeTextNode2 __parentFQN "Name"
                                                                |])
                                                            FRS.th
                                                                []
                                                                ([|
                                                                    makeTextNode2 __parentFQN "Type"
                                                                |])
                                                            FRS.th
                                                                []
                                                                [||]
                                                            FRS.th
                                                                []
                                                                ([|
                                                                    makeTextNode2 __parentFQN "Default"
                                                                |])
                                                            FRS.th
                                                                []
                                                                ([|
                                                                    makeTextNode2 __parentFQN "Description"
                                                                |])
                                                        |])
                                                |])
                                            FRS.tbody
                                                []
                                                ([|
                                                    (
                                                        (fields)
                                                        |> Seq.mapi
                                                            (fun i field ->
                                                                FRS.tr
                                                                    [unbox("key", (i))]
                                                                    ([|
                                                                        FRS.td
                                                                            [(FRP.ClassName ("name"))]
                                                                            ([|
                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", field.Name))
                                                                            |])
                                                                        FRS.td
                                                                            [(FRP.ClassName ("type"))]
                                                                            ([|
                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", field.Type))
                                                                            |])
                                                                        FRS.td
                                                                            []
                                                                            [||]
                                                                        FRS.td
                                                                            [(FRP.ClassName ("value"))]
                                                                            ([|
                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", field.Default |> Option.getOrElse ""))
                                                                            |])
                                                                        FRS.td
                                                                            [(FRP.ClassName ("description"))]
                                                                            ([|
                                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", field.Description |> Option.getOrElse ""))
                                                                            |])
                                                                    |])
                                                            )
                                                        |> Array.ofSeq |> castAsElement
                                                    )
                                                |])
                                        |])
                                    #else
                                    (
                                        (fields)
                                        |> Seq.mapi
                                            (fun i field ->
                                                let __parentFQN = Some "ReactXP.Components.View"
                                                let __currClass = "props"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                ReactXP.Components.Constructors.RX.View(
                                                    key = (i.ToString()),
                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                    children =
                                                        [|
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            ReactXP.Components.Constructors.RX.View(
                                                                children =
                                                                    [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Name: ", (field.Name), ""))
                                                                    |]
                                                            )
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            ReactXP.Components.Constructors.RX.View(
                                                                children =
                                                                    [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Type: ", (field.Type), ""))
                                                                    |]
                                                            )
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            ReactXP.Components.Constructors.RX.View(
                                                                children =
                                                                    [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Default: ", (field.Default |> Option.getOrElse ""), ""))
                                                                    |]
                                                            )
                                                            let __parentFQN = Some "ReactXP.Components.View"
                                                            ReactXP.Components.Constructors.RX.View(
                                                                children =
                                                                    [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Description: ", (field.Description |> Option.getOrElse ""), ""))
                                                                    |]
                                                            )
                                                        |]
                                                )
                                            )
                                        |> Array.ofSeq |> castAsElement
                                    )
                                    #endif
                                |]
                            | Error error ->
                                [|
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "error"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                let __currClass = "error-text"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                LibClient.Components.Constructors.LC.LegacyText(
                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                    children =
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}", error))
                                                        |]
                                                )
                                            |]
                                    )
                                |]
                            |> castAsElementAckingKeysWarning
                        |]
                )
            |]
    )

module AppEggShellGallery.Components.Route.HomeRender

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

open AppEggShellGallery.Components.Route.Home
open LibClient.ContextMenus


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Route.Home.Props, estate: AppEggShellGallery.Components.Route.Home.Estate, pstate: AppEggShellGallery.Components.Route.Home.Pstate, actions: AppEggShellGallery.Components.Route.Home.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let __parentFQN = Some "LibClient.Components.SetPageMetadata"
        LibClient.Components.Constructors.LC.SetPageMetadata(
            title = ("Home")
        )
        let __parentFQN = Some "LibRouter.Components.Route"
        LibRouter.Components.Constructors.LR.Route(
            scroll = (LibRouter.Components.Route.Vertical),
            children =
                [|
                    let __parentFQN = Some "LibClient.Components.Section.Padded"
                    let __currClass = "content"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    LibClient.Components.Constructors.LC.Section.Padded(
                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                        children =
                            [|
                                let __parentFQN = Some "ReactXP.Components.Image"
                                let __currClass = "logo-image"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.Image(
                                    size = (ReactXP.Components.Image.FromStyles),
                                    source = (localImage "/images/logo-sketch.jpg"),
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Image" __currStyles |> Some) else None)
                                )
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "subtitle"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "content-text"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN "EggShell is a tech stack for front end apps."
                                                    |]
                                            )
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "content-text"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN "It combines a number of technologies:"
                                                    |]
                                            )
                                        |]
                                )
                                let __parentFQN = Some "ReactXP.Components.View"
                                let __currClass = "table"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                ReactXP.Components.Constructors.RX.View(
                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                    children =
                                        [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "row"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-left"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                    let __currClass = "label"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                        children =
                                                                            [|
                                                                                makeTextNode2 __parentFQN "F#"
                                                                            |]
                                                                    )
                                                                |]
                                                        )
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-right"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN "a functional programming language with a powerful type system"
                                                                |]
                                                        )
                                                    |]
                                            )
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "row"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-left"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                    let __currClass = "label"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                        children =
                                                                            [|
                                                                                makeTextNode2 __parentFQN "React"
                                                                            |]
                                                                    )
                                                                |]
                                                        )
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-right"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN "a JS library for building UIs declaratively"
                                                                |]
                                                        )
                                                    |]
                                            )
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "row"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-left"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                    let __currClass = "label"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                        children =
                                                                            [|
                                                                                makeTextNode2 __parentFQN "Fable"
                                                                            |]
                                                                    )
                                                                |]
                                                        )
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-right"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN "an F# to JS compiler"
                                                                |]
                                                        )
                                                    |]
                                            )
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "row"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-left"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                                    let __currClass = "label"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                        children =
                                                                            [|
                                                                                makeTextNode2 __parentFQN "ReactXP"
                                                                            |]
                                                                    )
                                                                |]
                                                        )
                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                        let __currClass = "cell-right"
                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                        ReactXP.Components.Constructors.RX.View(
                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                            children =
                                                                [|
                                                                    makeTextNode2 __parentFQN "a layer on top of React that allows targeting ReactDOM and ReactNative from the same code base"
                                                                |]
                                                        )
                                                    |]
                                            )
                                        |]
                                )
                            |]
                    )
                |]
        )
    |])

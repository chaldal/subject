module LibClient.Components.AppShell.TopLevelErrorMessageRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibClient.Components

open LibClient
open LibClient.RenderHelpers
open LibClient.Icons
open LibClient.Chars
open LibClient.ColorModule
open LibClient.LocalImages
open LibClient.Responsive

open LibClient.Components.AppShell.TopLevelErrorMessage
open ReactXP.LegacyStyles
open LibClient


let render(children: array<ReactElement>, props: LibClient.Components.AppShell.TopLevelErrorMessage.Props, estate: LibClient.Components.AppShell.TopLevelErrorMessage.Estate, pstate: LibClient.Components.AppShell.TopLevelErrorMessage.Pstate, actions: LibClient.Components.AppShell.TopLevelErrorMessage.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.ScreenSize"
    LibClient.Components.Constructors.LC.With.ScreenSize(
        ``with`` =
            (fun (screenSize) ->
                    (castAsElementAckingKeysWarning [|
                        let genericError = (
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Icon"
                                    let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (screenSize.Class), ""))
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Icon(
                                        icon = (Icon.Error),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                    )
                                    let __parentFQN = Some "LibClient.Components.Heading"
                                    let __currClass = "error-heading"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Heading(
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN "Oops!"
                                            |]
                                    )
                                    let __parentFQN = Some "LibClient.Components.Heading"
                                    let __currClass = "error-title"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Heading(
                                        level = (LibClient.Components.Heading.Secondary),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN "Something went wrong"
                                            |]
                                    )
                                    let __parentFQN = Some "LibClient.Components.Heading"
                                    let __currClass = "error-subtitle"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    LibClient.Components.Constructors.LC.Heading(
                                        level = (LibClient.Components.Heading.Tertiary),
                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                        children =
                                            [|
                                                makeTextNode2 __parentFQN "Please try again later."
                                            |]
                                    )
                                |])
                        )
                        let __parentFQN = Some "LibClient.Components.With.Layout"
                        LibClient.Components.Constructors.LC.With.Layout(
                            ``with`` =
                                (fun (_, maybeLayout) ->
                                        (castAsElementAckingKeysWarning [|
                                            let __parentFQN = Some "ReactXP.Components.View"
                                            let __currClass = "view"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            let __dynamicStyles: List<ReactXP.LegacyStyles.RuleFunctionReturnedStyleRules> = maybeLayout |> Option.map (fun l -> [minHeight l.Height]) |> Option.getOrElse [height 0]
                                            let __currStyles = __currStyles @ (ReactXP.LegacyStyles.Designtime.processDynamicStyles __dynamicStyles)
                                            ReactXP.Components.Constructors.RX.View(
                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                children =
                                                    [|
                                                        let __parentFQN = Some "ReactXP.Components.ScrollView"
                                                        ReactXP.Components.Constructors.RX.ScrollView(
                                                            vertical = (true),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                                    let __currClass = "center"
                                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                    ReactXP.Components.Constructors.RX.View(
                                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                        children =
                                                                            [|
                                                                                match (props.Error) with
                                                                                | AsyncDataException AsyncDataFailure.NetworkFailure ->
                                                                                    [|
                                                                                        let __parentFQN = Some "LibClient.Components.Icon"
                                                                                        let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (screenSize.Class), ""))
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        LibClient.Components.Constructors.LC.Icon(
                                                                                            icon = (Icon.NoNetwork),
                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Heading"
                                                                                        let __currClass = "error-heading"
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        LibClient.Components.Constructors.LC.Heading(
                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                            children =
                                                                                                [|
                                                                                                    makeTextNode2 __parentFQN "Internet Problem"
                                                                                                |]
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Heading"
                                                                                        let __currClass = "error-title"
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        LibClient.Components.Constructors.LC.Heading(
                                                                                            level = (LibClient.Components.Heading.Secondary),
                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                            children =
                                                                                                [|
                                                                                                    makeTextNode2 __parentFQN "Unable to connect to the internet."
                                                                                                |]
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Heading"
                                                                                        let __currClass = "error-subtitle"
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        LibClient.Components.Constructors.LC.Heading(
                                                                                            level = (LibClient.Components.Heading.Tertiary),
                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                            children =
                                                                                                [|
                                                                                                    makeTextNode2 __parentFQN "Please make sure you are connected to the internet and reload."
                                                                                                |]
                                                                                        )
                                                                                    |]
                                                                                | AsyncDataException (AsyncDataFailure.RequestFailure requestFailure) ->
                                                                                    [|
                                                                                        match (requestFailure) with
                                                                                        | RequestFailure.ClientError (statusCode, response) ->
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (screenSize.Class), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (Icon.ServerError),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-heading"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "Request Failed"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-title"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    level = (LibClient.Components.Heading.Secondary),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "App request failed!"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-subtitle"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    level = (LibClient.Components.Heading.Tertiary),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "Please try to reload. If the problem remains, please update the app to latest version."
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.InfoMessage"
                                                                                                LibClient.Components.Constructors.LC.InfoMessage(
                                                                                                    message = (statusCode.ToString() + " - " + response.ToString())
                                                                                                )
                                                                                            |]
                                                                                        | RequestFailure.ServerError (statusCode, response) ->
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (screenSize.Class), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (Icon.ServerError),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-heading"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "Server Error"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-title"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    level = (LibClient.Components.Heading.Secondary),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "There seems to be problem with our server!"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-subtitle"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    level = (LibClient.Components.Heading.Tertiary),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "Please try to reload. If the problem remains contact support"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.InfoMessage"
                                                                                                LibClient.Components.Constructors.LC.InfoMessage(
                                                                                                    message = (statusCode.ToString() + " - " + response.ToString())
                                                                                                )
                                                                                            |]
                                                                                        | RequestFailure.Unknown (statusCode, response) ->
                                                                                            [|
                                                                                                let __parentFQN = Some "LibClient.Components.Icon"
                                                                                                let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (screenSize.Class), ""))
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Icon(
                                                                                                    icon = (Icon.ServerError),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-heading"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "Unknown Server Error"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-title"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    level = (LibClient.Components.Heading.Secondary),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "There seems to be problem with our server!"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.Heading"
                                                                                                let __currClass = "error-subtitle"
                                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                                LibClient.Components.Constructors.LC.Heading(
                                                                                                    level = (LibClient.Components.Heading.Tertiary),
                                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                                    children =
                                                                                                        [|
                                                                                                            makeTextNode2 __parentFQN "Please try to reload. If the problem remains contact support"
                                                                                                        |]
                                                                                                )
                                                                                                let __parentFQN = Some "LibClient.Components.InfoMessage"
                                                                                                LibClient.Components.Constructors.LC.InfoMessage(
                                                                                                    message = (statusCode.ToString() + " - " + response.ToString())
                                                                                                )
                                                                                            |]
                                                                                        |> castAsElementAckingKeysWarning
                                                                                    |]
                                                                                | AsyncDataException (AsyncDataFailure.UserReadableFailure message) ->
                                                                                    [|
                                                                                        let __parentFQN = Some "LibClient.Components.Icon"
                                                                                        let __currClass = (System.String.Format("{0}{1}{2}", "icon ", (screenSize.Class), ""))
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        LibClient.Components.Constructors.LC.Icon(
                                                                                            icon = (Icon.Error),
                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                        )
                                                                                        let __parentFQN = Some "LibClient.Components.Heading"
                                                                                        let __currClass = "error-title"
                                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                        LibClient.Components.Constructors.LC.Heading(
                                                                                            level = (LibClient.Components.Heading.Secondary),
                                                                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                                                            children =
                                                                                                [|
                                                                                                    makeTextNode2 __parentFQN (System.String.Format("{0}", message))
                                                                                                |]
                                                                                        )
                                                                                    |]
                                                                                | AsyncDataException (AsyncDataFailure.UnknownFailure _) ->
                                                                                    [|
                                                                                        genericError
                                                                                    |]
                                                                                | _ ->
                                                                                    [|
                                                                                        genericError
                                                                                    |]
                                                                                |> castAsElementAckingKeysWarning
                                                                                let __parentFQN = Some "LibClient.Components.Button"
                                                                                let __currClass = "button"
                                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                                LibClient.Components.Constructors.LC.Button(
                                                                                    state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (LibClient.Components.Button.Actionable actions.reload)),
                                                                                    level = (LibClient.Components.Button.Secondary),
                                                                                    label = ("Reload"),
                                                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                                                )
                                                                            |]
                                                                    )
                                                                |]
                                                        )
                                                    |]
                                            )
                                        |])
                                )
                        )
                    |])
            )
    )

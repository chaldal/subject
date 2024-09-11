module LibClient.Components.Dialog.Shell.WhiteRounded.RawRender

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

open LibClient.Components.Dialog.Shell.WhiteRounded.Raw
open LibClient


let render(children: array<ReactElement>, props: LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Props, estate: LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Estate, pstate: LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Pstate, actions: LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                        (
                            let onPress = if ReactXP.Runtime.isNative() then Undefined else (fun (e: Browser.Types.PointerEvent) -> e.stopPropagation())
                            let __parentFQN = Some "LibClient.Components.Dialog.Base"
                            LibClient.Components.Constructors.LC.Dialog.Base(
                                canClose = (props.CanClose),
                                contentPosition = (match screenSize with ScreenSize.Desktop -> LibClient.Components.Dialog.Base.Center | ScreenSize.Handheld -> LibClient.Components.Dialog.Base.Free),
                                children =
                                    [|
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        let __currClass = (System.String.Format("{0}{1}{2}", "max-size-limiter position-", (props.Position), ""))
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        ReactXP.Components.Constructors.RX.View(
                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                            children =
                                                [|
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    let __currClass = (System.String.Format("{0}{1}{2}", "white-rounded-base ", (screenSize.Class), ""))
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ReactXP.Components.Constructors.RX.View(
                                                        onPress = (onPress),
                                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ReactXP.Components.View"
                                                                let __currClass = "content"
                                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                ReactXP.Components.Constructors.RX.View(
                                                                    children = (children),
                                                                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None)
                                                                )
                                                                (
                                                                    if (props.InProgress) then
                                                                        let __parentFQN = Some "ReactXP.Components.View"
                                                                        let __currClass = "in-progress"
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.View(
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                                                            children =
                                                                                [|
                                                                                    let __parentFQN = Some "ReactXP.Components.ActivityIndicator"
                                                                                    ReactXP.Components.Constructors.RX.ActivityIndicator(
                                                                                        color = ((Color.Grey "cc").ToReactXPString)
                                                                                    )
                                                                                |]
                                                                        )
                                                                    else noElement
                                                                )
                                                                (
                                                                    if (props.CanClose.ShouldShowCloseButton) then
                                                                        let __parentFQN = Some "LibClient.Components.IconButton"
                                                                        LibClient.Components.Constructors.LC.IconButton(
                                                                            state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LC.IconButton.Actionable props.CanClose.OnClose)),
                                                                            icon = (Icon.X),
                                                                            theme = (RawStyles.Styles.closeButtonTheme),
                                                                            styles = ([| RawStyles.Styles.closeButton |])
                                                                        )
                                                                    else noElement
                                                                )
                                                            |]
                                                    )
                                                |]
                                        )
                                    |]
                            )
                        )
                    |])
            )
    )

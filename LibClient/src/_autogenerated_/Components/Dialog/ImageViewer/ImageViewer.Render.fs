module LibClient.Components.Dialog.ImageViewerRender

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

open LibClient.Components.Dialog.ImageViewer



let render(children: array<ReactElement>, props: LibClient.Components.Dialog.ImageViewer.Props, estate: LibClient.Components.Dialog.ImageViewer.Estate, pstate: LibClient.Components.Dialog.ImageViewer.Pstate, actions: LibClient.Components.Dialog.ImageViewer.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let sources = Seq.toList props.Parameters.Sources
        let __parentFQN = Some "LibClient.Components.With.ScreenSize"
        LibClient.Components.Constructors.LC.With.ScreenSize(
            ``with`` =
                (fun (screenSize) ->
                        (castAsElementAckingKeysWarning [|
                            let carousel = (
                                    (castAsElementAckingKeysWarning [|
                                        (
                                            (sources.Length |> PositiveInteger.ofInt)
                                            |> Option.map
                                                (fun count ->
                                                    let __parentFQN = Some "LibClient.Components.Carousel"
                                                    let __currClass = (System.String.Format("{0}{1}{2}", "carousel ", (screenSize.Class), ""))
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    LibClient.Components.Constructors.LC.Carousel(
                                                        requestFocusOnMount = (true),
                                                        initialIndex = (props.Parameters.InitialIndex),
                                                        count = (count),
                                                        slide =
                                                            (fun (index) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        let __parentFQN = Some "ReactXP.Components.Image"
                                                                        let __currClass = (System.String.Format("{0}{1}{2}", "image ", (screenSize.Class), ""))
                                                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                        ReactXP.Components.Constructors.RX.Image(
                                                                            size = (ReactXP.Components.Image.FromStyles),
                                                                            resizeMode = (props.Parameters.ResizeMode),
                                                                            source = (sources.[index]),
                                                                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Image" __currStyles |> Some) else None)
                                                                        )
                                                                    |])
                                                            ),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                )
                                            |> Option.getOrElse noElement
                                        )
                                    |])
                            )
                            let __parentFQN = Some "LibClient.Components.Responsive"
                            LibClient.Components.Constructors.LC.Responsive(
                                desktop =
                                    (fun (_) ->
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "LibClient.Components.Dialog.Shell.WhiteRounded.Base"
                                                LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded.Base(
                                                    canClose = (LibClient.Components.Dialog.Shell.WhiteRounded.Base.When ([LibClient.Components.Dialog.Shell.WhiteRounded.Base.OnEscape; LibClient.Components.Dialog.Shell.WhiteRounded.Base.OnBackground; LibClient.Components.Dialog.Shell.WhiteRounded.Base.OnCloseButton], actions.TryCancel)),
                                                    children =
                                                        [|
                                                            carousel
                                                        |]
                                                )
                                            |])
                                    ),
                                handheld =
                                    (fun (_) ->
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "LibClient.Components.Dialog.Base"
                                                LibClient.Components.Constructors.LC.Dialog.Base(
                                                    canClose = (LibClient.Components.Dialog.Base.When ([LibClient.Components.Dialog.Base.OnEscape; LibClient.Components.Dialog.Base.OnBackground], actions.TryCancel)),
                                                    contentPosition = (LibClient.Components.Dialog.Base.ContentPosition.Center),
                                                    children =
                                                        [|
                                                            carousel
                                                            let __parentFQN = Some "LibClient.Components.IconButton"
                                                            let __currClass = "close-button"
                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                            LibClient.Components.Constructors.LC.IconButton(
                                                                state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LibClient.Components.IconButton.Actionable actions.TryCancel)),
                                                                icon = (Icon.X),
                                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                            )
                                                        |]
                                                )
                                            |])
                                    )
                            )
                        |])
                )
        )
    )

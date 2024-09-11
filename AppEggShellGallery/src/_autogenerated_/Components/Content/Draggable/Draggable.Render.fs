module AppEggShellGallery.Components.Content.DraggableRender

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

open AppEggShellGallery.Components.Content.Draggable



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Draggable.Props, estate: AppEggShellGallery.Components.Content.Draggable.Estate, pstate: AppEggShellGallery.Components.Content.Draggable.Pstate, actions: AppEggShellGallery.Components.Content.Draggable.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Draggable"),
        displayName = ("Draggable"),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                <LC.With.Ref rt-fs='true' rt-with='bindRef, maybeRef: Option&lt;Draggable.IDraggableRef&gt;'>
                    <LC.Draggable
                     Left='{| ForwardThreshold = 50; Offset = 100; BackwardThreshold = 20 |}'
                     Right='{| ForwardThreshold = 100; Offset = 200; BackwardThreshold = 20 |}'
                     Up='{| ForwardThreshold = 20; Offset = 100; BackwardThreshold = 20 |}'
                     Down='{| ForwardThreshold = 20; Offset = 200; BackwardThreshold = 20 |}'
                     BaseOffset='(50, 20)'
                     ref='bindRef'>
                        <LC.ImageCard Source='localImage ""/images/wlop4.jpg""'/>
                    </LC.Draggable>

                    <LC.Buttons rt-fs='true'>
                        <LC.Button Label='""Move Left""' State='^LowLevel (maybeRef |> Option.map (fun ref -> ~Actionable (fun _ -> ref.SetPosition LibClient.Components.Draggable.Position.Left |> ignore)) |> Option.getOrElse ~Disabled)'/>
                        <LC.Button Label='""Reset""' State='^LowLevel (maybeRef |> Option.map (fun ref -> ~Actionable (fun _ -> ref.SetPosition LibClient.Components.Draggable.Position.Base |> ignore)) |> Option.getOrElse ~Disabled)'/>
                        <LC.Button Label='""Move Right""' State='^LowLevel (maybeRef |> Option.map (fun ref -> ~Actionable (fun _ -> ref.SetPosition LibClient.Components.Draggable.Position.Right |> ignore)) |> Option.getOrElse ~Disabled)'/>
                    </LC.Buttons>
                </LC.With.Ref>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.With.Ref"
                                    LibClient.Components.Constructors.LC.With.Ref(
                                        ``with`` =
                                            (fun (bindRef, maybeRef: Option<Draggable.IDraggableRef>) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.Draggable"
                                                        LibClient.Components.Constructors.LC.Draggable(
                                                            ref = (bindRef),
                                                            baseOffset = ((50, 20)),
                                                            down = ({| ForwardThreshold = 20; Offset = 200; BackwardThreshold = 20 |}),
                                                            up = ({| ForwardThreshold = 20; Offset = 100; BackwardThreshold = 20 |}),
                                                            right = ({| ForwardThreshold = 100; Offset = 200; BackwardThreshold = 20 |}),
                                                            left = ({| ForwardThreshold = 50; Offset = 100; BackwardThreshold = 20 |}),
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.ImageCard"
                                                                    LibClient.Components.Constructors.LC.ImageCard(
                                                                        source = (localImage "/images/wlop4.jpg")
                                                                    )
                                                                |]
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.Buttons"
                                                        LibClient.Components.Constructors.LC.Buttons(
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                                    LibClient.Components.Constructors.LC.Button(
                                                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (maybeRef |> Option.map (fun ref -> LibClient.Components.Button.Actionable (fun _ -> ref.SetPosition LibClient.Components.Draggable.Position.Left |> ignore)) |> Option.getOrElse LibClient.Components.Button.Disabled)),
                                                                        label = ("Move Left")
                                                                    )
                                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                                    LibClient.Components.Constructors.LC.Button(
                                                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (maybeRef |> Option.map (fun ref -> LibClient.Components.Button.Actionable (fun _ -> ref.SetPosition LibClient.Components.Draggable.Position.Base |> ignore)) |> Option.getOrElse LibClient.Components.Button.Disabled)),
                                                                        label = ("Reset")
                                                                    )
                                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                                    LibClient.Components.Constructors.LC.Button(
                                                                        state = (LibClient.Components.Button.PropStateFactory.MakeLowLevel (maybeRef |> Option.map (fun ref -> LibClient.Components.Button.Actionable (fun _ -> ref.SetPosition LibClient.Components.Draggable.Position.Right |> ignore)) |> Option.getOrElse LibClient.Components.Button.Disabled)),
                                                                        label = ("Move Right")
                                                                    )
                                                                |]
                                                        )
                                                    |])
                                            )
                                    )
                                |])
                    )
                |])
    )

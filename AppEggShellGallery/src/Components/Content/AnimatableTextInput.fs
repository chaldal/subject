[<AutoOpen>]
module AppEggShellGallery.Components.Content_AnimatableTextInput

open System
open Fable.React
open LibClient
open LibClient.Components
open ReactXP.Components
open ReactXP.Styles
open ReactXP.Styles.Animation

[<RequireQualifiedAccess>]
module private Styles =
    let basic (fontSize: AnimatedValue) =
        makeAnimatableTextInputStyles {
            color Color.DevRed
            animatedFontSize (AnimatableValue.Value fontSize)
        }

type private Helpers =
    [<Component>]
    static member Basic() : ReactElement =
        let min = 8.0
        let max = 32.0
        let isAnimatedRef = Hooks.useRef false
        let animatedValue = Hooks.useRef (AnimatedValue.Create max)

        let animate (_: ReactEvent.Action) =
            let animateTo =
                if isAnimatedRef.current then
                    max
                else
                    min

            let animation = Animation.Timing(animatedValue.current, animateTo, TimeSpan.FromSeconds 1)
            animation.Start(fun () -> isAnimatedRef.current <- not isAnimatedRef.current)

        let textState = Hooks.useState "My name"

        LC.Column(
            children =
                elements {
                    RX.AnimatableTextInput(
                        value = textState.current,
                        placeholder = "Name",
                        onChangeText = textState.update,
                        styles = [| Styles.basic animatedValue.current |]
                    )

                    LC.Button(
                        label = "Animate",
                        state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable animate)
                    )
                },
            gap = 12
        )

type Ui.Content with
    [<Component>]
    static member AnimatableTextInput () : ReactElement =
        Ui.ComponentContent (
            displayName = "AnimatableTextInput",
            isResponsive = false,
            samples = (
                element {
                    Ui.ComponentSampleGroup(
                        samples = (
                            element {
                                Ui.ComponentSample(
                                    heading = "Basic",
                                    visuals = Helpers.Basic(),
                                    code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text "")
                                )
                            }
                        )
                    )
                }
            )
        )

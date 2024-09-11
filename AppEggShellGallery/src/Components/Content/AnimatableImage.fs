[<AutoOpen>]
module AppEggShellGallery.Components.Content_AnimatableImage

open System
open Fable.React
open LibClient
open LibClient.Components
open ReactXP.Components
open ReactXP.Styles
open ReactXP.Styles.Animation
open AppEggShellGallery.LocalImages

[<RequireQualifiedAccess>]
module private Styles =
    let scale (scale: AnimatableValue) =
        makeAnimatableViewStyles {
            width 200
            height 100
            AlignSelf.Center
            animatedTransform
                [
                    [ animatedScale scale ]
                ]
        }

type private Helpers =
    [<Component>]
    static member private Image(scale: AnimatableValue) : ReactElement =
        RX.AnimatableImage(
            (localImage "/images/wlop2.jpg").Url,
            styles = [| Styles.scale scale |]
        )

    [<Component>]
    static member Basic() : ReactElement =
        let min = 0.1
        let max = 1.0
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

        LC.Column(
            children =
                elements {
                    Helpers.Image(AnimatableValue.Value animatedValue.current)

                    LC.Button(
                        label = "Animate",
                        state = ButtonHighLevelState.LowLevel (ButtonLowLevelState.Actionable animate)
                    )
                },
            gap = 12
        )

type Ui.Content with
    [<Component>]
    static member AnimatableImage () : ReactElement =
        Ui.ComponentContent (
            displayName = "AnimatableImage",
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

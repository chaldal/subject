[<AutoOpen>]
module AppEggShellGallery.Components.Content_TouchableOpacity

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
    let view = makeViewStyles {
        backgroundColor (Color.Grey "cc")
        size 300 200
    }
    
    let text =
        makeTextStyles {
            fontSize 10
            TextAlign.Center
        }


type private Helpers =
    [<Component>]
    static member TouchableOpacityOnView() : ReactElement =
        let isButtonPressed = Hooks.useState false
        
        LC.TouchableOpacity(
            onPress = (fun _ -> isButtonPressed.update true),
            children = [|
                RX.View (
                    styles = [|Styles.view|],
                    children = [|
                        RX.Text "Click Me"
                |])
            |]
        )


    

type Ui.Content with
    [<Component>]
    static member TouchableOpacity () : ReactElement =
        Ui.ComponentContent (
            displayName = "TouchableOpacity",
            isResponsive = false,
            samples = (
                element {
                    Ui.ComponentSampleGroup(
                        samples = (
                            element {
                                Ui.ComponentSample(
                                    heading = "Basic Touchable opacity",
                                    visuals = Helpers.TouchableOpacityOnView(),
                                    code = ComponentSample.SingleBlock (
                                        ComponentSample.Fsharp,
                                        RX.Text( "
LC.TouchableOpacity(
    onPress = (fun _ -> isButtonPressed.update true),
    children = [|
        RX.View (
            styles = [|Styles.view|],
            children = [|
                RX.Text \"Click Me\"
        |])
    |]
)
                                            "
                                        )
                                    )
                                )
                            }
                        )
                    )
                }
            )
        )

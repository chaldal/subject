[<AutoOpen>]
module LibClient.Components.TouchableOpacity

open Fable.React

open LibClient

open ReactXP.Styles
open ReactXP.Components

[<RequireQualifiedAccess>]
module private Styles =
    let container =
        makeViewStyles {
            padding 15
            margin -15
        }
    let onPressOpacity =
        makeViewStyles {
            opacity 0.6
        }

type LibClient.Components.Constructors.LC with
    static member TouchableOpacity (
            children: ReactElements,
            onPress: ReactEvent.Action -> unit,
            ?onHoverStart: Browser.Types.PointerEvent -> unit,
            ?onHoverEnd: Browser.Types.PointerEvent -> unit,
            ?onPressIn: Browser.Types.PointerEvent -> unit,
            ?onPressOut: Browser.Types.PointerEvent -> unit,
            ?pointerState: LC.Pointer.State.PointerState,
            ?styles: array<ViewStyles>,
            ?key: string) : ReactElement =
        
        let isPressed: IStateHook<bool> = Hooks.useState false
        
        let TouchableOpacityOnPressIn = fun e ->
            isPressed.update true
            onPressIn |> Option.sideEffect (fun pressIn -> pressIn e)
            
        let TouchableOpacityOnPressOut = fun e ->
            isPressed.update false
            onPressOut |> Option.sideEffect (fun pressOut -> pressOut e)
                
        
        RX.View(
            styles = [| Styles.container; if isPressed.current = true then Styles.onPressOpacity|],
            children = elements {
                children
                LC.TapCapture (
                    onPress = onPress, 
                    ?onHoverStart = onHoverStart, 
                    ?onHoverEnd = onHoverEnd, 
                    ?onPressIn = Some TouchableOpacityOnPressIn, 
                    ?onPressOut = Some TouchableOpacityOnPressOut, 
                    ?pointerState = pointerState, 
                    ?styles = styles, 
                    ?key = key
                )
            }
        )

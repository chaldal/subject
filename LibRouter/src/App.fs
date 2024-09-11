module LibRouter.App

open LibLang
open LibClient
open LibClient.Responsive
open Fable.Core.JsInterop

// this is to make TEs available
open ReactXP.Components
open LibClient.Components

type Props = (* GenerateMakeFunction *) {
    PstoreKey: string
}

type Estate = EmptyRecordType

// NOTE this class isn't in LibRouter because it naturally belongs here,
// I wanted to have it in LibClient, but its dependency on Router in the Props
// prevents me from doing so, and I don't have the cycles to make a new home for it now.
[<AbstractClass>]
type AppComponent<'Parameters, 'Result, 'Actions, 'Self>(name: string, initialProps: Props, actionsConstructor: 'Self -> 'Actions, hasStyles: bool) as this =
    inherit PureStatelessComponent<Props, 'Actions, 'Self>(name, initialProps, actionsConstructor, hasStyles)

    let styles =
        !!(createObj [
            "position" ==> "absolute"
            "top"      ==> 0
            "right"    ==> 0
            "bottom"   ==> 0
            "left"     ==> 0
        ] |> ReactXP.Helpers.ReactXPRaw?Styles?createViewStyle)

    do
        // technically we should be unmounting these, but because it's the top level app,
        // we don't care what happens when it goes away.
        addOnScreenSizeUpdatedListener (System.Action (fun () -> this.forceUpdate())) |> ignore
        LibClient.Components.TapCaptureDebugVisibility.addIsVisibleForDebugChangeListener (System.Action (fun () -> this.forceUpdate())) |> ignore

    member this.OnLayout (onLayoutEvent: ReactXP.Types.ViewOnLayoutEvent) : unit =
        screenSizeOnLayout onLayoutEvent

    override this.render () : ReactElement =
        screenSizeContextProvider
            (getLatestScreenSize())
            [|
                RX.View (
                    onLayout = this.OnLayout,
                    styles   = styles,
                    children = castAsElements (base.render())
                )
            |]

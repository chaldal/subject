[<AutoOpen>]
module LibClient.Components.Responsive

open Fable.React

open LibClient
open LibClient.Responsive

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Responsive (
        desktop:  ScreenSize -> ReactElement,
        handheld: ScreenSize -> ReactElement
    ) : ReactElement =
        LC.With.ScreenSize (fun screenSize ->
            match screenSize with
            | ScreenSize.Desktop  -> desktop screenSize
            | ScreenSize.Handheld -> handheld screenSize
        )
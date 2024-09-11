[<AutoOpen>]
module LibClient.Components.With_ScreenSize

open Fable.React
open LibClient
open LibClient.Components
open LibClient.Responsive

type LC.With with
    [<Component>]
    static member ScreenSize (``with``: LibClient.Responsive.ScreenSize -> ReactElement) : ReactElement =
        LC.With.Context(
            context = screenSizeContext,
            ``with`` = ``with``
        )

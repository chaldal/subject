namespace LibClient.Components

open LibClient
open Fable.React
open LibClient.Components.Dialog.Shell.FullScreen
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Dialog_Shell_FullScreenTypeExtensions =
    type LibClient.Components.Constructors.LC.Dialog.Shell with
        static member FullScreen(?children: ReactChildrenProp, ?backButton: BackButton, ?scroll: Scroll, ?heading: string, ?headerRight: ReactElement, ?bottomSection: ReactElement, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    BackButton = defaultArg backButton (BackButton.No)
                    Scroll = defaultArg scroll (Scroll.Vertical)
                    Heading = heading |> Option.orElse (None)
                    HeaderRight = headerRight |> Option.orElse (None)
                    BottomSection = bottomSection |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Dialog.Shell.FullScreen.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
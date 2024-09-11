namespace LibClient.Components

open LibClient
open Fable.React
open LibClient.Components.Dialog.Shell.WhiteRounded.Standard
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Dialog_Shell_WhiteRounded_StandardTypeExtensions =
    type LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded with
        static member Standard(canClose: CanClose, ?children: ReactChildrenProp, ?body: ReactElement, ?buttons: ReactElement, ?mode: Mode, ?heading: string, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    CanClose = canClose
                    Body = defaultArg body (noElement)
                    Buttons = defaultArg buttons (noElement)
                    Mode = defaultArg mode (Default)
                    Heading = heading |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Dialog.Shell.WhiteRounded.Standard.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
namespace LibClient.Components

open LibClient
open Fable.Core
open Browser.Types
open LibClient.Components.Dialog.Shell.WhiteRounded.Raw
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Dialog_Shell_WhiteRounded_RawTypeExtensions =
    type LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded with
        static member Raw(canClose: CanClose, ?children: ReactChildrenProp, ?position: DialogPosition, ?inProgress: bool, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    CanClose = canClose
                    Position = defaultArg position (Center)
                    InProgress = defaultArg inProgress (false)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Dialog.Shell.WhiteRounded.Raw.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
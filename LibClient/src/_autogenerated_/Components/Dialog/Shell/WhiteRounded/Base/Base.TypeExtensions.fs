namespace LibClient.Components

open LibClient
open LibClient.Components.Dialog.Shell.WhiteRounded.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Dialog_Shell_WhiteRounded_BaseTypeExtensions =
    type LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded with
        static member Base(canClose: CanClose, ?children: ReactChildrenProp, ?inProgress: bool, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    CanClose = canClose
                    InProgress = defaultArg inProgress (false)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Dialog.Shell.WhiteRounded.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
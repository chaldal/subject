namespace LibClient.Components

open LibClient
open Fable.Core
open Browser.Types
open LibClient.Components.Dialog.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Dialog_BaseTypeExtensions =
    type LibClient.Components.Constructors.LC.Dialog with
        static member Base(canClose: CanClose, contentPosition: ContentPosition, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    CanClose = canClose
                    ContentPosition = contentPosition
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Dialog.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
namespace LibClient.Components

open LibClient
open LibClient.Components
open LibClient.JsInterop
open LibClient.Components.Legacy.Input.PositiveInteger
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_Input_PositiveIntegerTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy.Input with
        static member PositiveInteger(onChange: Result<Positive.PositiveInteger, InputValidationError> -> unit, ?children: ReactChildrenProp, ?initialValue: Positive.PositiveInteger, ?onKeyPress: (Browser.Types.KeyboardEvent -> unit), ?ref: (LibClient.JsInterop.JsNullable<InputPositiveIntegerRef> -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    OnChange = onChange
                    InitialValue = initialValue |> Option.orElse (None)
                    OnKeyPress = onKeyPress |> Option.orElse (JsUndefined)
                    ref = ref |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.Input.PositiveInteger.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
namespace LibClient.Components

open LibClient
open LibClient.Components.LabelledFormField
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module LabelledFormFieldTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member LabelledFormField(label: string, ?children: ReactChildrenProp, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Label = label
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.LabelledFormField.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
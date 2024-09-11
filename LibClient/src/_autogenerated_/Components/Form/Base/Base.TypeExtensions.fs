namespace LibClient.Components

open LibClient
open LibClient.Components.Form.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Form_BaseTypeExtensions =
    type LibClient.Components.Constructors.LC.Form with
        static member Base(accumulator: Accumulator<'Acc>, submit: 'Acced -> ReactEvent.Action -> UDAction, content: FormHandle<'Field, 'Acc, 'Acced> -> ReactElement, ?children: ReactChildrenProp, ?initializeAccOnSubmit: bool, ?executor: MakeExecutor, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Accumulator = accumulator
                    Submit = submit
                    Content = content
                    InitializeAccOnSubmit = defaultArg initializeAccOnSubmit (false)
                    Executor = executor |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Form.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
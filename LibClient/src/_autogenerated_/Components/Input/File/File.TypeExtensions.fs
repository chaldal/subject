namespace LibClient.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open LibLifeCycleTypes.File
open LibClient.Components.Input.File
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_FileTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member File(value: list<LibLifeCycleFile>, validity: InputValidity, onChange: Result<list<LibLifeCycleFile>, string> -> unit, ?children: ReactChildrenProp, ?acceptedTypes: Set<AcceptedType>, ?selectionMode: SelectionMode, ?maxFileCount: Positive.PositiveInteger, ?maxFileSize: int<KB>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    Validity = validity
                    OnChange = onChange
                    AcceptedTypes = defaultArg acceptedTypes (Set.empty)
                    SelectionMode = defaultArg selectionMode (ReplacedExisting)
                    MaxFileCount = maxFileCount |> Option.orElse (None)
                    MaxFileSize = maxFileSize |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.File.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
namespace LibClient.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open LibLifeCycleTypes.File
open LibClient.Components.Input.NamedFile
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Input_NamedFileTypeExtensions =
    type LibClient.Components.Constructors.LC.Input with
        static member NamedFile(value: list<LibLifeCycleNamedFile>, validity: InputValidity, onChange: Result<list<LibLifeCycleNamedFile>, string> -> unit, ?children: ReactChildrenProp, ?acceptedTypes: Set<AcceptedType>, ?selectionMode: SelectionMode, ?maxFileCount: Positive.PositiveInteger, ?maxFileSize: int<KB>, ?maxTotalFileSize: int<KB>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    Validity = validity
                    OnChange = onChange
                    AcceptedTypes = defaultArg acceptedTypes (Set.empty)
                    SelectionMode = defaultArg selectionMode (ReplacedExisting)
                    MaxFileCount = maxFileCount |> Option.orElse (None)
                    MaxFileSize = maxFileSize |> Option.orElse (None)
                    MaxTotalFileSize = maxTotalFileSize |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Input.NamedFile.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
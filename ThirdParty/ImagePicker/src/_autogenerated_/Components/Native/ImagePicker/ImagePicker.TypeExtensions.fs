namespace ThirdParty.ImagePicker.Components

open LibClient
open LibLifeCycleTypes.File
open Fable.Core
open ThirdParty.ImagePicker.Components.ReactNativeImagePicker
open ReactXP.Styles
open ThirdParty.ImagePicker.Components.Native.ImagePicker
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Native_ImagePickerTypeExtensions =
    type ThirdParty.ImagePicker.Components.Constructors.ImagePicker.Native with
        static member ImagePicker(value: list<File>, validity: InputValidity, onChange: Result<list<File>, string> -> unit, ?children: ReactChildrenProp, ?showPreview: bool, ?selectionMode: SelectionMode, ?maxFileCount: Positive.PositiveInteger, ?maxFileSize: int<KB>, ?styles: array<ViewStyles>, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Value = value
                    Validity = validity
                    OnChange = onChange
                    ShowPreview = defaultArg showPreview (true)
                    SelectionMode = defaultArg selectionMode (ReplacedExisting)
                    MaxFileCount = maxFileCount |> Option.orElse (None)
                    MaxFileSize = maxFileSize |> Option.orElse (None)
                    styles = styles |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            ThirdParty.ImagePicker.Components.Native.ImagePicker.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
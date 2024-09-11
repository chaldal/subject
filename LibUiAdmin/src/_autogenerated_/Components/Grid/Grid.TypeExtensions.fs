namespace LibUiAdmin.Components

open LibClient
open Fable.React
open LibUiAdmin.Components.Grid
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module GridTypeExtensions =
    type LibUiAdmin.Components.Constructors.UiAdmin with
        static member Grid(input: Input<'T>, ?children: ReactChildrenProp, ?pageSizeChoices: List<PositiveInteger>, ?handleVerticalScrolling: bool, ?headers: ReactElement, ?headersRaw: ReactElement, ?itemKey: ('T -> string), ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Input = input
                    PageSizeChoices = defaultArg pageSizeChoices (([10; 20; 50; 100] |> List.map PositiveInteger.ofLiteral))
                    HandleVerticalScrolling = defaultArg handleVerticalScrolling (false)
                    Headers = headers |> Option.orElse (None)
                    HeadersRaw = headersRaw |> Option.orElse (None)
                    ItemKey = itemKey |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiAdmin.Components.Grid.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
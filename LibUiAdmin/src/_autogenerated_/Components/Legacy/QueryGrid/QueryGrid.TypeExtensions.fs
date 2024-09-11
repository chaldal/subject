namespace LibUiAdmin.Components

open LibClient
open LibClient.Components.Form.Base.Types
open LibClient.Services.Subscription
open LibUiAdmin
open LibUiAdmin.Components.Legacy.QueryGrid
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_QueryGridTypeExtensions =
    type LibUiAdmin.Components.Constructors.UiAdmin.Legacy with
        static member QueryGrid(headers: ReactElement, makeRow: ('Item * (* lastRequestQuery *) Option<'Query> * (* refresh *) (unit -> unit)) -> ReactElement, initialQueryAcc: 'QueryAcc, queryForm: FormHandle<'QueryFormField, 'QueryAcc, 'Query> -> ReactElement, mode: Mode<'Item, 'Query>, ?children: ReactChildrenProp, ?pageSizeChoices: List<PositiveInteger>, ?initialPageSize: PositiveInteger, ?shouldSubmitOnMountIfValid: bool, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Headers = headers
                    MakeRow = makeRow
                    InitialQueryAcc = initialQueryAcc
                    QueryForm = queryForm
                    Mode = mode
                    PageSizeChoices = defaultArg pageSizeChoices (([10; 20; 50; 100] |> List.map PositiveInteger.ofLiteral))
                    InitialPageSize = defaultArg initialPageSize (PositiveInteger.ofLiteral 10)
                    ShouldSubmitOnMountIfValid = defaultArg shouldSubmitOnMountIfValid (false)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiAdmin.Components.Legacy.QueryGrid.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
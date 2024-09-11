namespace LibUiAdmin.Components

open LibClient
open LibClient.Components.Form.Base.Types
open LibClient.Services.Subscription
open LibUiAdmin
open LibUiAdmin.Components.QueryGrid
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module QueryGridTypeExtensions =
    type LibUiAdmin.Components.Constructors.UiAdmin with
        static member QueryGrid(mode: Mode<'Item, 'Query>, page: Page<'Query>, onPageChange: Page<'Query> -> unit, initialQueryAcc: 'QueryAcc, headers: ReactElement, row: 'Item * (* currQueryPage *) Option<QueryPage<'Query>> * (* refresh *) (unit -> unit) -> ReactElement, ?children: ReactChildrenProp, ?handheldRow: ('Item * (* currQueryPage *) Option<QueryPage<'Query>> * (* refresh *) (unit -> unit) -> ReactElement), ?queryForm: FormHandle<'QueryFormField, 'QueryAcc, 'Query> -> ReactElement, ?customRender: ((* Form *) ReactElement * (* Grid *) ReactElement -> ReactElement), ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Mode = mode
                    Page = page
                    OnPageChange = onPageChange
                    InitialQueryAcc = initialQueryAcc
                    Headers = headers
                    Row = row
                    HandheldRow = handheldRow |> Option.orElse (None)
                    QueryForm = queryForm |> Option.orElse (None)
                    CustomRender = customRender |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiAdmin.Components.QueryGrid.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            
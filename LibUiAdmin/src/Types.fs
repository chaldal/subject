[<AutoOpen>]
module LibUiAdmin.Types

type ItemsMaybeWithTotalCount<'Item> = {
    Items:           seq<'Item>
    MaybeTotalCount: Option<uint64>
}

module ItemsMaybeWithTotalCount =
    let withoutTotalCount (items: seq<'Item>) : ItemsMaybeWithTotalCount<'Item> = {
        Items           = items
        MaybeTotalCount = None
    }

    (*
    let withTotalCount (items: seq<'Item>) : ItemsMaybeWithTotalCount<'Item> = {
        Items           = items
        MaybeTotalCount = if items |> Seq.isEmpty then None else items |> Seq.length |> uint64 |> Some
    }
    *)

    let withTotalCount (maybeTotalCount: Option<uint64>) (items: seq<'Item>) : ItemsMaybeWithTotalCount<'Item> = {
        Items           = items
        MaybeTotalCount = maybeTotalCount
    }

    let items (input: ItemsMaybeWithTotalCount<'Item>) : seq<'Item> = input.Items

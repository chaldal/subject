[<AutoOpen>]
module LibClient.Output

type [<RequireQualifiedAccess>] SortDirection =
| Ascending
| Descending
with
    member this.Opposite: SortDirection =
        match this with
        | SortDirection.Ascending  -> SortDirection.Descending
        | SortDirection.Descending -> SortDirection.Ascending


[<RequireQualifiedAccess>]
type Badge =
| Count of int
| Text  of string
| NoContent

[<RequireQualifiedAccess>]
type HorizontalAlignment =
| Left
| Right
| Center

type Layout = {
    // not sure what x and y on ViewOnLayoutEvent are, so until we need them, not adding them
    Width:  int
    Height: int
}

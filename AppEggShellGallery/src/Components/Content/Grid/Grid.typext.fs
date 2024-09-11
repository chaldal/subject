module AppEggShellGallery.Components.Content.Grid

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    CurrentPage: LibUiAdmin.Components.Grid.PaginatedGridData<string>
}

type RowData = string * string * string * int
let fruit: seq<RowData> = Seq.ofList [
    ("Mango", "Orange", "Sweet",          15)
    ("Kiwi",  "Green",  "Sweet and sour", 12)
    ("Lemon", "Yellow", "Sour",           8)
    ("Apple", "Green",  "Sweet",          11)
]

let words = [
    "accoutrements"
    "acumen"
    "anomalistic"
    "auspicious"
    "bellwether"
    "callipygian"
    "circumlocution"
    "concupiscent"
    "conviviality"
    "coruscant"
    "cuddlesome"
    "cupidity"
    "cynosure"
    "ebullient"
    "equanimity"
    "excogitate"
    "gasconading"
    "idiosyncratic"
    "luminescent"
    "magnanimous"
    "nidificate"
    "osculator"
    "parsimonious"
    "penultimate"
    "perfidiousness"
    "perspicacious"
    "proficuous"
    "remunerative"
    "saxicolous"
    "sesquipedalian"
    "superabundant"
    "unencumbered"
    "unparagoned"
    "usufruct"
    "winebibber"
]

let uniqueCharacterCount (s: string) : int =
    s.ToCharArray() |> Set.ofSeq |> Set.count

let skipAtMost (n: int) (source: list<'T>) : list<'T> =
    try
        source |> List.skip n
    with
    | _ -> List.empty

let takeAtMost (n: int) (source: list<'T>) : list<'T> =
    try
        source |> List.take n
    with
    | _ -> source

type Grid(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Grid>("AppEggShellGallery.Components.Content.Grid", _initialProps, Actions, hasStyles = true)

    member this.GoToPage (pageSize: PositiveInteger) (pageNumber: PositiveInteger) (_e: Option<ReactEvent.Action>) : unit =
        this.SetEstate (fun estate _ ->
            let updatedPage: LibUiAdmin.Components.Grid.PaginatedGridData<string> = {
                PageNumber          = pageNumber
                PageSize            = pageSize
                MaybePageCount      = None
                Items               = Available (words |> skipAtMost ((pageNumber.Value - 1) * pageSize.Value) |> takeAtMost pageSize.Value |> Seq.ofList)
                GoToPage            = this.GoToPage
                MaybeTotalItemCount = None
            }

            { estate with CurrentPage = updatedPage }
        )

    override this.GetInitialEstate(_initialProps: Props) : Estate = {
        CurrentPage = {
            PageNumber          = PositiveInteger.ofLiteral 1
            PageSize            = PositiveInteger.ofLiteral 5
            MaybePageCount      = None
            Items               = Available (words |> takeAtMost 5 |> Seq.ofList)
            GoToPage            = this.GoToPage
            MaybeTotalItemCount = None
        }
    }

and Actions(_this: Grid) =
    class end

let Make = makeConstructor<Grid, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

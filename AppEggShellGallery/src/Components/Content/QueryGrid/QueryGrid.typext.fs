module AppEggShellGallery.Components.Content.QueryGrid

open LibClient
open LibClient.Components.Form.Base.Types

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    SomeValue: int
}

// This is run-of-the-mill form defition: a DU of fields, the result type that
// a successfully validated form produces (Query), and an accumulator (Acc) that
// implements AbstractAcc (from LibClient.Components.Form.Base.Types).
// (you can stub this out in VSCode using the `formacc` snippet)
[<RequireQualifiedAccess>]
type Field =
| Substring
| MinLength

type Query = {
    Substring: string
    MinLength: Option<PositiveInteger>
} with
    // this predicate is the only non-standard thing here, it's a helper
    // method that we use inside the ExecuteQuery method below
    member this.Predicate (candidate: string) : bool =
        if candidate.Contains this.Substring then
            match this.MinLength with
            | Some minLength -> candidate.Length >= minLength.Value
            | None           -> true
        else
            false

type Acc = {
    Substring: Option<NonemptyString>
    MinLength: LibClient.Components.Input.PositiveInteger.Value
} with
    static member Empty : Acc = {
        Substring  = None
        MinLength  = LibClient.Components.Input.PositiveInteger.empty
    }

    interface AbstractAcc<Field, Query> with
        member this.Validate () : Result<Query, ValidationErrors<Field>> = resultful {
            let! minLength = Forms.GetOptionalFieldValue (Field.MinLength, this.MinLength.Result)

            return {
                Substring = this.Substring |> NonemptyString.optionToString
                MinLength = minLength
            }
        }

let uniqueCharacterCount (s: string) : int =
    s.ToCharArray() |> Set.ofSeq |> Set.count

type QueryGrid(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, QueryGrid>("AppEggShellGallery.Components.Content.QueryGrid", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        SomeValue = 42
    }

and Actions(_this: QueryGrid) =
    // This code is essentially emulating a backend server that processes the query
    member _.ExecuteQuery (query: Query) (pageSize: PositiveInteger) (pageNumber: PositiveInteger) (_order: LibUiAdmin.Components.Legacy.QueryGrid.Order) : Async<AsyncData<seq<string>>> =
        async {
            do! Async.Sleep 1000

            return
                AppEggShellGallery.Components.Content.Grid.words
                |> List.filter query.Predicate
                |> AppEggShellGallery.Components.Content.Grid.skipAtMost ((pageNumber.Value - 1) * pageSize.Value)
                |> AppEggShellGallery.Components.Content.Grid.takeAtMost pageSize.Value
                |> Seq.ofList
                |> Available
        }


let Make = makeConstructor<QueryGrid, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

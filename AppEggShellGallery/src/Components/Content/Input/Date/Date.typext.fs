module AppEggShellGallery.Components.Content.Input.Date

open System
open LibClient
open LibClient.Components

type DateOnly = LC.Input.DateTypes.DateOnly

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LC.Input.DateTypes.Value>
}

let canSelectDate (date: DateOnly) =
    date.DayOfWeek <> DayOfWeek.Saturday && date.DayOfWeek <> DayOfWeek.Sunday

type Date(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Date>("AppEggShellGallery.Components.Content.Input.Date", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [
            ("A", DateTimeOffset.UtcNow.ToString("dd/MM/yyyy"))
            ("B", DateTimeOffset.UtcNow.ToString("dd/MM/yyyy"))
            ("C", "invalid")
        ] |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LC.Input.DateTypes.parse)
    }

and Actions(this: Date) =
    member _.OnChange (key: string) (value: LC.Input.DateTypes.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<Date, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

module AppEggShellGallery.Components.Content.DateSelector

open LibClient

type DateOnly = LibClient.Components.DateSelector.DateOnly

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    MaybeSelectedDate: Map<string, Option<DateOnly>>
}

type DateSelector(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, DateSelector>("AppEggShellGallery.Components.Content.DateSelector", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        MaybeSelectedDate = Map.ofList [
            ("A", None)
            ("B", None)
        ]
    }

and Actions(this: DateSelector) =
    member _.OnChange (key: string) (selectedDate: DateOnly) : unit =
        this.SetEstate (fun estate _ ->
            { estate with MaybeSelectedDate = estate.MaybeSelectedDate.AddOrUpdate (key, Some selectedDate) }
        )

let Make = makeConstructor<DateSelector, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

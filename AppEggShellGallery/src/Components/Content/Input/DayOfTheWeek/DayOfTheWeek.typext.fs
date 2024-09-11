module AppEggShellGallery.Components.Content.Input.DayOfTheWeek

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    MaybeOne: Option<DayOfTheWeek>
    One:      Option<DayOfTheWeek>
    Set:      Set<DayOfTheWeek>
}

type DayOfTheWeek(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, DayOfTheWeek>("AppEggShellGallery.Components.Content.Input.DayOfTheWeek", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        MaybeOne = None
        One      = None
        Set      = Set.empty
    }

and Actions(this: DayOfTheWeek) =
    member _.SetValue (value: Option<DateTimeExtensions.DayOfTheWeek>) : unit =
        this.SetEstate (fun estate _ -> { estate with MaybeOne = value })

    member _.SetValue (value: DateTimeExtensions.DayOfTheWeek) : unit =
        this.SetEstate (fun estate _ -> { estate with One = Some value })

    member _.SetValue (value: Set<DateTimeExtensions.DayOfTheWeek>) : unit =
        this.SetEstate (fun estate _ -> { estate with Set = value })

let Make = makeConstructor<DayOfTheWeek, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

module LibClient.Components.Input.DayOfTheWeek

open LibClient

type Mode =
| MaybeOne of Value: Option<DayOfTheWeek> * OnChange: (Option<DayOfTheWeek> -> unit)
| One      of Value: Option<DayOfTheWeek> * OnChange: (DayOfTheWeek         -> unit)
| Set      of Value: Set<DayOfTheWeek>    * OnChange: (Set<DayOfTheWeek>    -> unit)
with
    member this.IsSelected (candidate: DayOfTheWeek) : bool =
        match this with
        | MaybeOne (Some day,  _) -> day = candidate
        | One      (Some day,  _) -> day = candidate
        | Set      (days,      _) -> days.Contains candidate
        | _                       -> false

    member this.OnPress (day: DayOfTheWeek) (_e: ReactEvent.Action) : unit =
        match this with
        | MaybeOne (Some selectedDay,  onChange) when selectedDay = day -> onChange None
        | MaybeOne (_, onChange)                                        -> onChange (Some day)
        | One      (Some selectedDay, _onChange) when selectedDay = day -> Noop
        | One      (_,                 onChange)                        -> onChange day
        | Set      (days,              onChange)                        -> onChange (days.Toggle day)

let days = [
    DayOfTheWeek.Sunday
    DayOfTheWeek.Monday
    DayOfTheWeek.Tuesday
    DayOfTheWeek.Wednesday
    DayOfTheWeek.Thursday
    DayOfTheWeek.Friday
    DayOfTheWeek.Saturday
]

type Props = (* GenerateMakeFunction *) {
    Label:    string option // defaultWithAutoWrap None
    Mode:     Mode
    Validity: InputValidity

    key: string option // defaultWithAutoWrap JsUndefined
}

type DayOfTheWeek(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, DayOfTheWeek>("LibClient.Components.Input.DayOfTheWeek", _initialProps, Actions, hasStyles = true)

and Actions(_this: DayOfTheWeek) =
    class end

let Make = makeConstructor<DayOfTheWeek, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate

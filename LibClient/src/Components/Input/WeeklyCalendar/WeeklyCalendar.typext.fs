module LibClient.Components.Input.WeeklyCalendar

open System
open LibClient

type Props = (* GenerateMakeFunction *) {
    Label:    Option<string> // defaultWithAutoWrap None
    Value:    Set<Date>
    OnChange: Set<Date> -> unit
    Validity: InputValidity
    StartDate: Option<Date> // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    StartDate: Date
}

let generateWeek (today: Date) : seq<Date> =
    seq { for i in 0 .. 6 -> (Date.addDays i today) }


type WeeklyCalendar(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, WeeklyCalendar>("LibClient.Components.Input.WeeklyCalendar", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(initialProps: Props) : Estate = {
        StartDate =
            initialProps.StartDate
            |> Option.defaultWith (fun () ->
                LibClient.ServiceInstances.services().Date.GetNow |> Date.ofDateTimeOffset
            )
    }

and Actions(this: WeeklyCalendar) =
    member _.NextWeek (date: Date) : unit =
        this.SetEstate (fun estate _ -> { estate with StartDate = Date.addDays 7 date })

    member _.PreviousWeek (date: Date) : unit =
        this.SetEstate (fun estate _ -> { estate with StartDate = Date.addDays -7 date })

let Make = makeConstructor<WeeklyCalendar, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

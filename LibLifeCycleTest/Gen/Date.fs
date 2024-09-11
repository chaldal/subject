[<AutoOpen>]
module LibLifeCycleTest.GenDate

open System
open FsCheck

let genNow = Gen.constant DateTimeOffset.UtcNow // This needs to be stabilized

let genDatesTillToday : Gen<list<Date>> =
    gen {
        let! today = genNow
        let! daysToGoBack = Gen.choose (10, 15)

        return
            [ 0 .. daysToGoBack ]
            |> List.map
                (fun dayToGoBack ->
                    Date.ofDateTimeOffset (today.AddDays(float -dayToGoBack)))
            |> List.rev
    }
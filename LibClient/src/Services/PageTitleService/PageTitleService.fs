module LibClient.Services.PageTitleService

open System

type SetReceipt = SetReceipt of Guid
type private LastSetData = {
    PreviousMaybeRouteName: Option<string>
    Receipt:                SetReceipt
}

type PageTitleService (appName: string) =
    let mutable maybeRouteName:          Option<string>      = None
    let mutable maybeLastSetData:        Option<LastSetData> = None
    let mutable maybeNotificationsCount: Option<int>         = None

    let setTitle (title: string) : unit =
        ReactXP.Runtime.ifWeb (fun document ->
            document.title <- title
        )

    let updateTitle () : unit =
        let maybeNotifications =
            maybeNotificationsCount
            |> Option.map (fun nc -> "(" + nc.ToString() + ") ")
            |> Option.getOrElse ""

        let title =
            match maybeRouteName with
            | Some routeName -> maybeNotifications + routeName + " - " + appName
            | None           -> maybeNotifications + appName

        setTitle title

    member _.ResetRouteName () : unit =
        maybeRouteName   <- None
        maybeLastSetData <- None
        updateTitle()

    member _.SetRouteName (value: string) : SetReceipt =
        let receipt = Guid.NewGuid() |> SetReceipt
        maybeLastSetData <- Some { PreviousMaybeRouteName = maybeRouteName; Receipt = receipt }

        maybeRouteName <- Some value
        updateTitle()

        receipt

    // Okay, this is pretty half-assed. I'm in a rush, and not doing a particularly
    // good job here. This was meant to work with dialog titles, but it needs more work.
    // Basically we need a full stack model implemented here, but there are nontrivial
    // caveats there, so leaving it all alone for now.
    member _.RestoreIfStillTheSame (receipt: SetReceipt) : unit =
        match maybeLastSetData with
        | Some data when data.Receipt = receipt ->
            maybeRouteName <- data.PreviousMaybeRouteName
            maybeLastSetData <- None
            updateTitle()
        | _ -> Noop

    member _.SetNotificationCount (value: Option<int>) : unit =
        maybeNotificationsCount <- value
        updateTitle()
[<AutoOpen>]
module LibClient.Components.DueDateTag

open System
open Fable.React

open LibClient
open LibClient.ServiceInstances

open ReactXP.Styles

// TODO: delete after RenderDSL migration
// making ~UniDateTime.Of available with ~ sytnax to caller
type UniDateTime = LibClient.Services.DateService.UniDateTime

[<RequireQualifiedAccess>]
module private Helpers =
    type Dueness =
    | Overdue
    | DueToday
    | DueLater

    let compareDate (date: UniDateTime) (now: System.DateTimeOffset) : Dueness =
        let today = now.Date
        match (date.ToDateTimeOffset.Date) with
        | date when date = today ->
            DueToday
        | date when date < today ->
            Overdue
        | _ ->
            DueLater

[<RequireQualifiedAccess>]
module private Styles =
    let tagTheme dueOn currentDate (theme: LC.Tag.Theme): LC.Tag.Theme =
        let dueness = Helpers.compareDate dueOn currentDate

        { theme with
            Tags =
                { theme.Tags with
                    Selected =
                        { theme.Tags.Selected with
                            TextColor =
                                match dueness with
                                | Helpers.Dueness.Overdue  ->
                                    Color.White
                                | Helpers.Dueness.DueToday ->
                                    Color.White
                                | Helpers.Dueness.DueLater ->
                                    Color.Grey "70"
                            BackgroundColor =
                                match dueness with
                                | Helpers.Dueness.Overdue ->
                                    Color.Hex "#e6897d"
                                | Helpers.Dueness.DueToday ->
                                    Color.Hex "#dfba49"
                                | Helpers.Dueness.DueLater ->
                                    Color.Grey "f9"
                        }
                }
        }
    let view =
        makeViewStyles {
            paddingHV    6 3
            borderRadius 5
            Cursor.Default
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member DueDateTag(
            dueOn:   UniDateTime,
            ?format: string,
            ?offset: TimeSpan,
            ?key:    string
        ) : ReactElement =
        key |> ignore

        let format = defaultArg format "dd MM yy"

        let currentDateHook = Hooks.useState (services().Date.GetToday)

        Hooks.useEffectDisposable (
            (fun () ->
                let subscription =
                    services().Date.SubscribeToToday
                        (fun newToday -> currentDateHook.update newToday)

                { new IDisposable with
                    member _.Dispose() =
                        subscription.Off ()
                }
            ),
            [||]
        )

        LC.Tag (
            text = (
                match offset with
                | Some offset -> LibClient.Services.DateService.formatDateWithOffset format dueOn offset
                | None        -> LibClient.Services.DateService.formatDate           format dueOn 
            ),
            isSelected = true,
            theme      = Styles.tagTheme dueOn currentDateHook.current,
            styles     = [| Styles.view |]
        )
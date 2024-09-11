[<AutoOpen>]
module LibClient.Components.Timestamp

open System
open Fable.React

open LibClient
open ReactXP.Styles

module LC =
    module Timestamp =
        type UniDateTime = LibClient.Services.DateService.UniDateTime
        type PropValueFactory = LibClient.Services.DateService.UniDateTimePropFactory

        let format (format: string) (value: UniDateTime) (maybeOffset: Option<System.TimeSpan>) : string =
            match maybeOffset with
            | None        -> LibClient.Services.DateService.formatDate format value
            | Some offset -> LibClient.Services.DateService.formatDateWithOffset format value offset

// TODO: delete after RenderDSL migration
// making ~UniDateTime.Of available with ~ syntax to caller
type UniDateTime = LC.Timestamp.UniDateTime
type PropValueFactory = LC.Timestamp.PropValueFactory

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Timestamp(
            value: UniDateTime,
            ?format: string,
            ?offset: TimeSpan,
            ?selectable: bool,
            ?numberOfLines: int,
            ?styles: array<TextStyles>,
            ?key: string) : ReactElement =
        key |> ignore

        let format = defaultArg format "yyyy-MM-dd HH:mm:ss"
        let selectable = defaultArg selectable false

        let formattedValue = LC.Timestamp.format format value offset

        if selectable then
            LC.Text(
                formattedValue,
                ?styles = styles,
                ?numberOfLines = numberOfLines
            )
        else
            LC.UiText(
                formattedValue,
                ?styles = styles,
                ?numberOfLines = numberOfLines
            )
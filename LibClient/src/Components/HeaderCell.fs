[<AutoOpen>]
module LibClient.Components.HeaderCell

open Fable.React
open LibClient
open LibClient.Icons
open ReactXP.Components
open ReactXP.Styles
open LibClient.JsInterop

module LC =
    module HeaderCell =
        type Theme = {
            FontColor: Color
            FontSize: int
        }

open LC.HeaderCell

[<RequireQualifiedAccess>]
module private Styles =
    let headerCell =
        makeViewStyles {
            FlexDirection.Row
            AlignItems.FlexStart
            JustifyContent.FlexStart
        }

    let headerCellText =
        TextStyles.Memoize(
            fun (theme: Theme) ->
                makeTextStyles {
                    fontSize   18
                    flexShrink 1
                    color      theme.FontColor
                    fontSize   theme.FontSize
                }
        )

    let icon =
        makeViewStyles {
            marginLeft 4
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member HeaderCell<'T when 'T: equality>(
            label: string,
            ?sort: ( (* field *) 'T * (* currentSort *) ('T * SortDirection) * (* setSort *) ('T * SortDirection -> unit)),
            ?numberOfLines: int,
            ?styles: array<ViewStyles>,
            ?theme: Theme -> Theme,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let maybeSort (field: 'T) (currField: 'T) (currDirection: SortDirection) (setSort: 'T * SortDirection -> unit) (_e: ReactEvent.Action) : unit =
            match field = currField with
            | true  -> setSort (field, currDirection.Opposite)
            | false -> setSort (field, SortDirection.Ascending)

        let theTheme = Themes.GetMaybeUpdatedWith theme
        let numberOfLines =
            match numberOfLines with
            | Some numberOfLines -> Some numberOfLines
            | None -> Undefined

        RX.View(
            styles =
                [|
                    Styles.headerCell
                    yield! styles |> Option.defaultValue [||]
                |],
            children =
                elements {
                    LC.UiText(
                        label,
                        styles = [| Styles.headerCellText theTheme |],
                        ?numberOfLines = numberOfLines
                    )

                    match sort with
                    | Some (sortField, (currSortField, currSortDirection), setSort) ->
                        if sortField = currSortField then
                            RX.View(
                                styles = [| Styles.icon |],
                                children =
                                    elements {
                                        let iconCtor =
                                            match currSortDirection with
                                            | SortDirection.Ascending -> Icon.ArrowDown
                                            | _ -> Icon.ArrowUp

                                        iconCtor Color.DevBlue 14
                                    }
                            )

                        LC.TapCapture(
                            onPress = maybeSort sortField currSortField currSortDirection setSort
                        )
                    | None ->
                        noElement
                }
        )
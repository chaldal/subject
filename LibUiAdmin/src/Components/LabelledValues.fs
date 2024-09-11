[<AutoOpen>]
module LibUiAdmin.Components.LabelledValues

open Fable.React
open ReactXP.Styles
open ReactXP.Components
open LibClient
open LibClient.Components
open LibClient.Responsive


module UiAdmin =
    module LabelledValues =
        type Theme = {
            LabelColor:  Color
            LabelIsBold: bool
        }

open UiAdmin.LabelledValues

type LabelValuePosition =
| LabelAboveValue
| LabelBelowValue
| LabelNextToValue of MaybeLabelWidth: Option<int> * MaybeGap: Option<int>


module private Styles =
    let container (isRow: bool) = makeViewStyles {
        if isRow then
            FlexDirection.Row
            JustifyContent.SpaceBetween
    }

    let label =
        TextStyles.Memoize
            (fun (labelValuePosition: LabelValuePosition) (theme: Theme) ->
            makeTextStyles {
                color theme.LabelColor

                if theme.LabelIsBold then
                    FontWeight.Bold
                else
                    FontWeight.Normal

                match labelValuePosition with
                | LabelNextToValue (maybeWidth, maybeGap) ->
                    TextAlign.Right

                    match maybeWidth with
                    | Some theWidth -> width theWidth
                    | None          -> ()

                    match maybeGap with
                    | Some gap -> marginRight gap
                    | None     -> marginRight 50

                | _ ->
                    marginVertical 5
            }
        )

    let value = makeViewStyles {
        flex       1
        marginLeft 5
    }

type UiAdmin with
    // TODO: Move to LibClient
    [<Component>]
    static member LabelledValue (
        label:               string,
        children:            ReactChildrenProp,
        ?labelValuePosition: LabelValuePosition,
        ?theme:              Theme -> Theme,
        ?styles:             array<ViewStyles>
    ) : ReactElement =
        let theTheme = Themes.GetMaybeUpdatedWith theme

        let labelValuePosition =
            labelValuePosition
            |> Option.defaultValue LabelAboveValue

        match labelValuePosition with
        | LabelAboveValue ->
            RX.View (
                styles = [|
                    Styles.container false
                    yield! styles |> Option.defaultValue [||]
                |],
                children = [|
                    LC.Text (
                        value  = label,
                        styles = [|
                            Styles.label labelValuePosition theTheme
                        |]
                    )
                    RX.View (
                        styles = [|
                            Styles.value
                        |],
                        children = children
                    )
                |]
            )
        | LabelBelowValue ->
            RX.View (
                styles = [|
                    Styles.container false
                    yield! styles |> Option.defaultValue [||]
                |],
                children = [|
                    RX.View (
                        styles = [|
                            Styles.value
                        |],
                        children = children
                    )
                    LC.Text (
                        value  = label,
                        styles = [|
                            Styles.label labelValuePosition theTheme
                        |]
                    )
                |]
            )
        | LabelNextToValue _ ->
            RX.View (
                styles = [|
                    Styles.container true
                    yield! styles |> Option.defaultValue [||]
                |],
                children = [|
                    LC.Text (
                        value  = label,
                        styles = [|
                            Styles.label labelValuePosition theTheme
                        |]
                    )
                    RX.View (
                        styles = [|
                            Styles.value
                        |],
                        children = children
                    )
                |]
            )


    [<Component>]
    static member LabelledValues (
        labelledValues: List<string * array<ReactElement>>,
        ?styles:        ViewStyles,
        ?labelWidth:    int
    ) : ReactElement = element {
        let styles =
            styles
            |> Option.defaultValue 
                (
                    makeViewStyles {
                        paddingHV    10 13
                        borderBottom 1 (Color.Grey "cc")
                    }
                )

        let labelWidth = labelWidth |> Option.defaultValue 150

        LC.With.ScreenSize (
            fun screenSize -> element {
                labelledValues
                |> Seq.map (fun (label, value) ->
                    UiAdmin.LabelledValue (
                        label    = label,
                        children = value,
                        styles   = [| styles |],
                        theme    =
                            (fun theme ->
                                match screenSize with
                                | ScreenSize.Desktop  ->
                                    {
                                        theme with
                                            LabelColor = (Color.Grey "66")
                                            LabelIsBold = false
                                    }
                                | ScreenSize.Handheld -> theme
                            ),
                        labelValuePosition =
                            match screenSize with
                            | ScreenSize.Desktop  -> LabelValuePosition.LabelNextToValue (Some labelWidth, None)
                            | ScreenSize.Handheld -> LabelValuePosition.LabelAboveValue
                    )
                )
            }
        )
    }
[<AutoOpen>]
module LibUiAdmin.Components.Table

open Fable.React
open ReactXP.Styles
open LibClient
open LibClient.Components

module dom = Fable.React.Standard

type UiAdmin.Table with
    static member KeyValue (key: string, value: string) : ReactElement =
        UiAdmin.Table.KeyValue (key, LC.Text value)

    static member KeyValue (key: ReactElement, value: string) : ReactElement =
        UiAdmin.Table.KeyValue (key, LC.Text value)

    static member KeyValue (key: string, maybeValue: Option<string>) : ReactElement =
        UiAdmin.Table.KeyValue (key, [|LC.Text (maybeValue |> Option.getOrElse "None")|])

    static member KeyValue (key: string, maybeEmptyValues: list<'T>, render: 'T -> ReactElement) : ReactElement =
        UiAdmin.Table.KeyValue (LC.Text key, maybeEmptyValues, render)

    static member KeyValue (key: ReactElement, maybeEmptyValues: list<'T>, render: 'T -> ReactElement) : ReactElement =
        UiAdmin.Table.KeyValue (key, [|
            match maybeEmptyValues with
            | [] -> LC.Text "None"
            | values ->
                dom.table [(Props.ClassName "la-table-keyvalue")] [|
                    dom.tbody [] (values |> List.map render)
                |]
        |])

    static member KeyValue (key: string, value: ReactElements) : ReactElement =
        UiAdmin.Table.KeyValue (LC.Text key, value)

    static member KeyValue (key: string, value: ReactElement) : ReactElement =
        UiAdmin.Table.KeyValue (LC.Text key, value)

    static member KeyValue (key: ReactElement, value: ReactElement) : ReactElement =
        UiAdmin.Table.KeyValue ([key], [|value|])

    static member KeyValue (key: ReactElement, value: ReactElements) : ReactElement =
        UiAdmin.Table.KeyValue ([key], value)

    static member KeyValue (key: list<ReactElement>, value: ReactElements) : ReactElement =
        dom.tr [] [|
            dom.td [] key
            dom.td [] value
        |]

and private Styles() =
    static member val Section = makeViewStyles {
        paddingTop 100
    }
module AppEggShellGallery.Components.Content.QueryGridRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ThirdParty.Map.Components
open ReactXP.Components
open ThirdParty.Recharts.Components
open ThirdParty.Showdown.Components
open ThirdParty.SyntaxHighlighter.Components
open LibUiAdmin.Components
open AppEggShellGallery.Components

open LibLang
open LibClient
open LibClient.Services.Subscription
open LibClient.RenderHelpers
open LibClient.Chars
open LibClient.ColorModule
open LibClient.Responsive
open AppEggShellGallery.RenderHelpers
open AppEggShellGallery.Navigation
open AppEggShellGallery.LocalImages
open AppEggShellGallery.Icons
open AppEggShellGallery.AppServices
open AppEggShellGallery

open AppEggShellGallery.Components.Content.QueryGrid



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.QueryGrid.Props, estate: AppEggShellGallery.Components.Content.QueryGrid.Estate, pstate: AppEggShellGallery.Components.Content.QueryGrid.Pstate, actions: AppEggShellGallery.Components.Content.QueryGrid.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibUiAdmin.Components.QueryGrid"),
        displayName = ("QueryGrid"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "QueryGrid is a paginated Grid that is type parametrized by 'Query, taking as props a chunk of\n        UI through which the user inputs the query, and a query execution function."
                |]),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        verticalAlignment = (AppEggShellGallery.Components.ComponentSample.VerticalAlignment.Top),
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.Children
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                    children =
                                                        [|
                                                            @"
// This is run-of-the-mill form defition: a DU of fields, the result type that
// a successfully validated form produces (Query), and an accumulator (Acc) that
// implements AbstractAcc (from LibClient.Components.Form.Base.Types).
// (you can stub this out in VSCode using the `formacc` snippet)
[<RequireQualifiedAccess>]
type Field =
| Substring
| MinLength

type Query = {
    Substring: string
    MinLength: Option<PositiveInteger>
} with
    // this predicate is the only non-standard thing here, it's a helper
    // method that we use inside the ExecuteQuery method below
    member this.Predicate (candidate: string) : bool =
        if candidate.Contains this.Substring then
            match this.MinLength with
            | Some minLength -> candidate.Length >= minLength.Value
            | None           -> true
        else
            false

type Acc = {
    Substring: string
    MinLength: LibClient.Components.Input.PositiveInteger.Value
} with
    static member Empty : Acc = {
        Substring  = """"
        MinLength  = LibClient.Components.Input.PositiveInteger.empty
    }

    interface AbstractAcc<Field, Query> with
        member this.Validate () : Result<Query, ValidationErrors<Field>> = resultful {
            let! minLength = Forms.GetOptionalFieldValue (Field.MinLength, this.MinLength.Result)

            return {
                Substring = this.Substring
                MinLength = minLength
            }
        }
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                    children =
                                                        [|
                                                            @"
                // This code is essentially emulating a backend server that processes the query
                member _.ExecuteQuery (query: Query) (pageSize: PositiveInteger) (pageNumber: PositiveInteger) (order: LibUiAdmin.Components.QueryGrid.Order) : Async<AsyncData<seq<string>>> =
                    async {
                        do! Async.Sleep 1000

                        return
                            AppEggShellGallery.Components.Content.Grid.words
                            |> List.filter query.Predicate
                            |> AppEggShellGallery.Components.Content.Grid.skipAtMost ((pageNumber.Value - 1) * pageSize.Value)
                            |> AppEggShellGallery.Components.Content.Grid.takeAtMost pageSize.Value
                            |> Seq.ofList
                            |> Available
                    }
                "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                    children =
                                                        [|
                                                            @"
                <UiAdmin.Legacy.QueryGrid Mode='~OneTime actions.ExecuteQuery' InitialQueryAcc='Acc.Empty'>
                    <rt-prop name='QueryForm(form: ~FormHandle&lt;Field, Acc, Query&gt;)'>
                        <LC.Input.Text
                         Label='""Substring""'
                         Validity='form.FieldValidity Field.Substring'
                         Value='form.Acc.Substring'
                         OnChange='fun value -> form.UpdateAcc (fun acc -> { acc with Substring = value })'/>
                        <LC.Input.PositiveInteger
                         Label='""MinLength""'
                         Validity='form.FieldValidity Field.MinLength'
                         Value='form.Acc.MinLength'
                         OnChange='fun value -> form.UpdateAcc (fun acc -> { acc with MinLength = value })'/>
                    </rt-prop>

                    <rt-prop name='Headers'>
                        <dom.td><LC.HeaderCell Label='""Word""'                  /></dom.td>
                        <dom.td><LC.HeaderCell Label='""Character Count""'       /></dom.td>
                        <dom.td><LC.HeaderCell Label='""Unique Character Count""'/></dom.td>
                    </rt-prop>

                    <rt-prop name='MakeRow(word, _, _refresh)'>
                        <dom.td>{word}</dom.td>
                        <dom.td>{word.Length}</dom.td>
                        <dom.td>{uniqueCharacterCount word}</dom.td>
                    </rt-prop>
                </UiAdmin.Legacy.QueryGrid>
            "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibUiAdmin.Components.Legacy.QueryGrid"
                                    LibUiAdmin.Components.Constructors.UiAdmin.Legacy.QueryGrid(
                                        initialQueryAcc = (Acc.Empty),
                                        mode = (LibUiAdmin.Components.Legacy.QueryGrid.OneTime actions.ExecuteQuery),
                                        headers =
                                                (castAsElementAckingKeysWarning [|
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Word")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Character Count")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Unique Character Count")
                                                            )
                                                        |])
                                                |]),
                                        makeRow =
                                            (fun (word, _, _refresh) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", word))
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", word.Length))
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", uniqueCharacterCount word))
                                                            |])
                                                    |])
                                            ),
                                        queryForm =
                                            (fun (form: LibUiAdmin.Components.Legacy.QueryGrid.FormHandle<Field, Acc, Query>) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.Input.Text"
                                                        LibClient.Components.Constructors.LC.Input.Text(
                                                            onChange = (fun value -> form.UpdateAcc (fun acc -> { acc with Substring = value })),
                                                            value = (form.Acc.Substring),
                                                            validity = (form.FieldValidity Field.Substring),
                                                            label = ("Substring")
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.Input.PositiveInteger"
                                                        LibClient.Components.Constructors.LC.Input.PositiveInteger(
                                                            onChange = (fun value -> form.UpdateAcc (fun acc -> { acc with MinLength = value })),
                                                            value = (form.Acc.MinLength),
                                                            validity = (form.FieldValidity Field.MinLength),
                                                            label = ("MinLength")
                                                        )
                                                    |])
                                            )
                                    )
                                |])
                    )
                |])
    )

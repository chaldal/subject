module AppEggShellGallery.Components.Content.GridRender

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

open AppEggShellGallery.Components.Content.Grid



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Grid.Props, estate: AppEggShellGallery.Components.Content.Grid.Estate, pstate: AppEggShellGallery.Components.Content.Grid.Pstate, actions: AppEggShellGallery.Components.Content.Grid.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibUiAdmin.Components.Grid"),
        displayName = ("Grid"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "The grid is currently fairly basic, we're building it out as we go.\n Also see QueryGrid and\n        WithSortAndFilter for additional options."
                |]),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        verticalAlignment = (AppEggShellGallery.Components.ComponentSample.VerticalAlignment.Top),
                        heading = ("Dynamic asynchronous rows, paginated"),
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
                    // the props listed above don't include the actual definition of the helper types
                    type PaginatedGridData<'T> = {
                        PageNumber:     PositiveInteger
                        PageSize:       PositiveInteger
                        MaybePageCount: Option<UnsignedInteger>
                        Items:          AsyncData<seq<'T>>
                        GoToPage:       (* pageSize *) PositiveInteger -> (* pageNumber *) PositiveInteger -> Option<ReactEvent.Pointer> -> unit
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
                <UiAdmin.Grid Input='~Paginated (estate.CurrentPage, makeRow, None)'>
                    <rt-prop name='Headers'>
                        <dom.td><LC.HeaderCell Label='""Word""'                  /></dom.td>
                        <dom.td><LC.HeaderCell Label='""Character Count""'       /></dom.td>
                        <dom.td><LC.HeaderCell Label='""Unique Character Count""'/></dom.td>
                    </rt-prop>

                    <rt-outer-let name='makeRow(word: string)'>
                        <dom.td>{word}</dom.td>
                        <dom.td>{word.Length}</dom.td>
                        <dom.td>{uniqueCharacterCount word}</dom.td>
                    </rt-outer-let>
                </UiAdmin.Grid>
            "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibUiAdmin.Components.Grid"
                                    let makeRow(word: string) =
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
                                    LibUiAdmin.Components.Constructors.UiAdmin.Grid(
                                        input = (LibUiAdmin.Components.Grid.Paginated (estate.CurrentPage, makeRow, None)),
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
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        verticalAlignment = (AppEggShellGallery.Components.ComponentSample.VerticalAlignment.Top),
                        heading = ("Dynamic asynchronous rows, displayed in full"),
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
                    type RowData = string * string * string * int
                    let fruit: seq<RowData> = Seq.ofList [
                        (""Mango"", ""Orange"", ""Sweet"",          15)
                        (""Kiwi"",  ""Green"",  ""Sweet and sour"", 12)
                        (""Lemon"", ""Yellow"", ""Sour"",           8)
                        (""Apple"", ""Green"",  ""Sweet"",          11)
                    ]
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
                <UiAdmin.Grid Input='~Everything (Available fruit, makeRow, None)'>
                    <rt-prop name='Headers'>
                        <dom.td><LC.HeaderCell Label='""Name""' /></dom.td>
                        <dom.td><LC.HeaderCell Label='""Color""'/></dom.td>
                        <dom.td><LC.HeaderCell Label='""Taste""'/></dom.td>
                        <dom.td><LC.HeaderCell Label='""Price""'/></dom.td>
                    </rt-prop>

                    <rt-outer-let name='makeRow((name, color, taste, price): RowData)'>
                        <dom.td>{name}</dom.td>
                        <dom.td>{color}</dom.td>
                        <dom.td>{taste}</dom.td>
                        <dom.td>{price}</dom.td>
                    </rt-outer-let>
                </UiAdmin.Grid>
            "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibUiAdmin.Components.Grid"
                                    let makeRow((name, color, taste, price): string * string * string * int) =
                                            (castAsElementAckingKeysWarning [|
                                                FRS.td
                                                    []
                                                    ([|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", name))
                                                    |])
                                                FRS.td
                                                    []
                                                    ([|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", color))
                                                    |])
                                                FRS.td
                                                    []
                                                    ([|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", taste))
                                                    |])
                                                FRS.td
                                                    []
                                                    ([|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", price))
                                                    |])
                                            |])
                                    LibUiAdmin.Components.Constructors.UiAdmin.Grid(
                                        input = (LibUiAdmin.Components.Grid.Everything (Available fruit, makeRow, None)),
                                        headers =
                                                (castAsElementAckingKeysWarning [|
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Name")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Color")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Taste")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Price")
                                                            )
                                                        |])
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        verticalAlignment = (AppEggShellGallery.Components.ComponentSample.VerticalAlignment.Top),
                        heading = ("Static, hardcoded rows"),
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                @"
                <UiAdmin.Grid Input='~Static (rows, None)'>
                    <rt-prop name='Headers'>
                        <dom.td><LC.HeaderCell Label='""Name""' /></dom.td>
                        <dom.td><LC.HeaderCell Label='""Color""'/></dom.td>
                        <dom.td><LC.HeaderCell Label='""Taste""'/></dom.td>
                        <dom.td><LC.HeaderCell Label='""Price""'/></dom.td>
                    </rt-prop>

                    <rt-outer-let name='rows'>
                        <dom.tr>
                            <dom.td>Mango</dom.td>
                            <dom.td>Orange</dom.td>
                            <dom.td>Sweet</dom.td>
                            <dom.td>15</dom.td>
                        </dom.tr>
                        <dom.tr>
                            <dom.td>Kiwi</dom.td>
                            <dom.td>Green</dom.td>
                            <dom.td>Sweet and sour</dom.td>
                            <dom.td>12</dom.td>
                        </dom.tr>
                        <dom.tr>
                            <dom.td>Lemon</dom.td>
                            <dom.td>Yellow</dom.td>
                            <dom.td>Sour</dom.td>
                            <dom.td>8</dom.td>
                        </dom.tr>
                        <dom.tr>
                            <dom.td>Apple</dom.td>
                            <dom.td>Green</dom.td>
                            <dom.td>Sweet</dom.td>
                            <dom.td>11</dom.td>
                        </dom.tr>
                    </rt-outer-let>
                </UiAdmin.Grid>
            "
                                                |> makeTextNode2 __parentFQN
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibUiAdmin.Components.Grid"
                                    let rows =
                                            (castAsElementAckingKeysWarning [|
                                                FRS.tr
                                                    []
                                                    ([|
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Mango"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Orange"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Sweet"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "15"
                                                            |])
                                                    |])
                                                FRS.tr
                                                    []
                                                    ([|
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Kiwi"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Green"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Sweet and sour"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "12"
                                                            |])
                                                    |])
                                                FRS.tr
                                                    []
                                                    ([|
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Lemon"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Yellow"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Sour"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "8"
                                                            |])
                                                    |])
                                                FRS.tr
                                                    []
                                                    ([|
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Apple"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Green"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "Sweet"
                                                            |])
                                                        FRS.td
                                                            []
                                                            ([|
                                                                makeTextNode2 __parentFQN "11"
                                                            |])
                                                    |])
                                            |])
                                    LibUiAdmin.Components.Constructors.UiAdmin.Grid(
                                        input = (LibUiAdmin.Components.Grid.Static (rows, None)),
                                        headers =
                                                (castAsElementAckingKeysWarning [|
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Name")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Color")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Taste")
                                                            )
                                                        |])
                                                    FRS.td
                                                        []
                                                        ([|
                                                            let __parentFQN = Some "LibClient.Components.HeaderCell"
                                                            LibClient.Components.Constructors.LC.HeaderCell(
                                                                label = ("Price")
                                                            )
                                                        |])
                                                |])
                                    )
                                |])
                    )
                |])
    )

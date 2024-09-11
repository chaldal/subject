module AppEggShellGallery.Components.Route.ComponentsRender

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

open AppEggShellGallery.Components.Route.Components



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Route.Components.Props, estate: AppEggShellGallery.Components.Route.Components.Estate, pstate: AppEggShellGallery.Components.Route.Components.Pstate, actions: AppEggShellGallery.Components.Route.Components.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (castAsElementAckingKeysWarning [|
        let __parentFQN = Some "LibClient.Components.SetPageMetadata"
        LibClient.Components.Constructors.LC.SetPageMetadata(
            title = ("Components")
        )
        let __parentFQN = Some "LibRouter.Components.Route"
        LibRouter.Components.Constructors.LR.Route(
            scroll = (LibRouter.Components.Route.Vertical),
            children =
                [|
                    let __parentFQN = Some "LibClient.Components.Section.Padded"
                    LibClient.Components.Constructors.LC.Section.Padded(
                        children =
                            [|
                                match (props.Content) with
                                | Index ->
                                    [|
                                        let __parentFQN = Some "ThirdParty.Showdown.Components.MarkdownViewer"
                                        ThirdParty.Showdown.Components.Constructors.Showdown.MarkdownViewer(
                                            showdownConverter = (showdownConverterWithSyntaxHighlighting),
                                            globalLinkHandler = ("globalMarkdownLinkHandler"),
                                            source = (ThirdParty.Showdown.Components.MarkdownViewer.Url ("/docs/components/index.md" |> services().Http.PrepareInBundleResourceUrl))
                                        )
                                    |]
                                | Avatar ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Avatar"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Avatar()
                                    |]
                                | AsyncData ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.AsyncData"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.AsyncData()
                                    |]
                                | ComponentItem.Button ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Button"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Button()
                                    |]
                                | ComponentItem.Buttons ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Buttons"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Buttons()
                                    |]
                                | Card ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Card"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Card()
                                    |]
                                | Carousel ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Carousel"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Carousel()
                                    |]
                                | ComponentItem.ContextMenu ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ContextMenu"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ContextMenu()
                                    |]
                                | DateSelector ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.DateSelector"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.DateSelector()
                                    |]
                                | Dialogs ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Dialogs"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Dialogs()
                                    |]
                                | Draggable ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Draggable"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Draggable()
                                    |]
                                | ErrorBoundary ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ErrorBoundary"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ErrorBoundary()
                                    |]
                                | FloatingActionButton ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.FloatingActionButton"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.FloatingActionButton()
                                    |]
                                | Forms ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Forms"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Forms()
                                    |]
                                | Grid ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Grid"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Grid()
                                    |]
                                | ComponentItem.Heading ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Heading"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Heading()
                                    |]
                                | Icon ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Icon"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Icon()
                                    |]
                                | IconButton ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.IconButton"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.IconButton()
                                    |]
                                | IconWithBadge ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.IconWithBadge"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.IconWithBadge()
                                    |]
                                | ImageCard ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ImageCard"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ImageCard()
                                    |]
                                | InfoMessage ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.InfoMessage"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.InfoMessage()
                                    |]
                                | Input_Checkbox ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Checkbox"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Checkbox()
                                    |]
                                | Input_ChoiceList ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.ChoiceList"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.ChoiceList()
                                    |]
                                | Input_Date ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Date"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Date()
                                    |]
                                | Input_DayOfTheWeek ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.DayOfTheWeek"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.DayOfTheWeek()
                                    |]
                                | Input_Decimal ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Decimal"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Decimal()
                                    |]
                                | Input_Duration ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Duration"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Duration()
                                    |]
                                | Input_EmailAddress ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.EmailAddress"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.EmailAddress()
                                    |]
                                | Input_LocalTime ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.LocalTime"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.LocalTime()
                                    |]
                                | Input_File ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.File"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.File()
                                    |]
                                | Input_Image ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Image"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Image()
                                    |]
                                | Input_Quantity ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Quantity"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Quantity()
                                    |]
                                | Input_PhoneNumber ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.PhoneNumber"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.PhoneNumber()
                                    |]
                                | Input_PositiveInteger ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.PositiveInteger"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.PositiveInteger()
                                    |]
                                | Input_PositiveDecimal ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.PositiveDecimal"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.PositiveDecimal()
                                    |]
                                | Input_UnsignedInteger ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.UnsignedInteger"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.UnsignedInteger()
                                    |]
                                | Input_UnsignedDecimal ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.UnsignedDecimal"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.UnsignedDecimal()
                                    |]
                                | Input_Text ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Text"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Text()
                                    |]
                                | ItemList ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ItemList"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ItemList()
                                    |]
                                | Input_Picker ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Input.Picker"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Input.Picker()
                                    |]
                                | InProgress ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.XmlDocsContent.LC.InProgress"
                                        AppEggShellGallery.Components.Constructors.Ui.XmlDocsContent.LC.InProgress()
                                    |]
                                | Layout_Row ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.XmlDocsContent.LC.Row"
                                        AppEggShellGallery.Components.Constructors.Ui.XmlDocsContent.LC.Row()
                                    |]
                                | Layout_Column ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.XmlDocsContent.LC.Column"
                                        AppEggShellGallery.Components.Constructors.Ui.XmlDocsContent.LC.Column()
                                    |]
                                | Layout_Sized ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.XmlDocsContent.LC.Sized"
                                        AppEggShellGallery.Components.Constructors.Ui.XmlDocsContent.LC.Sized()
                                    |]
                                | Layout_Constrained ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.XmlDocsContent.LC.Constrained"
                                        AppEggShellGallery.Components.Constructors.Ui.XmlDocsContent.LC.Constrained()
                                    |]
                                | Nav_Top ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Nav.Top"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Nav.Top()
                                    |]
                                | Nav_Bottom ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Nav.Bottom"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Nav.Bottom()
                                    |]
                                | Pre ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Pre"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Pre()
                                    |]
                                | QueryGrid ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.QueryGrid"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.QueryGrid()
                                    |]
                                | Section_Padded ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Section.Padded"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Section.Padded()
                                    |]
                                | Stars ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Stars"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Stars()
                                    |]
                                | Scrim ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Scrim"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Scrim()
                                    |]
                                | Sidebar ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Sidebar"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Sidebar()
                                    |]
                                | Tabs ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Tabs"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Tabs()
                                    |]
                                | Tag ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Tag"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Tag()
                                    |]
                                | TextButton ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.TextButton"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.TextButton()
                                    |]
                                | TimeSpan ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.TimeSpan"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.TimeSpan()
                                    |]
                                | Timestamp ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Timestamp"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Timestamp()
                                    |]
                                | ThirdParty_Map ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ThirdParty.Map"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ThirdParty.Map()
                                    |]
                                | ThirdParty_Recharts ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ThirdParty.Recharts"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ThirdParty.Recharts()
                                    |]
                                | Thumb ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Thumb"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Thumb()
                                    |]
                                | Thumbs ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Thumbs"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Thumbs()
                                    |]
                                | ToggleButtons ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.ToggleButtons"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.ToggleButtons()
                                    |]
                                | WithDataFlowControl ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.With.DataFlowControl"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.With.DataFlowControl()
                                    |]
                                | WithExecutor ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.XmlDocsContent.LC.With_Executor"
                                        AppEggShellGallery.Components.Constructors.Ui.XmlDocsContent.LC.With_Executor()
                                    |]
                                | WithSortAndFilter ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.WithSortAndFilter"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.WithSortAndFilter()
                                    |]
                                | Executor_AlertErrors ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.Executor.AlertErrors"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.Executor.AlertErrors()
                                    |]
                                | AnimatableImage ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.AnimatableImage"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.AnimatableImage()
                                    |]
                                | AnimatableText ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.AnimatableText"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.AnimatableText()
                                    |]
                                | AnimatableTextInput ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.AnimatableTextInput"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.AnimatableTextInput()
                                    |]
                                | AnimatableView ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.AnimatableView"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.AnimatableView()
                                    |]
                                | TouchableOpacity ->
                                    [|
                                        let __parentFQN = Some "AppEggShellGallery.Components.Content.TouchableOpacity"
                                        AppEggShellGallery.Components.Constructors.Ui.Content.TouchableOpacity()
                                    |]
                                | _ ->
                                    [|
                                        makeTextNode2 __parentFQN "Docs not available yet — why don't you add it?"
                                    |]
                                |> castAsElementAckingKeysWarning
                            |]
                    )
                |]
        )
    |])

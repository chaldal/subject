module AppEggShellGallery.Navigation

open LibClient
open LibClient.Responsive
open LibRouter.RoutesSpec
open LibRouter.Components.With.Navigation

type ResultlessDialog =
| Sentinel
with interface NavigationResultlessDialog

type ResultfulDialog =
| Sentinel
with interface NavigationResultfulDialog

type Route = {
    SampleVisualsScreenSize: ScreenSize
    ActualRoute:             ActualRoute
}
with interface NavigationRoute

and ActualRoute =
| Home
| Docs       of MarkdownUrl: string
| Components of ComponentItem
| Tools      of MarkdownUrl: string
| HowTo      of HowToItem
| Subject    of MarkdownUrl: string
| Design     of DesignItem
| Legacy     of LegacyItem
| TinyGuid

and HowToItem =
| Markdown of Url: string

and DesignItem =
| Markdown of Url: string
| Icons
| ColorVariants

and LegacyItem =
| Markdown of Url: string

and ComponentItem =
| Index
| Layout_Row
| Layout_Column
| Layout_Sized
| Layout_Constrained
| Avatar
| Button
| Buttons
| Card
| Carousel
| ContextMenu
| DateSelector
| Dialogs
| Draggable
| DueDateTag
| ErrorBoundary
| FloatingActionButton
| FormFieldsDivider
| Grid
| HandheldListItem
| HeaderCell
| Heading
| Icon
| IconButton
| IconWithBadge
| InfoMessage
| ImageCard
| Input_Checkbox
| Input_ChoiceList
| Input_Date
| Input_Decimal
| Input_DayOfTheWeek
| Input_Duration
| Input_EmailAddress
| Input_File
| Input_Image
| Input_LocalTime
| Input_Picker
| Input_PhoneNumber
| Input_PositiveInteger
| Input_PositiveDecimal
| Input_Quantity
| Input_Text
| Input_UnsignedInteger
| Input_UnsignedDecimal
| InProgress
| ItemList
| Forms
| LabelledFormField
| LabelledValue
| Nav_Top
| Nav_Bottom
| Popup
| Pre
| QueryGrid
| Route
| Section_Padded
| Scrim
| Stars
| Sidebar
| Tab
| Tabs
| Tag
| TextButton
| TimeSpan
| Timestamp
| ThirdParty_Map
| ThirdParty_Recharts
| Thumb
| Thumbs
| ToggleButtons
| TwoWayScrollable
| AsyncData
| WithContext
| WithDataFlowControl
| WithExecutor
| WithSortAndFilter
| TriStateful
| QuadStateful
| Responsive
| Executor_AlertErrors
| AnimatableImage
| AnimatableText
| AnimatableTextInput
| AnimatableView
| TouchableOpacity

let navigationState = LibRouter.RoutesSpec.NavigationState<Route, ResultlessDialog, ResultfulDialog>()

let private lazyRoutesSpec: Lazy<LibRouter.RoutesSpec.Conversions<Route, ResultlessDialog>> = lazy (
    let specs: List<LibRouter.RoutesSpec.Spec<Route>> =
        [
            ("/TinyGuid",
                (fun _ -> { SampleVisualsScreenSize = ScreenSize.Desktop; ActualRoute = TinyGuid }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = TinyGuid }) -> Some [Json.ToString svss] | _ -> None))
            ("/{json}/Docs/{json}",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = Docs (parts.GetFromJson 1) }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = Docs p }) -> Some [Json.ToString svss; Json.ToString p] | _ -> None))
            ("/{json}/Components/{json}",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = Components (parts.GetFromJson 1) }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = Components p }) -> Some [Json.ToString svss; Json.ToString p] | _ -> None))
            ("/{json}/Tools/{json}",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = Tools (parts.GetFromJson 1) }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = Tools p }) -> Some [Json.ToString svss; Json.ToString p] | _ -> None))
            ("/{json}/HowTo/{json}",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = HowTo (parts.GetFromJson 1) }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = HowTo p }) -> Some [Json.ToString svss; Json.ToString p] | _ -> None))
            ("/{json}/Design/{json}",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = Design (parts.GetFromJson 1) }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = Design p }) -> Some [Json.ToString svss; Json.ToString p] | _ -> None))
            ("/{json}/Legacy/{json}",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = Legacy (parts.GetFromJson 1) }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = Legacy p }) -> Some [Json.ToString svss; Json.ToString p] | _ -> None))
            ("/{json}/Subject/{json}",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = Subject (parts.GetFromJson 1) }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = Subject p }) -> Some [Json.ToString svss; Json.ToString p] | _ -> None))
            ("/{json}/",
                (fun parts -> { SampleVisualsScreenSize = parts.GetFromJson 0; ActualRoute = Home }),
                (function ({ SampleVisualsScreenSize = svss; ActualRoute = Home }) -> Some [Json.ToString svss] | _ -> None))
            ("/",
                (fun _ -> { SampleVisualsScreenSize = ScreenSize.Desktop; ActualRoute = Home }),
                (function _ -> None))
        ]
    LibRouter.RoutesSpec.makeConversions (Config.current().AppUrlBase) specs navigationState
)

let routesSpec() = lazyRoutesSpec.Force()

type Navigation(queue: LibClient.EventBus.Queue<NavigationAction<Route, ResultlessDialog, ResultfulDialog>>) =
    inherit LibRouter.Components.With.Navigation.Navigation<Route, ResultlessDialog, ResultfulDialog>(queue)

    member this.SetSampleVisualsScreenSize (maybeCurrentRoute: Option<Route>) (value: ScreenSize) : unit =
        maybeCurrentRoute |> Option.sideEffect(fun currentRoute ->
            this.GoInSameTab { currentRoute with SampleVisualsScreenSize = value }
        )

    member this.CurrentSampleVisualsScreenSizeOrDefault (maybeCurrentRoute: Option<Route>) : ScreenSize =
        match maybeCurrentRoute with
        | None                                                      -> ScreenSize.Desktop
        | Some { SampleVisualsScreenSize = value; ActualRoute = _ } -> value

    member this.GoInSameTab (maybeCurrentRoute: Option<Route>, actualRoute: ActualRoute) : unit =
        this.GoInSameTab {
            SampleVisualsScreenSize = this.CurrentSampleVisualsScreenSizeOrDefault maybeCurrentRoute
            ActualRoute             = actualRoute
        }

    member this.GoInNewTab (maybeCurrentRoute: Option<Route>, actualRoute: ActualRoute) : unit =
        this.GoInNewTab {
            SampleVisualsScreenSize = this.CurrentSampleVisualsScreenSizeOrDefault maybeCurrentRoute
            ActualRoute             = actualRoute
        }

    member this.Go (maybeCurrentRoute: Option<Route>, actualRoute: ActualRoute) : ReactEvent.Action -> unit =
        this.Go {
            SampleVisualsScreenSize = this.CurrentSampleVisualsScreenSizeOrDefault maybeCurrentRoute
            ActualRoute             = actualRoute
        }

    member this.CurrentActualRoute (maybeCurrentRoute: Option<Route>) : Option<ActualRoute> =
        maybeCurrentRoute |> Option.map (fun route -> route.ActualRoute)

type LibClient.Input.ButtonHighLevelStateFactory with
    static member MakeGo (maybeCurrentRoute: Option<Route>, route: ActualRoute, nav: Navigation) : ButtonHighLevelState =
        nav.Go (maybeCurrentRoute, route)
        |> ButtonLowLevelState.Actionable
        |> ButtonHighLevelState.LowLevel

let navigationQueue: LibClient.EventBus.Queue<NavigationAction<Route, ResultlessDialog, ResultfulDialog>> = LibClient.EventBus.Queue "navigation"
let nav = Navigation navigationQueue

module LibClient.Components.AppShell.Content

open LibClient
open Fable.React

type DesktopSidebarStyle =
| Fixed
| Popup

type Props = (* GenerateMakeFunction *) {
    DesktopSidebarStyle: DesktopSidebarStyle
    Sidebar:             ReactElement
    TopStatus:           ReactElement // default noElement
    TopNav:              ReactElement // default noElement
    Content:             ReactElement
    Dialogs:             ReactElement
    BottomNav:           ReactElement // default noElement
    OnError:             (System.Exception * (unit -> unit)) -> ReactElement

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    IsSidebarScrimVisible: bool
    SidebarPopupConnector: LibClient.Components.Popup.Connector
    MaybeSidebarDraggable: Option<LibClient.Components.Draggable.IDraggableRef>
}

[<RequireQualifiedAccess>]
type SidebarVisibilityEvent =
| Set    of IsVisible: bool * Event: ReactEvent.Action
| Toggle of Event: ReactEvent.Action

let private sidebarVisibilityQueue: LibClient.EventBus.Queue<SidebarVisibilityEvent> = LibClient.EventBus.Queue "sidebarVisibility"

let setSidebarVisibility (isVisible: bool) (e: ReactEvent.Action) : unit =
    SidebarVisibilityEvent.Set (isVisible, e)
    |> LibClient.ServiceInstances.services().EventBus.Broadcast sidebarVisibilityQueue

let toggleSidebarVisibility (e: ReactEvent.Action) : unit =
    SidebarVisibilityEvent.Toggle e
    |> LibClient.ServiceInstances.services().EventBus.Broadcast sidebarVisibilityQueue

type Content(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Content>("LibClient.Components.AppShell.Content", _initialProps, Actions, hasStyles = true)

    override this.GetInitialEstate (_initialProps: Props) : Estate =
        let sidebarPopupConnector = LibClient.Components.Popup.Connector()
        sidebarPopupConnector.OnDismiss (fun () ->
            async {
                // ReactXP's popup system has an interesting way of handling off-clicks.
                // They hide the popup, but they also let the click do its thing. The
                // consequence of that is that the button that's set to "toggleSidebarVisibility"
                // will call the toggle function _after_ the OnDismiss callback is called, which
                // means that instead of hiding the open popup, it would open it again, because the
                // click itself hid the popup.
                // We could wire this more robustly, by completely getting rid of the "toggle" functionality
                // and always using "set", but I'm fairly convinced that adding this delay here
                // solves our problem without the need for rewiring. So in the interest of
                // saving time and moving on to other work, we try this delay.
                do! Async.Sleep (System.TimeSpan.FromMilliseconds 300.0)
                this.SetEstate (fun estate _ -> { estate with IsSidebarScrimVisible = false })
            } |> startSafely
        )

        {
            IsSidebarScrimVisible = false
            SidebarPopupConnector = sidebarPopupConnector
            MaybeSidebarDraggable = None
        }

    override this.ComponentDidMount () : unit =
        this.RunOnUnmount
            (LibClient.ServiceInstances.services().EventBus.On sidebarVisibilityQueue (fun sidebarVisibilityEvent ->
                let (isVisible, e) =
                    match sidebarVisibilityEvent with
                    | SidebarVisibilityEvent.Set (isVisible, e) -> (isVisible, e)
                    | SidebarVisibilityEvent.Toggle e           -> (not this.state.IsSidebarScrimVisible, e)

                this.SetEstate (fun estate _ -> { estate with IsSidebarScrimVisible = isVisible })

                if this.props.DesktopSidebarStyle = Popup then
                    match isVisible with
                    | true  -> this.state.SidebarPopupConnector.Show (e.MaybeSource |> Option.getOrElseRaise (exn "Can't open a popup without an anchor"))
                    | false -> this.state.SidebarPopupConnector.Hide ()

                this.state.MaybeSidebarDraggable |> Option.sideEffect (fun sidebarDraggable ->
                    match isVisible with
                    | true  -> LibClient.Components.Draggable.Position.Right
                    | false -> LibClient.Components.Draggable.Position.Base
                    |> sidebarDraggable.SetPosition
                    |> ignore
                )
            )).Off

and Actions(this: Content) =
    // see With.Ref's implementation for explanation
    let bound = {|
        RefSidebarDraggable = fun (nullableInstance: LibClient.JsInterop.JsNullable<LibClient.Components.Draggable.IDraggableRef>) ->
        this.SetEstate (fun estate _ -> { estate with MaybeSidebarDraggable = nullableInstance.ToOption })
    |}
    member _.Bound = bound

    member _.OnSidebarDraggableChange (change: LibClient.Components.Draggable.Change) : unit =
        match change with
        | LibClient.Components.Draggable.Change.DragInducedAnimationStarted LibClient.Components.Draggable.Position.Right
        | LibClient.Components.Draggable.Change.PositionChanged (LibClient.Components.Draggable.Position.Right, LibClient.Components.Draggable.PositionChangeReason.ManualDrag) ->
            this.SetEstate (fun estate _ -> { estate with IsSidebarScrimVisible = true })
        | LibClient.Components.Draggable.Change.DragInducedAnimationStarted LibClient.Components.Draggable.Position.Base
        | LibClient.Components.Draggable.Change.PositionChanged (LibClient.Components.Draggable.Position.Base, LibClient.Components.Draggable.PositionChangeReason.ManualDrag) ->
            this.SetEstate (fun estate _ -> { estate with IsSidebarScrimVisible = false })
        | _ -> Noop

    member _.OnError (error: System.Exception, retry: unit -> unit) : ReactElement =
        Log.Error ("AppShell.Content error boundary: {Error}", error)
        this.props.OnError (error, retry)

let Make = makeConstructor<Content, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

module LibClient.Components.Popup

open Fable.Core
open Fable.Core.JsInterop
open LibClient
open LibLang

// NOTE there's a lot more available in the ReactXP PopupOptions which we are
// not exposing here. We can add this functionality when needed.

type ConnectorInternalCallbacks = {
    Show:    ReactElement -> unit
    Hide:    unit -> unit
    IsShown: unit -> bool
}

type Connector() =
    let mutable maybeCallbacks: Option<ConnectorInternalCallbacks> = None
    let mutable onDismissCallbacks: List<unit -> unit> = []

    member _.SetCallbacks(callbacks: ConnectorInternalCallbacks) : unit =
        maybeCallbacks <- Some callbacks

    member _.ClearCallbacks () : unit =
        maybeCallbacks <- None

    member _.Show(anchor: ReactElement) : unit =
        maybeCallbacks |> Option.sideEffect (fun callbacks -> callbacks.Show anchor)

    member _.Hide() : unit =
        maybeCallbacks |> Option.sideEffect (fun callbacks -> callbacks.Hide())

    member _.IsShown() : bool =
        maybeCallbacks
        |> Option.map (fun callbacks -> callbacks.IsShown())
        // pretty safe to say it's not shown if the callbacks are not set
        |> Option.getOrElse false

    member _.OnDismiss (fn: unit -> unit) : unit =
        onDismissCallbacks <- fn :: onDismissCallbacks

    member _.CallOnDismissCallbacks () : unit =
        onDismissCallbacks |> List.iter (fun fn -> fn ())

[<StringEnum>]
type Position =
| Top
| Right
| Bottom
| Left
| Context

type Props = (* GenerateMakeFunction *) {
    Render:    unit -> ReactElement
    Connector: Connector
    Id:        string option // defaultWithAutoWrap None
}

type Estate = EmptyRecordType

type Popup(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Popup>("LibClient.Components.Popup", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) = EmptyRecord

    override this.ComponentDidMount() =
        let id = this.props.Id |> Option.getOrElse (sprintf "popup-%i" (System.Random().Next()))

        this.props.Connector.SetCallbacks
            {
                Show = (fun (anchor: ReactElement) ->
                    let options = createObj [
                        "getAnchor"   ==> fun () -> anchor
                        "renderPopup" ==> fun (_anchorPosition: obj, _anchorOffset: int, _popupWidth: int, _popupHeight: int) ->
                            this.props.Render ()
                        "onDismiss"   ==> fun () -> this.props.Connector.CallOnDismissCallbacks ()
                    ]
                    ReactXP.Helpers.ReactXPRaw?Popup?show(options, id)
                )
                Hide = (fun () ->
                    ReactXP.Helpers.ReactXPRaw?Popup?dismiss(id)
                )
                IsShown = (fun () ->
                    ReactXP.Helpers.ReactXPRaw?Popup?isDisplayed(id)
                )
            }

    override this.ComponentWillUnmount() =
        this.props.Connector.ClearCallbacks ()

and Actions(_this: Popup) =
    class end

let Make = makeConstructor<Popup, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
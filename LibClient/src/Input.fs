[<AutoOpen>]
module LibClient.Input

open Fable.Core.JsInterop
open LibClient.UniDirectionalDataFlow

type SelectableValue<'T when 'T : comparison> =
| AtMostOne  of Value: Option<'T>             * OnChange: (Option<'T>             -> unit)
| ExactlyOne of Value: Option<'T>             * OnChange: ('T                     -> unit)
| AtLeastOne of Value: Option<OrderedSet<'T>> * OnChange: (NonemptyOrderedSet<'T> -> unit)
| Any        of Value: Option<OrderedSet<'T>> * OnChange: (OrderedSet<'T>         -> unit)
with
    member this.Selected: OrderedSet<'T> =
        match this with
        | AtMostOne  (maybeSelectedValue,  _)
        | ExactlyOne (maybeSelectedValue,  _) ->
            maybeSelectedValue |> Option.map OrderedSet.ofOneItem |> Option.getOrElse OrderedSet.empty
        | AtLeastOne (maybeSelectedValues, _)
        | Any        (maybeSelectedValues, _) ->
            maybeSelectedValues |> Option.getOrElse OrderedSet.empty

    member this.IsSelected (value: 'T) : bool =
        match this with
        | AtMostOne  (mayBeSelectedValue,  _)
        | ExactlyOne (mayBeSelectedValue,  _) ->
            match mayBeSelectedValue with
            | Some selectedValue -> selectedValue = value
            | None               -> false
        | AtLeastOne (maybeSelectedValues, _)
        | Any        (maybeSelectedValues, _) ->
            match maybeSelectedValues with
            | Some selectedValues -> selectedValues.Contains value
            | None                -> false

    member this.Select (value: 'T) : unit =
        match this with
        | AtMostOne  (_,                   onChange) -> Some value |> onChange
        | ExactlyOne (_,                   onChange) -> value |> onChange
        | AtLeastOne (maybeSelectedValues, onChange) ->
            match maybeSelectedValues with
            | Some selectedValues ->
                selectedValues.Add value
                |> NonemptyOrderedSet.tryOfOrderedSet
                |> function
                    | Some value -> value |> onChange
                    | None       -> Noop
            | None -> NonemptyOrderedSet.ofOneItem value |> onChange
        | Any (maybeSelectedValues, onChange) ->
            match maybeSelectedValues with
            | Some selectedValues -> selectedValues.Add value |> onChange
            | None                -> OrderedSet.ofOneItem value |> onChange

    member this.Unselect (value: 'T) : unit =
        match this with
        | ExactlyOne _ -> Noop
        | AtMostOne  (_, onChange) -> onChange None
        | AtLeastOne (maybeSelectedValues, onChange) ->
            match maybeSelectedValues with
            | Some selectedValues -> selectedValues.Remove value |> NonemptyOrderedSet.tryOfOrderedSet |> Option.sideEffect onChange
            | None                -> Noop
        | Any (maybeSelectedValues, onChange) ->
            match maybeSelectedValues with
            | Some selectedValues -> selectedValues.Remove value |> onChange
            | None                -> Noop

    member this.UnselectAllIfAllowed () : unit =
        match this with
        | AtMostOne (_, onChange) -> onChange None
        | Any       (_, onChange) -> onChange OrderedSet.empty
        | _                       -> Noop

    member this.Toggle (value: 'T) : unit =
        match this with
        | AtMostOne  (maybeSelectedValue, _)
        | ExactlyOne (maybeSelectedValue, _) ->
            match maybeSelectedValue with
            | Some selectedValue -> value |> if selectedValue = value then this.Unselect else this.Select
            | None               -> value |> this.Select
        | AtLeastOne (maybeSelectedValues, _)
        | Any        (maybeSelectedValues, _) ->
            match maybeSelectedValues with
            | Some selectedValues -> value |> if selectedValues.Contains value then this.Unselect else this.Select
            | None                -> value |> this.Select

    member this.IsEmpty: bool =
        match this with
        | AtMostOne  (None, _)
        | ExactlyOne (None, _)
        | AtLeastOne (None, _)
        | Any        (None, _) -> true
        | _ -> false

    member this.CanSelectMultiple: bool =
        match this with
        | AtMostOne  _ -> false
        | ExactlyOne _ -> false
        | AtLeastOne _ -> true
        | Any        _ -> true

// NOTE this will probably go away as we unify our input components
type InputValidationError =
| Missing
| InvalidValue
| Other of string


type [<DefaultAugmentation(false)>] InputValidity =
| Valid
| Missing
| Invalid of Reason: string
with
    member this.IsInvalid : bool =
        this <> Valid

    member this.InvalidReason : Option<string> =
        match this with
        | Invalid reason -> Some reason
        | _              -> None

    member this.Message : Option<string> =
        match this with
        | Invalid reason -> Some reason
        | Missing        -> Some "Missing"
        | _              -> None

    member this.MessageWithMissing (missingMessage: string) : Option<string> =
        match this with
        | Invalid reason -> Some reason
        | Missing        -> Some missingMessage
        | _              -> None

    member this.Or (other: InputValidity) : InputValidity =
        match this with
        | Valid -> other
        | _     -> this

module ReactEvent =
    // Could have used generics, but they preclude having static member OfBrowserEvent,
    // since you can't have extensions of type aliases

    type Keyboard = {
        Event:       Browser.Types.KeyboardEvent
        MaybeSource: Option<Fable.React.ReactElement>
    } with
        static member OfBrowserEvent (e: Browser.Types.KeyboardEvent) = {
            Event       = e
            MaybeSource = None
        }
        member this.WithSource      (source:      Fable.React.ReactElement)         = { this with MaybeSource = Some source }
        member this.WithMaybeSource (maybeSource: Option<Fable.React.ReactElement>) = { this with MaybeSource = maybeSource }
        member this.PersistForLaterAccess () : unit = (this.Event?persist)()

    type Pointer = {
        Event:       Browser.Types.PointerEvent
        MaybeSource: Option<Fable.React.ReactElement>
    } with
        static member OfBrowserEvent (e: Browser.Types.PointerEvent) = {
            Event       = e
            MaybeSource = None
        }
        member this.WithSource      (source:      Fable.React.ReactElement)         = { this with MaybeSource = Some source }
        member this.WithMaybeSource (maybeSource: Option<Fable.React.ReactElement>) = { this with MaybeSource = maybeSource }
        member this.PersistForLaterAccess () : unit = (this.Event?persist)()


    type Focus = {
        Event:       Browser.Types.FocusEvent
        MaybeSource: Option<Fable.React.ReactElement>
    } with
        static member OfBrowserEvent (e: Browser.Types.FocusEvent) = {
            Event       = e
            MaybeSource = None
        }
        member this.WithSource      (source:      Fable.React.ReactElement)         = { this with MaybeSource = Some source }
        member this.WithMaybeSource (maybeSource: Option<Fable.React.ReactElement>) = { this with MaybeSource = maybeSource }
        member this.PersistForLaterAccess () : unit = (this.Event?persist)()

    [<RequireQualifiedAccess>]
    type Action =
    | Keyboard of Keyboard
    | Pointer  of Pointer
    | Focus    of Focus
    | NonUserOriginatingAction
    with
        static member Make (e: Keyboard) = Action.Keyboard e
        static member Make (e: Pointer)  = Action.Pointer e
        static member OfBrowserEvent (e: Browser.Types.KeyboardEvent) = Keyboard.OfBrowserEvent e |> Action.Keyboard
        static member OfBrowserEvent (e: Browser.Types.PointerEvent)  = Pointer.OfBrowserEvent  e |> Action.Pointer
        static member OfBrowserEvent (e: Browser.Types.FocusEvent)    = Focus.OfBrowserEvent    e |> Action.Focus

        member this.MaybeSource : Option<Fable.React.ReactElement> =
            match this with
            | Keyboard e               -> e.MaybeSource
            | Pointer  e               -> e.MaybeSource
            | Focus    e               -> e.MaybeSource
            | NonUserOriginatingAction -> None

        member this.MaybeEvent : Option<Browser.Types.Event> =
            match this with
            | Keyboard e               -> e.Event :> Browser.Types.Event |> Some
            | Pointer  e               -> e.Event :> Browser.Types.Event |> Some
            | Focus    e               -> e.Event :> Browser.Types.Event |> Some
            | NonUserOriginatingAction -> None

        member this.PersistForLaterAccess () : unit =
            this.MaybeEvent
            |> Option.sideEffect (fun event -> (event?persist)())


type ButtonLowLevelState =
| Actionable of OnPress: (ReactEvent.Action -> unit)
| InProgress
| Disabled
with
    member this.GetName : string =
        unionCaseName this

[<RequireQualifiedAccess>]
type ButtonHighLevelState =
| LowLevel       of ButtonLowLevelState
| BoundEventful  of (ReactEvent.Action -> UDAction) * Executor * AlsoInProgressIf: bool
| BoundEventless of UDAction                        * Executor * AlsoInProgressIf: bool
with
    member this.ToLowLevel : ButtonLowLevelState =
        match this with
        | LowLevel lowLevelState -> lowLevelState
        | BoundEventful (eventToAction, executor, alsoInProgressIf) ->
            match (alsoInProgressIf, executor) with
            | (true, _)                            -> InProgress
            | (false, Executor.Actionable execute) -> Actionable (fun e -> execute (eventToAction e) |> ignore)
            | (false, Executor.InProgress)         -> InProgress
            | (false, Executor.Error _)            -> Disabled
        | BoundEventless (action, executor, alsoInProgressIf) ->
            match (alsoInProgressIf, executor) with
            | (true, _)                            -> InProgress
            | (false, Executor.Actionable execute) -> Actionable (fun _e -> execute action |> ignore)
            | (false, Executor.InProgress)         -> InProgress
            | (false, Executor.Error _)            -> Disabled

type ButtonHighLevelStateFactory =
    static member Make (pair: (ReactEvent.Action -> UDAction) * Executor) : ButtonHighLevelState =
        let (eventToAction, executor) = pair
        ButtonHighLevelStateFactory.Make (eventToAction, executor)

    static member Make (eventToAction: ReactEvent.Action -> UDAction, executor: Executor) : ButtonHighLevelState =
        ButtonHighLevelStateFactory.Make (eventToAction, executor, (* alsoInProgressIf *) false)

    static member Make (pair: (ReactEvent.Action -> UDAction) * Executor, alsoInProgressIf: bool) : ButtonHighLevelState =
        let (eventToAction, executor) = pair
        ButtonHighLevelStateFactory.Make (eventToAction, executor, alsoInProgressIf)

    static member Make (eventToAction: ReactEvent.Action -> UDAction, executor: Executor, alsoInProgressIf: bool) : ButtonHighLevelState =
        ButtonHighLevelState.BoundEventful (eventToAction, executor, alsoInProgressIf)

    static member Make (pair: UDAction * Executor) : ButtonHighLevelState =
        let (action, executor) = pair
        ButtonHighLevelStateFactory.Make (action, executor)

    static member Make (action: UDAction, executor: Executor) : ButtonHighLevelState =
        ButtonHighLevelStateFactory.Make (action, executor, (* alsoInProgressIf *) false)

    static member Make (pair: UDAction * Executor, alsoInProgressIf: bool) : ButtonHighLevelState =
        let (action, executor) = pair
        ButtonHighLevelStateFactory.Make (action, executor, alsoInProgressIf)

    static member Make (action: UDAction, executor: Executor, alsoInProgressIf: bool) : ButtonHighLevelState =
        ButtonHighLevelState.BoundEventless (action, executor, alsoInProgressIf)

    static member MakeLowLevel (value: ButtonLowLevelState) : ButtonHighLevelState =
        ButtonHighLevelState.LowLevel value

    static member MakeDisabled : ButtonHighLevelState =
        ButtonHighLevelState.LowLevel Disabled

type ButtonHighLevelState with
    static member CloseDialog (tryCancel: ReactEvent.Action -> unit) : ButtonHighLevelState =
        tryCancel |> ButtonLowLevelState.Actionable |> ButtonHighLevelState.LowLevel


[<RequireQualifiedAccess>]
type InputSuffix =
| Text of string
| Icon of LibClient.Icons.IconConstructor
| Element of ReactElement

type InputSuffixFactory =
    static member Make (input: string) : InputSuffix =
        input |> InputSuffix.Text

    static member Make (input: LibClient.Icons.IconConstructor) : InputSuffix =
        input |> InputSuffix.Icon

    static member Make (input: ReactElement) : InputSuffix =
        input |> InputSuffix.Element

module KeyboardEvent =
    module Key =
        let (|ArrowUp   |_|) (key: string) = if key = "ArrowUp"    then Some ArrowUp    else None
        let (|ArrowDown |_|) (key: string) = if key = "ArrowDown"  then Some ArrowDown  else None
        let (|ArrowLeft |_|) (key: string) = if key = "ArrowLeft"  then Some ArrowLeft  else None
        let (|ArrowRight|_|) (key: string) = if key = "ArrowRight" then Some ArrowRight else None
        let (|Backspace |_|) (key: string) = if key = "Backspace"  then Some Backspace  else None
        let (|Tab       |_|) (key: string) = if key = "Tab"        then Some Tab        else None
        let (|Enter     |_|) (key: string) = if key = "Enter"      then Some Enter      else None
        let (|Escape    |_|) (key: string) = if key = "Escape"     then Some Escape     else None

type Browser.Types.PointerEvent with
    member this.CrossPlatformPageXY : Option<float * float> =
        if isNullOrUndefined this.pageX then
            // being sloppy about distinction between touches and changedTouches, hopefully this is
            // good enough for our needs. If not, will need to differentiate based on even type, see
            // https://developer.mozilla.org/en-US/docs/Web/API/TouchEvent/changedTouches
            let maybeTouches: Option<array<Browser.Types.Touch>> =
                if not (isNullOrUndefined this?nativeEvent) && not (isNullOrUndefined this?nativeEvent?touches) && not (this?nativeEvent?touches?length = 0) then
                    Some this?nativeEvent?touches
                elif not (isNullOrUndefined this?nativeEvent) && not (isNullOrUndefined this?nativeEvent?changedTouches) && not (this?nativeEvent?changedTouches?length = 0) then
                    Some this?nativeEvent?changedTouches
                else
                    None

            maybeTouches
            |> Option.map (fun touches -> (touches.[0].pageX, touches.[0].pageY))
        else
            Some (this.pageX, this.pageY)


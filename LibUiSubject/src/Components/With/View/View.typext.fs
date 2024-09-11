module LibUiSubject.Components.With.View

open Fable.React
open LibClient
open LibClient.Components
open LibUiSubject

// NEW hook implementation

let private getView
    (service: LibUiSubject.Services.ViewService.IViewService<'Input, 'Output, 'OpError>)
    (maybeUseCache: Option<UseCache>)
    (input: 'Input)
    : Async<AsyncData<'Output>> =
    let useCache = defaultArg maybeUseCache UseCache.IfReasonablyFresh
    service.GetOne useCache input


// TODO need to figure out the module/namespace strategy here
type C =
    [<Component>]
    static member View (
        service: LibUiSubject.Services.ViewService.IViewService<'Input, 'Output, 'OpError>,
        input: 'Input,
        whenAvailable: 'Output -> ReactElement,
        ?whenUninitialized: unit -> ReactElement,
        ?whenFetching: Option<'Output> -> ReactElement,
        ?whenFailed: AsyncDataFailure -> ReactElement,
        ?whenUnavailable: unit -> ReactElement,
        ?whenAccessDenied: unit -> ReactElement,
        ?whenElse: unit -> ReactElement,
        ?useCache: UseCache)
        : ReactElement =
            let valueState = Hooks.useState WillStartFetchingSoonHack

            Hooks.useEffect(
                dependencies = [| input |],
                effect =
                    fun () ->
                        async {
                            let! outputAD = getView service useCache input
                            valueState.update outputAD
                        } |> startSafely
            )

            LC.AsyncData (
                data = valueState.current,
                whenAvailable = whenAvailable,
                ?whenUninitialized = whenUninitialized,
                ?whenFetching = whenFetching,
                ?whenFailed = whenFailed,
                ?whenUnavailable = whenUnavailable,
                ?whenAccessDenied = whenAccessDenied,
                ?whenElse = whenElse
            )

    [<Component>]
    static member View (
        service: LibUiSubject.Services.ViewService.IViewService<'Input, 'Output, 'OpError>,
        input: 'Input,
        content: AsyncData<'Output> -> ReactElement,
        ?useCache: UseCache)
        : ReactElement =
            let valueState = Hooks.useState WillStartFetchingSoonHack

            Hooks.useEffect(
                dependencies = [| input |],
                effect =
                    fun () ->
                        async {
                            let! outputAD = getView service useCache input
                            valueState.update outputAD
                        } |> startSafely
            )

            content valueState.current

// LEGACY component implementation

let No                = UseCache.No
let IfNewerThan       = UseCache.IfNewerThan
let IfReasonablyFresh = UseCache.IfReasonablyFresh
let IfAvailable       = UseCache.IfAvailable

type WithView<'Output> =
| Raw           of (AsyncData<'Output> -> ReactElement)
| WhenAvailable of ('Output -> ReactElement)

type PropWithFactory =
    static member Make<'T> (whenAvailable: 'T -> ReactElement) =
        WithView.WhenAvailable whenAvailable

// UseCache for View is set to ~No by default
// because we don't support realtime streaming for them, yet
type Props<'Input, 'Output, 'OpError when 'Input : comparison> = (* GenerateMakeFunction *) {
    Service:  LibUiSubject.Services.ViewService.IViewService<'Input, 'Output, 'OpError>
    Input:    'Input
    UseCache: UseCache // default UseCache.No
    With:     WithView<'Output>

    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate<'Input, 'Output, 'OpError when 'Input : comparison> = {
    Value: AsyncData<'Output>
}

type View<'Input, 'Output, 'OpError when 'Input : comparison>(_initialProps) =
    inherit EstatefulComponent<Props<'Input, 'Output, 'OpError>, Estate<'Input, 'Output, 'OpError>, Actions<'Input, 'Output, 'OpError>, View<'Input, 'Output, 'OpError>>("LibUiSubject.Components.With.View", _initialProps, Actions<'Input, 'Output, 'OpError>, hasStyles = false)

    override _.GetInitialEstate(_initialProps: Props<'Input, 'Output, 'OpError>) : Estate<'Input, 'Output, 'OpError> = {
        Value = WillStartFetchingSoonHack
    }

    member this.FetchData () : unit =
        async {
            let! outputAD = this.props.Service.GetOne this.props.UseCache this.props.Input
            this.setState (fun estate _ -> { estate with Value = outputAD })
        } |> startSafely

    override this.ComponentDidMount() : unit =
        this.FetchData()

    override this.ComponentDidUpdate(prevProps, _) : unit =
        if prevProps.Input <> this.props.Input then
            this.FetchData()

and Actions<'Input, 'Output, 'OpError when 'Input : comparison>(_this: View<'Input, 'Output, 'OpError>) =
    class end

let Make = makeConstructor<View<'Input, 'Output, 'OpError>, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate

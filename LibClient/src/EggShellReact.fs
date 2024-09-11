[<AutoOpen>]
module LibClient.EggShellReact

open Fable.React
open LibClient
open LibClient.JsInterop
open Fable.Core

type ReactElement = Fable.React.ReactElement

type ReactElements = array<ReactElement>

let noElement = Fable.React.Helpers.nothing

let (|NoElement|Element|) (candidate: ReactElement) =
    if candidate = noElement then
        NoElement
    else
        Element candidate

type RenderFunction<'Props, 'EState, 'PState, 'Actions> = array<ReactElement> * 'Props * 'EState * 'PState * 'Actions * ReactXP.LegacyStyles.RuntimeStyles -> Fable.React.ReactElement

type NoEstate = unit
type NoEstate1<'T>                                                       = NoEstate1 of unit
type NoEstate2<'T1, 'T2>                                                 = NoEstate2 of unit
type NoEstate3<'T1, 'T2, 'T3>                                            = NoEstate3 of unit
type NoEstate4<'T1, 'T2, 'T3, 'T4>                                       = NoEstate4 of unit
type NoEstate5<'T1, 'T2, 'T3, 'T4, 'T5>                                  = NoEstate5 of unit
type NoEstate6<'T1, 'T2, 'T3, 'T4, 'T5, 'T6>                             = NoEstate6 of unit
type NoEstate7<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7>                        = NoEstate7 of unit
type NoEstate8<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8>                   = NoEstate8 of unit
type NoEstate9<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'T9>              = NoEstate9 of unit
type NoEstate10<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'T9, 'T10>       = NoEstate10 of unit
type NoEstate11<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'T9, 'T10, 'T11> = NoEstate11 of unit
type NoPstate = unit

let getRenderFunction<'Props, 'EState, 'PState, 'Actions> (fullyQualifiedComponentName: string) : RenderFunction<'Props, 'EState, 'PState, 'Actions> =
    ComponentRegistry.GetRender fullyQualifiedComponentName

let getStyles<'Props, 'EState, 'PState, 'Actions> (fullyQualifiedComponentName: string) (hasStyles: bool) : ReactXP.LegacyStyles.RuntimeStyles =
    match hasStyles with
    | true  -> ComponentRegistry.GetStyles fullyQualifiedComponentName
    | false -> ReactXP.LegacyStyles.RuntimeStyles.None

type PersistentStore private () =
    static let mutable maybeInstance: Option<PersistentStore> = None

    let store = System.Collections.Generic.Dictionary<string, obj>()

    static member MaybeInstance = maybeInstance

    member private _.Store = store

    static member Initialize (initialData: Map<string, obj>) : PersistentStore =
        let instance = PersistentStore ()
        initialData |> Map.iter (fun key data ->
            instance.Store.[key] <- data
        )
        maybeInstance <- Some instance
        instance

    member _.Put<'V> (key: string) (value: 'V) : unit =
        store.[key] <- value

    member _.Get<'V> (key: string) : Option<'V> =
        match store.TryGetValue key with
        | (true, data) -> Some (data :?> 'V)
        | _ -> None

    member _.Remove (key: string) : unit =
        store.Remove key
        |> ignore

let InitializePersistentStore (initialData: Map<string, obj>) : PersistentStore =
    PersistentStore.Initialize initialData


[<AbstractClass>]
[<AttachMembers>]
type BaseComponent<'Props, 'State> (initialProps: 'Props) =
    inherit Fable.React.Component<'Props, 'State>(initialProps)

    abstract member ComponentWillUpdate: nextProps: 'Props * nextState: 'State -> unit
    default this.ComponentWillUpdate(_, _) = ()

    member this.UNSAFE_componentWillUpdate(nextProps: 'Props, nextState: 'State) = this.ComponentWillUpdate(nextProps, nextState)

    abstract member ComponentWillReceiveProps: nextProps: 'Props -> unit
    default this.ComponentWillReceiveProps(_) = ()

    member this.UNSAFE_componentWillReceiveProps(nextProps: 'Props) = this.ComponentWillReceiveProps(nextProps)

// We provide PascalCased versions of React methods, to avoid lint errors
[<AbstractClass>]
type Component<'Props, 'State> (initialProps: 'Props) =
    inherit BaseComponent<'Props, 'State>(initialProps)

    let mutable onUnmountCallbacks: list<unit -> unit> = List.empty<unit -> unit>
    let mutable didComponentUnmount: bool = false

    member _.DidComponentUnmount: bool = didComponentUnmount

    member _.RunOnUnmount (callback: unit -> unit) =
        onUnmountCallbacks <- callback :: onUnmountCallbacks

    override this.componentWillUnmount () =
        List.iter (fun f -> f ()) onUnmountCallbacks
        this.ComponentWillUnmount()
        didComponentUnmount <- true

    override this.componentDidMount() = this.ComponentDidMount()
    override this.componentDidUpdate(prevProps: 'Props, prevState: 'State) = this.ComponentDidUpdate(prevProps, prevState)
    override this.shouldComponentUpdate(nextProps: 'Props, nextState: 'State) = this.ShouldComponentUpdate(nextProps, nextState)
    override this.render() = this.Render()

    abstract member ComponentWillUnmount: unit -> unit
    default this.ComponentWillUnmount() = ()

    abstract member ComponentDidMount: unit -> unit
    default this.ComponentDidMount() = ()

    // IMPORTANT we use these "renamings" to bring things into the F# standard naming
    // scheme, but compontnDidCatch is special — providing an implementation,
    // even one that happens to be a NOOP, turns this component into an error boundary,
    // which means it kills the default React's "unwind the component tree until we hit
    // an error boundary or reach the root" behaviour, which is seriously detrimental
    // if we actually want to properly use error boundaries.
    // So just override the lowercased version instead. Leaving the commented out
    // code in case some helpful person decides to add the "renamings" because they
    // appear to be missing.
    //
    // override this.componentDidCatch(error: System.Exception, info: obj) = this.ComponentDidCatch(error, info)
    // abstract member ComponentDidCatch: System.Exception * obj -> unit
    // default this.ComponentDidCatch(_, _) = ()

    abstract member ComponentDidUpdate: prevProps: 'Props * prevState: 'State -> unit
    default this.ComponentDidUpdate(_, _) = ()

    abstract member ShouldComponentUpdate: nextProps: 'Props * nextState: 'State -> bool
    default this.ShouldComponentUpdate(nextProps, nextState) =
        ((deepEquals this.props nextProps) && (deepEquals this.state nextState))
        |> not

    abstract Render: unit -> Fable.React.ReactElement

type PstatefulState<'EState, 'PState> = {
    estate: 'EState
    pstate: 'PState
}



// NOTE we want a Self type that the type system binds to the type of the current instance (subclass),
// but that doesn't seem to be available in F#, so we are forced to add another type parameter,
// and then do a cast when constructing actions.
//
// NOTE It is highly regrettable that the F# type system does not allow structural inheritance for
// record types. So we can't put a constraint "has the pstoreKey field" on the 'Props type and still
// keep it a record. As a result, we're forced to take pstoreKey as a constructor parameter (to ensure
// that it's necessarily provided), but the author of individual components is sadly given a choice
// of where to get the value for the parameter. The hope is that they will always just pass
// `initialProps.pstoreKey` and thus be forced to add `pstoreKey: string` to their `Props` definition.
[<AbstractClass>]
type PstatefulComponent<'Props, 'Estate, 'Pstate, 'Actions, 'Self>(fullyQualifiedName: string, pstoreKey: string, initialProps: 'Props, actionsConstructor: 'Self -> 'Actions, hasStyles: bool) as this =
    inherit Component<'Props, PstatefulState<'Estate, 'Pstate>>(initialProps)

    let actions: 'Actions = actionsConstructor ((this :> obj) :?> 'Self)
    let styles: ReactXP.LegacyStyles.RuntimeStyles = getStyles fullyQualifiedName hasStyles

    do
        match PersistentStore.MaybeInstance with
        | None -> failwith "Persistent store was never initialized"
        | Some persistentStore ->
            let initialPstate = (persistentStore.Get pstoreKey) |> Option.defaultWith (fun _ -> this.GetDefaultPstate initialProps)
            this.setInitState({
                pstate = initialPstate
                estate = this.GetInitialEstate initialProps
            })
            this.RunOnUnmount (fun () -> persistentStore.Remove pstoreKey)

    member _.Actions : 'Actions = actions

    override (* final *) this.Render() =
        (getRenderFunction fullyQualifiedName) (this.props?children, this.props, this.state.estate, this.state.pstate, actions, styles)

    abstract member GetDefaultPstate: (* initialProps *) 'Props -> 'Pstate
    abstract member GetInitialEstate: (* initialProps *) 'Props -> 'Estate

    member this.SetEstate(updater: 'Estate -> 'Pstate -> 'Props -> 'Estate) =
        this.setState(fun state props -> {state with estate = updater state.estate state.pstate props})

    member this.SetPstate(updater: 'Pstate -> 'Estate -> 'Props -> 'Pstate) =
        this.setState(fun state props ->
            let updatedPstate = updater state.pstate state.estate props

            match PersistentStore.MaybeInstance with
            | None -> failwith "Persistent store was never initialized"
            | Some persistentStore -> persistentStore.Put pstoreKey updatedPstate

            {state with pstate = updatedPstate}
        )

[<AbstractClass>]
type EstatefulComponent<'Props, 'Estate, 'Actions, 'Self>(fullyQualifiedName: string, initialProps: 'Props, actionsConstructor: 'Self -> 'Actions, hasStyles: bool) as this =
    inherit Component<'Props, 'Estate>(initialProps)

    let actions: 'Actions = actionsConstructor ((this :> obj) :?> 'Self)
    let styles: ReactXP.LegacyStyles.RuntimeStyles = getStyles fullyQualifiedName hasStyles

    do
        this.setInitState(this.GetInitialEstate initialProps)

    member _.Actions : 'Actions = actions

    override (* final *) this.Render() =
        (getRenderFunction fullyQualifiedName) (this.props?children, this.props, this.state, (), actions, styles)

    abstract member GetInitialEstate: (* initialProps *) 'Props -> 'Estate

    member this.SetEstate(updater: 'Estate -> 'Props -> 'Estate) =
        this.setState(updater)


type PureStatelessComponent<'Props, 'Actions, 'Self>(fullyQualifiedName: string, initialProps: 'Props, actionsConstructor: 'Self -> 'Actions, hasStyles: bool) as this =
    inherit Component<'Props, obj>(initialProps)

    let actions: 'Actions = actionsConstructor ((this :> obj) :?> 'Self)
    let styles: ReactXP.LegacyStyles.RuntimeStyles = getStyles fullyQualifiedName hasStyles

    member _.Actions : 'Actions = actions

    override (* final *) this.Render() =
        (getRenderFunction fullyQualifiedName) (this.props?children, this.props, (), (), actions, styles)

let inline makeConstructor<'Component, 'Props, 'State when 'Component :> Fable.React.Component<'Props, 'State>> =
    fun (props: 'Props) (children: array<ReactElement>) -> Fable.React.Helpers.ofType<'Component,_,_> props children

[<Emit("Object.assign({}, $1, $0)")>]
let (* want private but need public for inline *) addProps (_additionalProps: obj) (_originalProps: 'Props) : 'Props = jsNative

type MakeFnComponent<'Props> = 'Props -> seq<Fable.React.ReactElement> -> Fable.React.ReactElement

let inline makeFnConstructor
    (displayName: string)
    (fn: 'Props -> Fable.React.ReactElement)
    : MakeFnComponent<'Props> =
    // FunctionComponent.Of caches passed generic function based on source code file, line number and 'Props full type name
    let fnComponent : ('Props -> ReactElement) = FunctionComponent.Of (fn, displayName)
    fun (props: 'Props) (children: seq<ReactElement>) ->
        let maybeUpdatedProps =
            match children |> Seq.toList with
            | []                             -> props
            | [child] when child = noElement -> props
            | childrenList ->
                addProps
                    (JsInterop.createObj [("children", (Fable.React.Helpers.fragment [] childrenList) :> obj)])
                    props
        fnComponent maybeUpdatedProps


type FnComponentActions = interface end
let NoFnComponentActions = () :> obj :?> FnComponentActions

let inline makeFnComponent
    (fullyQualifiedName: string)
    (hasStyles: bool)
    (actions: FnComponentActions)
    : MakeFnComponent<'Props> =
    let fn : 'Props -> Fable.React.ReactElement = fun (props: 'Props) ->
        let styles         = getStyles         fullyQualifiedName hasStyles
        let renderFunction = getRenderFunction fullyQualifiedName
        renderFunction (props?children, props, (), (), actions, styles)
    makeFnConstructor fullyQualifiedName fn

let NoActions (_: obj): unit = ()

open Fable.Core.JsInterop
let private reactIs: obj = importDefault "react-is"
let isNoElementOrEmptyFragmentOrEmptyArray (el: ReactElement) : bool =
    if el = noElement then
        true
    else
        if reactIs?isFragment el then
            isNullOrUndefined (el?props?children)
        else
            if !!el && (el?length = 0) then
                true
            else
                false

let unpackIfFragment<'T> (el: ReactElement) : 'T =
    if reactIs?isFragment el then
        el?props?children :> obj :?> 'T
    else
        el :> obj :?> 'T

type CEAccumulator = seq<ReactElement>

type ElementsBuilder() =
    member _.Zero () : CEAccumulator =
        Seq.empty

    member _.Yield (e: ReactElement) : CEAccumulator =
        Seq.ofOneItem e

    member _.Yield (maybeE: Option<ReactElement>) : CEAccumulator =
        match maybeE with
        | None   -> Seq.empty
        | Some e -> Seq.ofOneItem e

    member _.Yield (maybeEs: Option<ReactElements>) : CEAccumulator =
        match maybeEs with
        | None    -> Seq.empty
        | Some es -> es

    member _.Yield (es: List<ReactElement>) : CEAccumulator =
        es

    member _.Yield (es: seq<ReactElement>) : CEAccumulator =
        es

    member _.Yield (ess: list<array<ReactElement>>) : CEAccumulator =
        !!(Seq.ofList ess)

    member _.Yield (ess: seq<array<ReactElement>>) : CEAccumulator =
        !!ess

    member _.Yield (ess: seq<list<ReactElement>>) : CEAccumulator =
        ess |> Seq.collect id

    member _.Combine (moreEs: List<ReactElement>, es: CEAccumulator) : CEAccumulator =
        Seq.append moreEs es

    member _.Combine (moreEs: seq<ReactElement>, es: CEAccumulator) : CEAccumulator =
        Seq.append moreEs es

    member _.Combine (moreEs: List<ReactElement>, esf: unit -> CEAccumulator) : CEAccumulator =
        Seq.append moreEs (esf())

    member _.Combine (moreEs: seq<ReactElement>, esf: unit -> CEAccumulator) : CEAccumulator =
        Seq.append moreEs (esf())

    member _.Combine (moreEs: List<List<ReactElement>>, es: CEAccumulator) : CEAccumulator =
        Seq.append (List.flatten moreEs) es

    member _.Delay (expr: unit -> CEAccumulator) : unit -> CEAccumulator = expr

    member _.Run (f: unit -> CEAccumulator) : ReactElements =
        f() |> Array.ofSeq

let elements = ElementsBuilder()

type ElementBuilder() =
    member _.Zero () : CEAccumulator =
        Seq.empty

    member _.Yield (e: ReactElement) : CEAccumulator =
        Seq.ofOneItem e

    member _.Yield (maybeE: Option<ReactElement>) : CEAccumulator =
        match maybeE with
        | None   -> Seq.empty
        | Some e -> Seq.ofOneItem e

    member _.Yield (maybeEs: Option<ReactElements>) : CEAccumulator =
        match maybeEs with
        | None    -> Seq.empty
        | Some es -> es

    member _.Yield (es: List<ReactElement>) : CEAccumulator =
        es

    member _.Yield (es: seq<ReactElement>) : CEAccumulator =
        es

    member _.Yield (ess: list<array<ReactElement>>) : CEAccumulator =
        !!(Seq.ofList ess)

    member _.Yield (ess: seq<array<ReactElement>>) : CEAccumulator =
        !!ess

    member _.Yield (ess: seq<list<ReactElement>>) : CEAccumulator =
        ess |> Seq.collect id

    member _.Combine (moreEs: List<ReactElement>, es: CEAccumulator) : CEAccumulator =
        Seq.append moreEs es

    member _.Combine (moreEs: seq<ReactElement>, es: CEAccumulator) : CEAccumulator =
        Seq.append moreEs es

    member _.Combine (moreEs: List<List<ReactElement>>, f: unit -> CEAccumulator) : CEAccumulator =
        Seq.append (List.flatten moreEs) (f())

    member _.Combine (moreEs: List<ReactElement>, f: unit -> CEAccumulator) : CEAccumulator =
        Seq.append moreEs (f())

    member _.Combine (moreEs: seq<ReactElement>, f: unit -> CEAccumulator) : CEAccumulator =
        Seq.append moreEs (f())

    member _.Combine (moreEs: List<List<ReactElement>>, es: CEAccumulator) : CEAccumulator =
        Seq.append (List.flatten moreEs) es

    member _.Delay (expr: unit -> CEAccumulator) : unit -> CEAccumulator = expr

    member _.Run (f: unit -> CEAccumulator) : ReactElement =
        let elements = Array.ofSeq (f())
        Fable.React.Helpers.fragment [] elements

let element = ElementBuilder()

let asFragment (els: seq<ReactElement>) : ReactElement =
    match Seq.length els with
    | 0 -> noElement
    | 1 -> Seq.head els
    | _ -> Fable.React.Helpers.fragment [] els

let fragmentOfList (els: list<ReactElement>) : ReactElement =
    match els with
    | [] -> noElement
    | [head] -> head
    | _ -> Fable.React.Helpers.fragment [] els

type Fable.React.IHooks with
    member this.useDisposableFn (dispose: unit -> unit, dependencies: array<obj>) : unit =
        this.useEffectDisposableFn(NoopFn, dispose, dependencies)

    member this.useEffectDisposableFn (effect: unit -> unit, dispose: unit -> unit, dependencies: array<obj>) : unit =
        this.useEffectDisposable (
            (fun () ->
                effect ()
                { new System.IDisposable with
                    member _.Dispose() =
                        dispose ()
                }
            ),
            dependencies
        )

[<Import("Children", "react")>]
let ReactChildren: obj = jsNative

let tellReactArrayKeysAreOkay (els: array<ReactElement>) : array<ReactElement> =
    ReactChildren?toArray els

type ReactChildrenProp = array<ReactElement>

// because react doesn't differentiate between single and multiple elements,
// but in F# we have no way of orchestrating the same setup without casts
let castAsElement (elements: array<ReactElement>) : ReactElement =
    !!elements

let castAsElements (element: ReactElement) : array<ReactElement> =
    !!element

let castAsElementAckingKeysWarning (elements: array<ReactElement>) : ReactElement =
    !!(tellReactArrayKeysAreOkay elements)

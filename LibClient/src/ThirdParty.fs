module LibClient.ThirdParty

open LibClient.JsInterop

let private isJsArray (value: obj) : bool =
    value?push <> Undefined

let fixPotentiallySingleChild (children: array<Fable.React.ReactElement>) : array<Fable.React.ReactElement> =
    match isJsArray children with
    | true -> children
    | false -> [|children :> obj :?> Fable.React.ReactElement|]

let wrapComponent<'Props>(rawFn: obj) : ('Props -> array<Fable.React.ReactElement> -> Fable.React.ReactElement) =
    fun (props: 'Props) (children: array<Fable.React.ReactElement>) ->
        Fable.React.ReactBindings.React.createElement(rawFn, props, fixPotentiallySingleChild children)

let wrapComponentTransformingProps<'Props>(rawFn: obj) (transformProps: 'Props -> obj) : ('Props -> array<Fable.React.ReactElement> -> Fable.React.ReactElement) =
    fun (props: 'Props) (children: array<Fable.React.ReactElement>) ->
        Fable.React.ReactBindings.React.createElement(rawFn, transformProps props, fixPotentiallySingleChild children)

let wrapComponentMaybeTransformingProps<'Props>(rawFn: obj) (maybeTransformProps: 'Props -> Option<obj>) : ('Props -> array<Fable.React.ReactElement> -> Fable.React.ReactElement) =
    fun (props: 'Props) (children: array<Fable.React.ReactElement>) ->
        match maybeTransformProps props with
        | Some transformedProps -> Fable.React.ReactBindings.React.createElement(rawFn, transformedProps, fixPotentiallySingleChild children)
        | None                  -> Fable.React.Helpers.nothing

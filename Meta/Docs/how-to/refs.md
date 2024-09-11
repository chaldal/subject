# React Refs

React refs are best avoided, but there's pretty much no subsitute for them when them when you need to
deal with focus, selection, and sometimes animation.

# High level

We have a high level component, `LC.With.Ref` that manages a ref for you. Here's an example:

```xml
<LC.With.Ref
 rt-prop-children='With(bindInput, maybeInput: Option&lt;LibClient.Components.Input.Text.ITextRef&gt;)'
 OnInitialize='fun (input: LibClient.Components.Input.Text.ITextRef) -> input.RequestFocus()'>
    <LC.Input.Text
     ref='bindInput'
     Label='"Things"'
     Value='estate.Value'
     Validity='Valid'
     OnChange='actions.OnValueChange'/>
    <LC.Button
     rt-mapo='input := maybeInput'
     Label='"Select All The Things"'
     State='~Actionable (fun _ -> input.SelectAll())'/>
</LC.With.Ref>
```

If you need to grab a ref of a DOM element, use the `LC.With.RefDom` component, and make sure you
uppercase the `Ref` attribute on the DOM element, e.g. `<dom.input Type='"file"' Ref='bindInput'/>`.
The API that Fable's React bindings library provides is slightly different in the case of DOM nodes.

## Low level

If for some reason `LC.With.Ref` is insufficient for your needs, and you need to roll your own, read on.

There's a serious quirk resulting from the combination of Fable's F# to JS compilation and React's
mechanism for dealing with refs that makes it tricky to get refs working correctly.

React needs to know if a given ref is a new one or an existing one, and it keeps track of that by
doing simple reference comparisons. If a new ref comes in (whenever props are sent in, i.e. on render
of the parent component), React will first send a "null" to (I think) the original ref, as a way of
telling it that it's getting unsubscribed, and then send the actual value to the new ref.

Now, typically we'll be setting refs using actions, like this: `ref='actions.RefInputField'`. This
should just work, but what happens during Fable compilation makes things break pretty badly. Member
functions in Fable land get compiled to simple, stand-alone module functions, with the first prameter
being the `this` reference. So if you write

```fsharp
type Foo =
    member _.Blah (someValue) =
        someValue * 2
```

you'll get something like this in JS:

```js
module Foo

function Foo$$Blah ($$this, someValue) {
    someValue * 2
}
```

Then if in some other code, you happen to say (contrived example)

```fsharp
let result = myFoo.Blah 42
```

what you get in JS is actually something like this:

```js
const result = (function (someValue) { return Foo$$Blah (myFoo, someValue) })(42)
```

What Fable is doing is first binding the function to an actual instance of the class,
and only then calling it with the parameter.

This works find behind the scenes most of the time, except when you want the function
literal to be to be stable for by-reference comparisons, as you do with React refs.

So we need a way to solve this mess while preserving the following properties:

* can pass refs so that they are not considered new on every re-render
* the syntax for declaring actions needs to remain clean and redundancy-free
* in particular, declaring and separately implementing the Actions interface is
  really annoying visual duplication that we don't want to allow
* it should be fairly easy to write, and straight-forward to understand
* ideally we'd retain the duplication-free property of prototypes, i.e. the function
  literals are not declared per-instance, but per-class
* if we can't have the above, at least only a subset of actions can be per-instance,
  while the remaining can be per-class

Here's what we end up with.

In the component class:

```fsharp
    let mutable maybeJumpToPageNumber: Option<LibClient.Components.InputPositiveInteger.InputPositiveIntegerRef> = None
    member _.MaybeJumpToPageNumber
        with get () = maybeJumpToPageNumber
        and  set (value: Option<LibClient.Components.InputPositiveInteger.InputPositiveIntegerRef>) : unit =
             match (maybeJumpToPageNumber, value) with
             | (None, Some input) ->
                 input.SelectAll()
                 input.RequestFocus()
             | _ -> Noop

             maybeJumpToPageNumber <- value
```

And the Actions class looks like this:

```fsharp
and Actions<'T>(this: Grid<'T>) =
    let bound = {|
        RefJumpToPageNumber = fun (nullableInstance: LibClient.JsInterop.JsNullable<LibClient.Components.InputPositiveInteger.InputPositiveIntegerRef>) ->
            this.MaybeJumpToPageNumber <- nullableInstance.ToOption
    |}
    member _.Bound = bound

    member _.ResizePage (data: PaginatedGridData<'T>) (newPageSize: PositiveInteger) : unit =
        data.GoToPage newPageSize PositiveInteger.One None
```

The `RefJumpToPageNumber` is a "special", "bound" action, while the `ResizePage` is a normal action.
Basically we end up using a local `let` to construct a bound value, and then we use an anonymous record
to create a type-safe "dictionary" or actions, while avoiding the unbound "this.SomeAction" issue that
having a class would bring about.

At call site, we simply have:

```
ref='actions.Bound.RefJumpToPageNumber'
```
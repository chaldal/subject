# RenderDSL Introduction

RenderDSL is a domain-specific language for building React component render functions.
It is similar to JSX, except control flow (if, match, loop/map, etc) are also implemented
as XML tags and attributes.


## Under the Hood

The source `.render` files get processed by the RenderDSL compiler, and `.Render.fs` F# files
are produced as a result. The generated code does a fair bit of heavy lifting, and is that not
particularly readable.

Unfortunately we currently do not have source mapping implemented, so when you make a mistake
or a typo in the `.render` file, the resulting F# error will be reported in the `.Render.fs`
file. If you keep to a very short edit-save-compile cycle, it will usually be trivial to figure
out where the errors are happening based on "see where you last touched" approach. If you edited
a bunch of things before saving, and get multiple errors, you may have to look inside the
generated `.Render.fs` file to figure out exactly where you messed up.

For brevity and ease of consumption, we will describe the various RenderDSL constructs at the
conceptual level, without actually showing the resulting F# code that the constructs generate to.
If you want to know exactly how various RenderDSL constructs get translated into F# code, the best
place to look is the unit test suite, where simple `.render` files with isolated constructs are
presented in pairs with the resulting `.Render.fs` files. See `Meta/AppRenderDslCompiler/compiler.Tests/tests/`.


## Language Constructs

### Summary

The following language constructs are available:

* `rt-if` attribute allows you to render an element conditionally
* `rt-let` attribute allows you to bind an expression to a name, just like the F# `let`
* `rt-map` attribute allows you to map items in a `Seq<'T>` to elements
* `rt-mapi` attribute allows you to do the same, with an index
* `rt-mapo` attribute allows you to map a `Some` optiona value to elements
* `rt-match` and `rt-case` tag allow you to pattern match, much like the `match` and `|` in F#
* `rt-block` tab allows you to wrap some elements in a noop wrapper, to which you can add
  control flow attributes
* A couple flavours of string interpolation are supported
* Passing react elements through props in various ways is supported
* `rt-class` and `class` attributes allow styling of both DOM elements and ReactXP base components
* `rt-prop` tag allows you to wrap some elements to pass them through props to the parent component,
  which enables construction of various higher-order components
* `rt-outer-let` tag is for special cases of using children to construct props that are not served
  by `rt-props` alone
* `rt-prop-children` is shorthand for when all children should be passed through a designated prop
* `rt-with` is shorthand for when all children should be passed to the `With` prop
* `~` back-reference operator allows you to reference types/functions/union cases defined in the
  component's module without typing the module name out explicitly
* `^` polymorphic prop constructor operator allows you to call a static MakePropName member function on
  the given component's Props type, which allows for polymorphic overloads
* `rt-root` tag allows you to define type parameters for generic components using the `rt-type-parameters`
  attribute
* `rt-open` attribute, allowed on the root node of any component, allows you to open modules, much like
  you would in F#
* `rt-let` tag allows you to wrap some elements as a named value, which is useful for in-place templating,
  like in the case of responsive design
* a way to "auto-wrap" optional props, as a way of significantly reducing verbosity

Below are detailed explanations and examples.

### The `rt-if` attribute

A simple conditional applied to a node.

```xml
<div rt-if='condition'>
    Some stuff we want to show if condition is true
</div>
```

If `condition` is `true`, the element gets rendered, otherwise it does not. The `expression` is
any valid F# expression of `bool` type.


### The `rt-let` attribute

Allows to bind a value to a name, just like the regular F# `let`.

```xml
<div rt-let='name := state.user.name; age := state.user.age'>
    Hello, {name}, you are {age} years old.
</div>
```

If you bind more than a couple values, or if the expressions are long, you may want to
break things up into multiple lines:

```xml
<div rt-let='
    name := state.user.name;
    age  := state.user.age;
'>
    Hello, {name}, you are {age} years old.
</div>
```

`rt-if` has higher precedence, so `rt-let` bindings are not available inside the
`rt-if` expression, and the `rt-let` bindings only get constructed if the `rt-if`
condition is `true`.

```xml
<!-- this will NOT compile, since `age` is not defined -->
<div rt-let='age := state.user.age' rt-if='age >= Law.AdultAge'>
    Explicit adult content for your {age}-year-old self
</div>

<!-- instead you have to do this -->
<div rt-let='age := state.user.age' rt-if='state.user.age >= Law.AdultAge'>
    Explicit adult content for your {age}-year-old self
</div>
```


### The `rt-map` attribute

Lets you iterate over a `seq<'T>` and map every `'T` to some components. For example,

```xml
<div rt-map='fruit := ["banana"; "apple"; "mango"]'>
    The fruit is {fruit}
</div>
```

will expectedly produce

```xml
<div>The fruit is banana</div>
<div>The fruit is apple</div>
<div>The fruit is mango</div>
```


The `rt-if` attribute has higher precedence:

```xml
<!-- nothing will be displayed -->
<div
 rt-map='fruit := ["banana"; "apple"; "mango"]'
 rt-if='false'>
    The fruit is {fruit}
</div>

<!-- this will NOT compile because the `if` wraps the `map`
     on the outside, i.e. fruit is not defined -->
<div
 rt-map='fruit := ["banana"; "apple"; "mango"]'
 rt-if='fruit.Length > 5'>
    The fruit is {fruit}
</div>
```

The `rt-let` attribute has lower precedence, so the bindings are in the context of each iteration.

```xml
<div rt-map='fruit := ["banana"; "apple"; "mango"]' rt-let='charCount := fruit.Length'>
    The fruit is {fruit} and it has {charCount} characters
</div>
```

Since the stuff on the left of the `:=` operator just becomes the parameter to the map function,
you can do regular F# destructuring here, like this:

```xml
<div rt-map='(name, price) := [("banana", 15); ("apple", 20); ("mango", 12)]'>
    The fruit {name} costs {price} dollars.
</div>
```


### The `rt-mapi` attribute

Lets you iterate over a `seq<'T>` with an index:

```xml
<div rt-mapi='index fruit := ["banana"; "apple"; "mango"]'>
    The fruit at index {index} is {fruit}
</div>
```


### The `rt-mapo` attribute

Lets you map over an `Option<'T>` (it is natural to think of "mapping" over an option, since an option is
essentially a collection of zero or one elements):

```xml
<div rt-mapo='fruit := Some "banana"'>
    The fruit was a {fruit}
</div>

<div rt-mapo='fruit := None'>
    The fruit was a {fruit}
</div>
```

will result in

```xml
<div>The fruit was a banana</div>
```

### The `rt-match` and `rt-case` nodes

The workhorse for type-safe consumption of union types. Maps directly to the F# `match` statement.

```xml
<rt-match what='maybeUser'>
    <rt-case is='None'>no user</rt-case>
    <rt-case is='Some Anonymous'>anonymous user</rt-case>
    <rt-case is='Some (LoggedIn username)'>Logged in user {username}</rt-case>
</rt-match>
```


### The `rt-block` node

Sometimes it is necessary to wrap a list of nodes in a directive like `rt-if` or `rt-map`, without
a wrapper DOM/React element. This is what `rt-block` allows you to do.

```xml
<rt-block rt-if='true'>
    some text
</rt-block>
```

renders as

```xml
some text
```

And something like this:

```xml
<rt-block rt-map='curr := [1; 2]'>
    <div>sibling A {curr}</div>
    <div>sibling B {curr}</div>
</rt-block>
```

renders as

```xml
<div>sibling A 1</div>
<div>sibling B 1</div>
<div>sibling A 2</div>
<div>sibling B 2</div>
```


### String interpolation

There are two places where you "put stuff" in react templates — XML node attributes, and text nodes:

```xml
<div>
    Some text
    <Button Label='expression'/>
</div>
```

Text nodes are treated as interpolated strings by default, which means that you can embed expressions
like so:

```plaintext
Hello, {state.User.Name}, welcome to {props.SiteName}.
```

Attributes are interpolated _only_ if the whole expression is surrounded by backticks, like so:

```xml
<Button Label='`Click to earn {state.price} dollars`'/>
```

Otherwise you're out of luck, and have to use the absurd F# string formatting functions. How a language
as advanced as F# could have such abysmal string building primitives is beyond me. (Two years after the
original writing, F# 5 now has string interpolation. We will upgrade in a few months).

There's one exception to the attributes rule — the `class` attribute. Since a pretty standard use case
is to have something like `class='button button-level-{props.Level}'`, the class attribute's value
is treated as an interpolated string by default, not an expression.


### React elements as values

The aforementioned string interpolation implicitly forces toStringification of values, but there are
times within when you need to drop in an actual value, for example for `props.children`.
In this case, you do

```xml
<div>
    <text>Some text</text>
    {=props.children}
</div>
```

To keep things typed, components that want to access `props.children`, which behind the scenes is available
to all components, but due to F#'s lack of support for inheritance in record types cannot be added to all `Props`
automatically, need to declare the field on `Props` explicitly, like so:

```fsharp
type Props = (* GenerateMakeFunction *) {
    children: ReactElement // default FRH.nothing
}
```


## The `rt-class` and `class` attributes

There are a few standard use cases for classes, as illustrated by the following example:

```xml
<div
 class='button button-size-{props.size}'
 rt-class='
     disabled             := props.disabled;
     `button-{someValue}` := someCondition;
 '>
    foo
</div>
```

* A static class — put it in `class`
* A dynamically named but statically present class — put it in `class` and use string interpolation
  for the dynamic name
* Statically named, conditionally present class name — put it in the `rt-class` attribute as a `:=`
  separated name and `bool` expression pair
* Dynamically named, conditionally present class — put it in the `rt-class` attribute, using the
  backtick syntax for the name

On DOM elements, classes just get added to the element. For ReactXP components, based on the class
names that end up on leaf node elements, actual style rules are extracted from the style sheet, and
passed into the component's `style` prop. See the [StylesDSL documentation](./stylesDsl/index.md)
for details.


### The `rt-prop` node

This allows you to wrap some elements up and pass them as a prop to the parent component.
There are a few possibilities for the `name` attribute.

When you component has a prop of type `ReactElement`, like this:

```fsharp
type Props<'T> = (* GenerateMakeFunction *) {
    Padding: int
    Body:    ReactElement
}
```

then you can pass the elements to it using `rt-prop` with simply the name of the prop
in the `name` attribute:

```xml
<Padded Padding='20'>
    <rt-prop name='Body'>
        <div>Some stuff</div>
    </rt-prop>
<Padded>
```

Sometimes the component may want to give you some data and ask you for some `ReactElement`s in return.
Perhaps we have a component `With.Details` that internally keeps state of whether the details section
is expanded or contracted, and it has the following props:

```fsharp
type Props<'T> = (* GenerateMakeFunction *) {
    Body: (* isExpanded *) bool -> ReactElement
}
```

Here is how we would use such a component:

```xml
<With.Details>
    <rt-prop name='Body(isExpanded: bool)'>
        <div>This is the summary</div>
        <div rt-if='isExpanded'>And here are the details</div>
    </rt-prop>
</With.Details>
```

Though this example is somewhat contrived. A more likely way we'd build the `With.Details` component
is with following props:

For an example of how to declare props to support this sort of stuff, look at `AsyncData`:

```fsharp
type Props<'T> = (* GenerateMakeFunction *) {
    Details:  ReactElement
    children: ReactElement
}
```

and use it like this:

```xml
<With.Details>
    <rt-prop name='Details'>
        <div>And here are the details</div>
    </rt-prop>
    <div>This is the summary</div>
</With.Details>
```

the `.render` file of `With.Details` would look something like this:

```xml
<div onPress='actions.ToggleIsExpanded'>
    <div class='summary'>{=props.children}</div>
    <div rt-if='estate.IsExpanded'>{=props.Details}</div>
</div>
```

Though notice that this always constructs the elements for the details view, regarless of whether
we actually render them in the end of not. This is wasteful, and so we should make details lazy,
by changing the props to this:

```fsharp
type Props<'T> = (* GenerateMakeFunction *) {
    Details:  unit -> ReactElement
    children: ReactElement
}
```

the use site to this:

```xml
<With.Details>
    <rt-prop name='Details(_)'>
        <div>And here are the details</div>
    </rt-prop>
    <div>This is the summary</div>
</With.Details>
```

and the `.render` file of `With.Details` to this:

```xml
<div onPress='actions.ToggleIsExpanded'>
    <div class='summary'>{=props.children}</div>
    <div rt-if='estate.IsExpanded'>{=props.Details ()}</div>
</div>
```

Finally, imagine we had a need for a `Message` component that most of the time is used to display
a simple string message, but in a few rare cases is required to allow the flexibility of passing
raw `ReactElement`s. It may have the following props:

```fsharp
type Value =
| String of string
| Raw    of ReactElement

type Props<'T> = (* GenerateMakeFunction *) {
    Value: Value
}
```

In the string case, we would use it like this:

```xml
<Message Value='~String "Hello, World!"'/>
```

(ignore the `~` for now, we'll get to it shortly)

and in the `ReactElement` case, like this:

```xml
<Message>
    <rt-prop name='Value |> ~Raw'>
        <div class='special'>Special greetings to the world!</div>
    </rt-prop>
</Message>
```

This pipe syntax also works for parametrized props, or union cases with other fields.
For a contrived example:

```xml
<rt-prop name='Value(isExpanded: bool) |> fun children -> ~Raw (children, 42)'>
```


### The `rt-prop-children` attribute

In many cases, a component will have one "main" prop for taking in children. In such cases,
using an `rt-prop` is verbose, redundant, and adds a level of nesting:

```xml
<With.Details>
    <rt-prop name='Details'>
        And here are the details
    </rt-prop>
</With.Details>
```

So instead, we can use a shorthand to mean "children go into this prop":

```xml
<With.Details rt-prop-children='Details'>
    And here are the details
</With.Details>
```

### The `rt-with` attribute

In many cases, a component will provide some data that the children use to render themselves.
One can orchestrate this in an ad hoc way using either `rt-prop` or `rt-prop-children`, as in:

```xml
<With.Details rt-prop-children='Content(details: Details)'>
    And here are the details: {details}
</With.Details>
```

This pattern is so common, though, that in order to declutter our `.render` files we introduce
some convention based syntactic sugar. Name your data providing prop `With`, and write the above
in this shorthand:

```xml
<With.Details rt-with='details: Details'>
    And here are the details: {details}
</With.Details>
```

### The `rt-let` node

USE SPARINGLY for naming values in one place and using them in another introduces indirection,
and indirection makes code hard to follow. The only place where it's okay to use `rt-let`, and in
fact the place that `rt-let` was added to RenderDsl for, is responsive design. Typically you have
a bunch of content chunks that are placed inside a light layout structure which differs slighly
between handheld and desktop. In this case, it's sometimes quite difficult to accommodate the
slight variation in layout cleanly, without duplicating the content chunks. This can quickly lead
to nothing short of a nightmare of code duplication.

### The `rt-outer-let` node

This tag allows you to wrap some React elements and name them, much like `rt-let`, but while `rt-let`
does this at the place where the node appears, `rt-outer-let` actually defines the block _outside_ the
tag that it's lexically sitting in, so that it can be then passed in through `rt-prop`. Before the `|>`
operator was introduced, given a prop of type `SomeWrapper of ReactElement`, we need to
`rt-outer-let name='blah'` the elements inside the `<Foo>` component, and then pass the wrapped prop
by saying `<Foo SomeProp='~SomeWrapper foo'>`. All together:

```fsharp
type Props = {
    SomeProp: SomeProp
}

and SomeProp =
| SomeWrapper of ReactElement
```

```xml
<Foo SomeProp='~SomeWrapper blah'>
    <rt-let name='blah'>
        elements here
    </rt-let>
</Foo>
```

Since the introduction of the `|>` operator, this can be accomplished with

```xml
<Foo>
    <rt-prop name='SomeProp |> ~SomeWrapper'>
        elements here
    </rt-prop>
</Foo>
```

which is more concise. There are still some legacy usage of `rt-outer-let`, but for the most
part, we don't think it should be necessary.


### The Back reference operator `~`

We want to use union types for props, for example `type Level = Info | Warning | Error` in some
kind of notification component. On the use site, ideally you want to be able to say

```xml
<Notification level='Warning'>Don't get carried away!</Notification>
```

Given our strong desire to avoid "open clobber free-for-all" that can lead to various hard-to-debug
scenarios, we do not `open` each component's `.fs` module in the render function `.fs` file, so
things like `Warning` will actually not be in scope. This forces you to write `level='Notification.Level.Warning'`,
which is a deal-breaking level of visual noise.

The solution we came up with is to have a special operator, usable only within props, which expands to
the fully qualified component name. The operator is `~`. The expansion is at the moment a naive string
subsitution, and we're hoping the tilda is a rare enough character that this does not cause problems
in practice (we didn't pay the price of doing full parsing on expressions yet, so this is currently
not easily doable "the right way").

As a result, we can now do this, which is almost the ideal

```xml
<Notification level='~Warning'>Don't get carried away!</Notification>
```

There will be times when your prop is of a union type that's declared elsewhere. For example, there may be

```fsharp
type State =
| Actionable of OnPress
| InProgress
| Disabled
```

in some `Button.typext.fs`, and you're building `SpecialButton.typext.fs`, whose `State` prop you want to be
of type `State` above. You can alias the type:

```fsharp
type State = Components.Button.State
```

but trying to do `<SpecialButton State='~InProgress'/>` will not work. You've aliased the type, but not the
constructors for the individual cases. To do the latter and make this snippet work, you'll need to add

```fsharp
let Actionable = State.Actionable
let InProgress = State.InProgress
let Disabled   = State.Disabled
```

below the type alias.


### The `^` polymorphic prop constructor operator

Consider a <Timestamp> component that renders the provided value as a timestamp, perhaps using a provided
format string. It would be desirable to be able use this components with a variety of types that are used
to represent time, e.g. `DateTimeOffset`, `int` for Unix time, and `string` for an ISO date-time string.
What are our options for the `Timespan` component's props?

This would clearly be, in the F# world, a non-option because of the possibility of providing more than one value:

```fsharp
type Props = {
    Value:     DateTimeOffset
    ValueUnix: int
    ValueISO:  string
}
```

but we list it here for completeness. The cleaner way to do the same and stay in good modelling territory is:

```fsharp
type Value =
| Default of DateTimeOffset
| Unix    of int
| ISO     of string

type Props = {
    Value: Value
}
```

The use sites would then look like this:

```xml
<Timestamp Value='~Default someValue'/>
<Timestamp Value='~Unix someMillis'/>
<Timestamp Value='~ISO someString'/>
```

This is not so bad, but stating the type explicitly here adds no value, yet takes up our attention, and in doing
so takes attention away from more important code. I.e. it is incidental noise brought about by the language/runtime,
and if we can get rid of it, we should. What we want, is basically a set of polymorphic functions that take the various
inputs that we support, and produce the unified output, and it should be the job of the compiler to pick the correct one
based on the provided data. In F#, the only place where we're allowed polymorphic functions is member functions on types,
no other choices there. So we have to do something like this:

```fsharp
type Value =
| Default of DateTimeOffset
| Unix    of int
| ISO     of string

type Props = {
    Value: Value
}

type PropValueFactory =
    static member Make (input: DateTimeOffset) : Value =
        Default input

    static member Make (input: int) : Value =
        Unix input

    static member Make (input: string) : Value =
        ISO input
```

at the use site we can then say:

```xml
<Timestamp Value='~Props.MakeValue someValue'/>
<Timestamp Value='~Props.MakeValue someMillis'/>
<Timestamp Value='~Props.MakeValue someString'/>
```

this is now nice and uniform, only pointlessly verbose. Luckily, if we fix the naming convention
for these polymorphic prop constructor functions, we can simply autogenerate the correct call.
This is eactly what the `^` operator does. The above is equivalent to this:

```xml
<Timestamp Value='^ someValue'/>
<Timestamp Value='^ someMillis'/>
<Timestamp Value='^ someString'/>
```

Now we only have the essentials in front of us, and the noise hidden away in a single `^` character.

We also support custom suffixes on the `Make` functions to increase readability. For example, if we want
to remind the user that they need to be careful about a certain flavour of the make function, we can have
a warningful name:

```fsharp
type PropValueFactory =
    static member Make (input: DateTimeOffset) : Value =
        Default input

    static member MakeLowLevel (input: int) : Value =
        Unix input
```

then at the use site

```xml
<Timestamp Value='^ someValue'/>
<Timestamp Value='^LowLevel someMillis'/>
```

Note that having one factory type per prop type may feel a little verbose, but it allows you to reuse
the factory type between components, which is hugely important. For example, all of `LC.Button`,
`LC.IconButton`, `LC.TextButton`, and `LC.FloatingActionButton` reuse a factory for the `State` prop.

(see a note in [Background](./unsorted/background.md) on why we couldn't get away without the `^` character)

### The `rt-root` tag

Only allowed as the root element of your `.render` file. This tag is the only one that's allowed to
have the `rt-type-parameters` attribute, which is necessary for declaring generic components. The
value of this attribute is dropped directly into the appropriate `<...>` expressions in the generated
F# code, so the values should be set appropraitely. E.g. `'T` for a single parameter, `'K, 'V` for multiple.
Keep the names for type parameters consistent between your `.render` and `.typext.fs` files.

### The `rt-open` root node-only attribute

Allows you to add additional `open` statements to the generated render function's `.fs` file. Use with
caution, since clobber case occur otherwise.

```
rt-open='Some.Module'
```

will result in

```fsharp
open Some.Module
```

while

```
rt-open='SM := Some.Module'
```

will result in

```fsharp
module SM = Some.Module
```

For multiple, delimit with semicolon.


## Default values for props, and auto-wrapping of options

In the JS world, which React is a part of, optional values are often
modelled as nullable types. E.g. you may have props declared as

```fsharp
type Props = {
    Name:   string
    Age:    number | undefined
    Height: number | undefined
    Weight: number | undefined
}
```

When constructing an instance of such props, you can simply omit the
keys which you don't want to provide, or, in JSX, it would look like

```xml
<Person Name='"Bob"'/>
```

In F#, optional values are modelled as the `Option<'T>` type. "Omitting" a value is not a thing,
you have to explicitly provide a `None`. This makes for a very tricky situation. We clearly don't
want a RenderDSL where we have to provide all the `None`s explicity, as it gets incredibly noisy.

```xml
<Person Name='"Bob"' Age='None' Height='None' Weight='None'/>
```

Worse, when we do provide a value, we need to wrap it in a `Some`:

```xml
<Person Name='"Bob"' Age='Some 26' Height='None' Weight='Some 80'/>
```

It's a deal breaker. So we need some way of working around this. It turns out that the _only_
language consturct in F# that allows for call time optionality is the optional parameters on
class member functions. So our RenderDSL system makes use of this language feature to allow us
to declare props using `Option`s, yet have a noiseless syntax for the `.render` files. To make
it work, we employ "structured control comments", which get parsed if the file type is `.typext.fs`,
and a special maker function is then created. It's that maker function that the `.render` files
end up calling. The syntax is:

```fsharp
type Props = (* GenerateMakeFunction *) {
    Name:   string
    Age:    int           // default 0
    Height: int    option // defaultWithAutoWrap None
    Weight: int    option // defaultWithAutoWrap None
}
```

The `// default` control comment allows you to simply specify a default value for a field. These are
used fairly rarely. The more common one is the `option // defaultWithAutoWrap` control comment —
this marks a field as optional, and auto-wraps the value in a `Some` if it is provided.

This setup doesn't allow us to explicitly pass an option for the prop anymore — the mere act of
passing something implies that it's a `Some`. This is extremely inconvenient in some cases, so we
autogenerate and wire up an extra set of synthetic props, for every e.g. `Age` that has the
`defaultWithAutoWrap` control comment, there's a corresponding `AgeOption` that you can call
from the DSL and pass the `Option`-wrapped value into. E.g.:

```xml
<Person Name='"Bob"' AgeOption='someOptionValue'/>
```


## Records With Defaults Quirks (as used for Props)

One tweak we had to introduce along the way is that the Make function's parameter names are not
straight-up record field names, but are instead record field names with first letter lowercased.
This is done to work around a feature in F# where you can have a union case class descruction inside
a parameter, like so:

```fsharp
type Blah = Blah of int

let someFunction (Blah value) =
    doSomethingWith value
```

So whenever `Blah` is present in scope, you can no longer have a `Blah` parameter name in any function
in that file, it will be a type error (and should be a syntax error too, but type error is reported
first for some reason).


## How to Run Manually

Typically when using RenderDsl in a project, you would use the `eggshell dev-web` command line utility,
which will watch `.render` and `.typext.fs` files for changes, recompile them into `.Render.fs` and
`.TypeExtensions.fs` files, which will then be picked up by Fable, which will compile them in to JS,
which webpack will then reload in your browser.

If you want to try the compiler in a standalone fashion, you can pipe some code into it
on the command line. Say you have a file `test.render` with the following content

```xml
<div>
    <span rt-if='state.isBroken'>OH NO</span>
    <div rt-map='fruit := ["banana"; "apple"; "mango"]'>
        The fruit is {fruit}
    </div>
</div>
```

By calling `cat test.render | ./render-dsl-compiler Render Test` you should get the following output

```fsharp
module Components.TestRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard

open Components.Testopen Test


let render(props: Props, estate: Estate, pstate: Pstate, actions: Actions) =
    div [] [
        (
            if state.isBroken then
                span [] [
                    (FRH.str "OH NO")
                ]
            else FRH.nothing
        )
        (
            (["banana"; "apple"; "mango"])
            |> Seq.map
                (fun fruit ->
                    div [] [
                        (FRH.str (System.String.Format("{0}{1}{2}", "\n        The fruit is ", (fruit), "\n    ")))
                    ]
                )
            |> List.ofSeq
            |> FRH.fragment []
        )
    ]
```

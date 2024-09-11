# Background and Design Decisions for EggShell

## Motivation for RenderDSL

DSLs are great — they make it easier to express solutions in a given domain in a short
and concise manner. This has two benefits:
* it's typically far less verbose than in a general-purpose language, specifically
  because general-purpose languages need to be expressive enough to cover the full
  spectrum of general use cases.
* it's restrictive — there's typically only one way of doing things, unlike in
  a general-purpose language. This is great, as it leads to strong consistency
  in the code, which reduces mental overhead.

In the case of building `render` functions for React components, JSX was a nice
step towards a DSL in that it reduced verbosity, but its authors chose to forego
the second benefit — you still have full JS power at your fingertips when writing
JSX, and it's easy to write hard-to-follow, indirection-riddled code.

We are in Fable land, and the standard syntax for building DOM/component trees here
is what I termed "bracket soup". It's seriously hard to read, because the syntax for
specifying everything is uniform — can't easily tell whether you're looking at a component,
a prop, or a child. Closing brackets carry no information about where they were opened,
and F# style indentation makes it rather laborious to trace directly upwards to see what
level of indentation, and thus what component, we're currently reading. It's just painful
to try to read and understand.

So we build a DSL based on one I've been using for four years: https://github.com/wix/react-templates

The main reason we're using Fable over TypeScript is to capitalize on F#'s powerful and
expressive type system, particularly the union types part of it. So this DSL is designed with
the goal of having first-class support for union types and type safety in general.

Choices in language constructs were dictated by both ease of usage and ease of implementation — ideally
as little work beyond simple translation to F# would get the biggest gains in usability. In particular
in handling expressions, I avoided having to write a full-blown parser, by assuming things like
"the := operator never shows up in any expression, including inside strings and comments". In practice
this should be a fairly safe assumption.

A corollary of the above is that the double quote character, which is the sole string delimiter in F#
should be reserved for string delimiting, so the default character surrounding XML attribute names should
be the single quote. That way you don't need annoying escaping like `<Button label="\"OK\""/>` by doing
`<Button label='"OK"'>`. As consistency is an important goal, let us not mix single and double quotes in
attribute value deilimiters, and always stick to single. I plan to enforce this programmatically in the
future.

Note, that the `react-templates` project that inspires this work uses "strings are the default value"
approach, i.e. you can do `<Button label="OK" onClick="{actions.onClick}"/>`. In practice, we do far
more passing of values that strings, so making that the default (i.e. invisible braces on everything)
seemed like a better choice than making strings default, and it also made the implementation easier.
So we end up with `<Button label='"OK"' onClick='actions.onClick'/>`.

## Stack Design Goals

The stack was designed with the following ideal goals in mind:

* Use a strong, discriminated union-enabled type system to design away
  classes of bugs by following the "impossible states are not representable"
  approach to modelling.

* Keep concerns separated. In particular, the severely detrimental approach of
  "style, component tree, and user interaction logic all in one file"
  made default by React is not acceptable, and each of these three concerns get
  a separate file.

* Default-deny everything. For most things, there should be only one way to
  do something. This leads to consistency, which leads to low conginitive overhead
  when working with code, which leads to faster dev velocity and fewer bugs.

* Minimize congnitive overead as much as possible. Whenever visual noise can be
  abstracted away, it should be. In practice this means creating library functions
  for common patterns, overloading operators to simplify common syntax, and creating
  DSLs for common domains.

* App state should be managed in a way that the app can be rehydrated from a serialized
  state blob. As long as this goal is achieved, the blob itself need not be transparent.

* Uni-directional data flow should be enforced as much as possible.

* Subscribing on data should be syntactically sugary and straight-forward. This should
  include out-of-the-box management of loading/loaded/errored states for async data.


## Stack Reality

### Constraints

The actual implementation falls short of the ideal, for the following reasons:

* F#'s type system is pretty inflexible, and modelling common JS usage patterns (React ones
  in particular) can be fairly awkward. Optional fields on records is a great example of that.
  We've really weaved through a lot of lazer beams to get to where we are.

* F#'s requirement for strict order of compilation makes life difficult for a dynamic system.
  Much hoop-jumping is required, either by splitting code that reasonably belongs together into
  nonsensical parts (i.e. you'd never do it unless forced), or following the "provided at runtime"
  pattern (see the EvenBus example)

* F#'s DSL support is extremely limited, anything where call site dynamic types are involved is
  impossible, which rules out a huge class of DSLs. As a result, we were forced to go the text-based
  code generation route, which has the disadvantage of no support for source maps (errors and IntelliSense).
  At least not easily implementable. We're planning to do this in the future.

* Since we generate code, in theory anything is possible, but in practice we have a huge constraint
  to be careful of — React's rendering model. We must avoid unnecessary re-render costs, so shallow
  equality of props and state must be kept in mind at all times when doing dynamic behind-the-scenes
  stuff in code generation land.

* There are two "compilers", and both need to be appeased — F#/dotnet and Fable.

### Result

So, what we managed, in terms of features is:

* An XML-based DSL for component/DOM tree layout that compiles into a render function for
  the given component. This includes reasonable (i.e. not F#'s abysmal) string interpolation.

* A system for specifying styles "reasonably". It's not quite CSS, but CSS is
  probably overkill anyway, we only consider it a standard because that's what we're used to.
  What we have is a mechanism for dynamically passing style sheets/rules and class names as
  implicit props behind the scenes, as well as the system for determining applicable styles
  based on class name.

* A clean split between props, ephemeral state (e.g. dropdown is open), peristent state, and
  actions. Thus the component instance is not in scope in the render function, which makes
  for a clean default-deny approach.

* Persistent state implementation where the persistent store can be initialized at app bootstrap
  time using serialized data.

* Pattern for subscibing on data.

* Support for component libraries (using XML namespace prefix syntax).

* ReactXP components imported and wrapped to be usable in a type-safe way.

* Records with optional fields, default values for fields, and auto wrapping
  of values in Option.Some for cleanest readability of the XML based DSL.
  This is done through code generation, which means that errors will not be
  mapped to the original source file.

* Edit-save-reload cycle with what seems like reasonable speed, in the browser.
  It remains to be seen how the cycle speed scales to larger projects.
  There also seems to be a problem with errors showing up only once, and not
  on every recompile (i.e. an error in A.fs will show up when you save A.fs,
  but not when you save B.fs), so it's easy to miss errors when doing refactoring.
  This may be something we can tweak.


## Tooling Design Decisions

### The `RtCompiler` suite

Given the complexity of the parsing and code generation, it made sense to use a fully
featured, compiled language. Given that the whole stack was going to be in F#, it made
sense for the compiler to also be in F#.

### The `eggshell` command line tool

Possible choices were:
* shell scripts (dev experience abysmal)
* yeoman (built one generator with it, and was left with an impression of it being
  really unnecessarily complex)
* gulp (very flexible)
* powershell
* F#

Requirements were:
* type-safe language for sane authoring experience
* runnable on all platforms
* preferrably no additional installs
* out-of-the-box libraries for templating, CLI, file handling, etc

Non-requirements were:
* Using the same language as the `RtCompiler` suite does not necessarly make sense,
  given that requirements for scaffolding are very different from requirements for
  serious parsing and code generation.

Also of note: this tool is closer to "writing code experience" than "build/toolchain experience".
The latter is "represented" by VSCode, which is written in TypeScript, while the latter is in
dotnet/MSBuild.

Based on this, gulp and TypeScript were chosen.


## Anatomy of a Component

A component in this system has the following structure

```
src/Components/Foo/Foo.typext.fs
```

holds the class definition for the component, along with `Props`, `Pstate`, `Estate`, and `Actions`
definitions, and the `Make` factory function. `Props` are expected to be defined as a record, and
a DSL for making fields optional and providing default values is available. See `RtCompiler/README.md`
for details.

```
src/Components/Foo/Foo.style.fs
```

holds the styles for a component. This consists of the main `style : ProcessedStyleSheet` value,
which is typically defined by a

```
let styles = compile (StyleSheet [
    // rules
])
```

sort of block, any possibly some functions `(...) -> ProcessedStyleSheet` provided for users of the
component to apply customizations. The idea is that internal structure of components is private,
and components expose the ability to tweak their visuals to users through these public functions,
like `setColors(backgroundColor, textColor)`.

```
src/Components/Foo/Foo.render
```

holds the XML-based definition of the render function. The compiled function takes the `props: Props`,
`pstate: Pstate`, `estate: Estate`, `actions: Actions` as parameters, so they are in scope everywhere
in the XML.


## Why we can't have polymorphic props without the `^` character.

Read the section on the `^` operator in [RenderDSL docs](./renderDsl/index.md) first.

This implementation begs the question: for each prop, why don't we auto-generate the

```fsharp
static member MakePropName (rawValue: PropType) : PropType = rawValue
```

identity function, and _always_ call `MakePropName` for every prop, instead of doing it only for the
ones that have the `^` operator at their start?

It would indeed be nice, but there seems to be either a bug or an intended-but-unclear behaviour in F#,
where an external type extension clobbers the first (in order of definition) static member by the same
name that appears in the internal (same file as type declaration) type augmentation. So if we have

```fsharp
type Props = {
    Foo: 'V
    // props here
} with
    static member MakeFoo (v: 'V1)
    static member MakeFoo (v: 'V2)
    static member MakeFoo (v: 'V3)
```

in the `Component.typext.fs` file, and then we autogenerate

```fsharp
    static member MakeFoo (v: V) : V = v
```

in the `Component.TypeExtensions.fs` autogenerated file, then as a result, the `Props` type will have
three flavours of the `MakeFoo` function on it, the `'V2`, `'V3`, and `'V` ones, but _not_ the `'V1`.

NOTE: since we've updated the way polymorphic prop constructors work from extending `type Props` to
having a separate wrapper type per prop in order to enable sharing, the above may not necessarily apply
directly anymore.


## Other Notes

* We choose not to have a sample app in the repo. This is because we provide scaffolding for an app,
  and keeping both a sample app and the scaffolding in the repo is a good recipe for having them go
  out of sync. Instead only the scaffolding is kept around, and making a new app takes only a few minutes.
  The dev workflow for updating the scaffolding itself can be as follows:
  * Make a new git branch
  * Scafoold a new app
  * Commit it
  * Make the changes you want, test them, etc
  * Now in git diff you can see only the changes you've made, so it's relatively easy to apply them back into
    the `templates` directory.


## Remaining Work

* test on device

* packaging for device

* server-side rendering
  * runLater needs implementation

* ReactXP.Styles.mergeTwo has a naive implementaiton to just get things working. It's expensive at
runtime, so need a more intelligent implementation.

* text shadows currently don't work, see note in LibClient/src/ReactXP/Styles.fs in createReactXPStyleObject method


## Do Eventually

* transition code generation infra from gulp to msbuild/dotnet

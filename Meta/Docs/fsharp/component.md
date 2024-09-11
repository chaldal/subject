# How to Add a Component

## Basics

A component is a static member on some type, with the `[<Component>]` attribute
decorating it, that returns a `ReactElement`. The member cannot have curried parameters.
For example:

```fsharp
type Ui with
    [<Component>]
    static member Link (shipmentId: ShipmentId, ?styles: array<TextStyles>) : ReactElement =
        let nav = useNavigation ()
        LC.TextButton (
            label   = $"{shipmentId.DisplayString}",
            state   = Input.ButtonHighLevelStateFactory.MakeGo (Debug (DebugRoute.Shipment shipmentId), nav),
            ?styles = styles
        )
```

These function typically have some required and some optional parameters. We follow
regular F# readability sensibilities when deciding whether to name the parameters at
the call site or use them positionally.

## Namespaced components

We want to have the ability to nest components in namespaces, for nice grouping,
readability, and IDE intellisense style support.

The way we achieve namespacing of components, and declaration of components in general,
is through type extensions on predefined types. This is why you see the `with` in the
`type Ui with` above. This type was actually predefined in a special file,
`ComponentsHierarchy.fs`, which exists in every app and lib. For example:

```fsharp
[<AutoOpen>]
module AppExample.Components.Constructors

type Ui = class end

module Ui =
    type Route = class end
    type Debug = class end

    type App = class end

    type ShipmentTag = class end

    module Route =
        type Debug = class end
        type Audit = class end
        type Admin = class end

    type With = class end

    module With =
        type Packages = class end
        type Shipments = class end
        type Stations = class end
        type Session = class end
```

Observe a number of things.

First, the module has the `[<AutoOpen>]` attribute. This means the types will be made available
to anybody who `open AppExample.Components`. Incidentally, any module where you define a component
should be a direct child of `AppExample.Components`, and should be `[<AutoOpen>]`'ed, e.g.

```
[<AutoOpen>]
module AppExample.Components.Link
```

This applies to namespaced components as well, e.g.:

```
[<AutoOpen>]
module AppExample.Components.Route_Admin_ShipmentAudit

// opens redacted for brevity

type Ui.Route.Admin with
    [<Component>]
    static member ShipmentAudit (shipmentId: ShipmentId) : ReactElement =
        // redacted for brevity
```

Note the underscores in the module name. So these modules are essentially throwaway, their
sole purpose is to extend the predefined type hierarchy.

Also note that the type hierarchy only contains _namespaces_, not actual components. So the
overhead of maintaining this special file is quite minimal, you only need to edit it when you
introduce a new namespace. and not for every component.

Also not the structure of a fully qualified component name:

```
Type.Member
Module.Type.Member
Module.Module.Type.Member
Module.Module.Module.Type.Member
...
```

The `Member`, i.e. the component function itself, is _always_ on a `Type`. The `Type` itself
is either the top level one (`Ui` for all apps, and `Whatever` for any `LibWhatever` — also
this value needs to match the value of the `alias` field in `eggshell.json` for interop to work
correctly), or exists on some `Module` (which itself can be part of some other `Module`). All
the `Module`s and `Type`s are defined in the `ComponentsHierarchy.fs` file, while all the
`Member`s are defined in their appropriate files (possibly multiple to a file where makes sense).

## Public-facing types

Sometimes components need to export public-facing type. Perhaps we have something like this:

```fsharp
[<AutoOpen>]
module AppExample.Components.Some_Namespace_Something

type Size =
| Large
| Small

type Ui.Some.Namespace with
    [<Component>]
    static member Something (size: Size) : ReactElement =
        ...
```

we certainly don't want to have this at the call site:

```fhsarp
Ui.Some.Namespace.Something (size = AppExample.Components.Some_Namespace_Something.Size.Large)
```

for it is ugly, and we don't like ugly. Instead, we'd like to keep the type and the component accessible
through the same type hierarchy. The way to achieve this sort of call site, i.e.

```fhsarp
Ui.Some.Namespace.Something (size = Ui.Some.Namespace.Something.Size.Large)
```

is as follows:

```fsharp
[<AutoOpen>]
module AppExample.Components.Some_Namespace_Something

module Ui =
    module Some =
        module Namespace =
            module Something =
                type Size =
                | Large
                | Small

open Ui.Some.Namespace.Something

type Ui.Some.Namespace with
    [<Component>]
    static member Something (size: Size) : ReactElement =
        ...
```

It is certainly ugly to have this staircase in the code, but that's a stupid F# limitation we can't do much about.
The saving grace is that we're unlikely have namespaces that go deeper than this level of nesting.

## Styles

We like to have styles at the end of the file, but we need to use them above. This requires us to define styles
as a muturally recursive type with whatever type holds our components. It looks like this:

```fsharp
module private Styles =
    let sample = makeTextStyles {
        fontSize 42
    }

type Ui with
    [<Component>]
    static member Something (size: Size) : ReactElement =
        LC.Text ("QQQ", styles = [|Styles.sample|])
```

The `makeTextStyles` computation expression (and its `makeViewStyles` counterpart) is made available via the `open ReactXP.Styles` statement.
All familiar components in the `RX` namespace (and some of their helper wrappers in `LC`) have this `styles`
parameter defined on them.

More on styles, including how to parametrize them, is [here](./fsharp/styling.md).

## The `elements` and `element` CEs

Depending on circumstances, components may take `child: ReactElement` or `children: array<ReactElement>`,
or builder functions like `content: SomeData -> array<ReactElement>`. With newer components (ones written
since the RenderDSL -> F# transition), the choice of child vs children is usually sensible, but
unfortunately some of the legacy components are built around taking a `child` even though they should
really be taking `children`. To help with converting between `child` and `children`, and to generally
make the job of building the component tree easier, we use `element` and `elements` computation expressions.

The process of building these `array<ReactElement>` as we build out our component trees is often
conditional. Sometimes you want to iterate some `seq` and produce a bunch of elements, or perhaps
map an `option`. Constructing raw F# arrays in this way is cumbersome, and adds a lot of incidental
eyesore noise to your code. So we have an `elements` computation expression that relies on implicit
yields to construct the resulting `array<ReactElement>`. Options, seqs, lists, arrays, nested lists/arrays,
lists/arrays of options, it's got pretty much any case we've encountered in practice covered.
Incidentally you can easily have local let-bound values interpsersed with implicitly yielded `ReactElement`s,
which makes for nice code writing. And whenever you need a single `ReactElement` to pass, just change `elements`
to `element`.

## They `key` prop, and the unused warning

The `key` prop is an important part of React, but unfortunately in the F#/Fable land we have to explicitly
declare it in order for it to be usable. Yet it's a system level prop, so we don't actually ever explicitly
use it, which results in F# compiler warnings like this:

```text
./src/Components/ShoppingCart/Shipment.fs(118,10): (118,13)
    warning FSHARP: The value 'key' is unused (code 1182)
```

Keeping the project free of meaningless warnings is critical for making meaningful warnings actually noticeable,
so the standard procedure for dealing with `key` is to have

```fsharp
ignore key
```

as the first line of the component's body. Fable does not generate any code corresponding to this `ignore` call,
so it is completely benign. Since this is standard procedure in the EggShell world, there is no need to
provide an end of line comment explaining why the `ignore` is necessary.


## So how do I add a component?

Just make a new file in the `Components/` directory, in whatever subdirectories are required for your
namespace (directly in `Components/` for anything at the top of the hierarchy), then add it in your
`.fsproj` file, set the module name as described above, declare the type you're extending, and write
the member function. If you really want, you can copy the sample below:

```fsharp
[<AutoOpen>]
module AppQQQ.Components.Something

open Fable.React
open LibClient
open LibClient.Components
open ReactXP.Components
open ReactXP.Styles

module private Styles =
    let sample = makeTextStyles {
        fontSize 42
    }

type Ui with
    [<Component>]
    static member Something (size: Size) : ReactElement =
        LC.Text ("QQQ", styles = [|Styles.Sample|])
```
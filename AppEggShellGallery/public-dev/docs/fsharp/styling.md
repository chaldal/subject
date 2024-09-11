# Styling

ReactXP's styling system is based on a least common demominator sort of system between CSS and
ReactNative's styles. We thinly wrap ReactXP's styling system to:
* make it type-safe
* allow for writing helper functions that return multiple rules
* provide some performance optimizations that complement ReactXP's internal caching

If you're coming from the web world, the earlier you abandon "but I can do this in CSS like this!"
line of thinking, the better.

Styles are typically applied in three ways:
* directly to ReactXP components (like RX.View, RX.Image, LC.Text)
* to custom components that decided to accept styles and pass it along to their top level RX component
* via themes

## Applying styles directly to ReactXP components

In the .fs file where your component lives, you will typically have a

```fsharp
module private Styles =
    ...
```

Within this, there are two patterns for declaring styles.

### Simple let-bound styles

```fsharp
let card = makeViewStyles {
    margin 16
}
```

This declares a style value `card`. It can be applied to a view like this:

```fsharp
RX.View (
    styles = [|Styles.card|],
    children = ...
)
```

Regardless of how many times you use it within your code, it is constructed exactly once.
This works for simple, static, non-parametrized styles.

### Parametrized styles

Sometimes we want to vary styles based on input parameters. E.g.

```fsharp
type Level =
| Info
| Warning

[<Component>]
member static Card (level: Level) =
    RX.View (
        styles = [|Styles.card level|],
        children = ...
    )
```

For this, a memoized function should be used:

```fsharp
module private Styles =
    let card = ViewStyles.Memoize (fun (level: Level) -> makeViewStyles {
        backgroundColor (
            match level with
            | Level.Info    -> Colors.White
            | Level.Warning -> Colors.Orange
        )
    })
```

This works for multiple parameters as well.

If you forget to memoize, and just write a let-bound function that produces styles, worry not — the moment
you use it for the second time with the same parameters, ReactXP will helpfully report a style leak on the JS
console, which is your cue to add memoization.

Note, that there's another way of accomplishing the same thing:

```fsharp
module private Styles =
    let cardInfo = makeViewStyles {
        backgroundColor Colors.White
    }

    let cardWarning = makeViewStyles {
        backgroundColor Colors.Orange
    }

[<Component>]
member static Card (level: Level) =
    RX.View (
        styles = [|
            match level with
            | Level.Info    -> Styles.cardInfo
            | Level.Warning -> Styles.cardWarning
        |],
        children = ...
    )
```

Performance here would be around the same, which means that we should prioritize readability.

As the `styles` parameter takes an array, you can pass multiple styles.

`makeViewStyles` makes styles to be used with RX.View, and `makeTextStyles` is used for LC.Text.

## Applying styles to custom components

In general, the visuals of the component is its own business, and "injecting" styles into it should
feel quite wrong. There are some legitimate use cases for this sort of behaviour, though. For example:
* setting the width of a text input element
* setting the margin around a button
* forcing a component to expand to full width

To accommodate these types of use cases, components can choose to take `?styles: array<ViewStyles>` as
a parameter, and apply it internally as they see fit, typically onto the top level RX.View within their
internal implementation. There are plenty of examples of this in the standard component library.

## Themeing components

The less privacy-invasive way of telling a component how to look is to use the "theme" that it provides
to style it. When a component declares a theme, they are explicitly saying "my visuals can be modified, here
are the modifications I accept". To learn more about themeing, read [here](./fsharp/themeing.md).

## What about dynamic style values

We talked about parametrized style values. Note that we are comfortable memoizing them because in the
entire application, the number of unique parametrizations that's likely to be used is likely limited
to a fairly small number, even if we take parameters like `width: int`.

This is because "parametrized" is not the same as "dynamic". If we had to implement some style function
that takes `x: int` and produces a `left x` sort of style rule, and used it to implement, say, a draggable
component, this use case would work poorly with memoization. We _could_ allow dynamic styles, but from experience
these dynamic use cases are far better served by ReactXP's animation values, which are built for performance,
which is usually a requirement for dynamic style scenarios anyway. 

## Older way of declaring styles

We used to have a preference for keeping styles at the bottom of the .fs file, for which we used
a recursive type, declared as

```
and private Styles() =
    ....
```

but this approach forced us to use an unorthodox syntax for declaring styles as `static member val`,
and the price for missing the `val` bit was runtime performance degradation. So it was decided that
having styles in a module is simpler and safer in tersm of performance.

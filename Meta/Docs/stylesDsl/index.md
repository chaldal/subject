# StyleDSL Background

RenderDSL was originally intended for normal React, styled through CSS. When ReactXP support was
added, it became apparent that styling in that universe is done very differently, in a crappy
ad hoc, imperative fashion (as necessitated by ReactNative), with no cascading behaviour and
no pattern for tweaking styles of other components. We needed a way to make this whole mess usable,
so as a result RenderDSL was extended to do some extra magic to make the use of styles smooth.

In all, the styling system consists of the following parts:

* ReactXP styles — the imperative, `style` prop based system that we need to integrate with.
* StylesDSL — the set of style building functions, operators, and types, that allow us to
  define style sheets and themes in a readable, digestable, concise manner
* RenderDSL styles runtime — the runtime library that takes the style sheets, examines the
  class and rt-class attributes, and plumbs the matching style rules and style sheets through
  the component tree all the way down to the base ReactXP components' `style` prop.

## Implicit props for `class` and `style`

Basically it works as follows. Styles are defined in a separate `styles.fs` file using the StylesDSL.
The contents of that "style sheet" is passed to the render function, and the `class='foo'` attribute
on `<SomeElement/>` is used to say "I want to give this element the style defined under the key `foo`
in the styles file".

What this does under the hood is pull the corresponding values from the style sheet, as well as any
other style sheets that were dynamically passed to the current component, filters them based on the classes
applied on the element via `class` and `rt-class`, and the resulting list of styles is passed
into the `style` prop of the component. The `style` prop itself is never declared explicitly
(partly because crappy F# records don't support inheritance), nor is the `class` prop. For ReactXP
base components, the value of the `style` prop is further transformed to make it match the ReactXP
specification.

# The public parts of the `.styles.fs` file

Every `.styles.fs` file is expected to have a publicly visible `styles` value of type `RuntimeStyles`
to be made available.

TODO also optionally a Theme

## Styling components internally

This is the base case for the styling system. The `.render` file lays the component out in terms of,
base ReactXP building blocks, and its `.styles.fs` file defines how they are styled. E.g.

```xml
<div class='view'>
    <text>{props.Heading}</text>
    <div class='line'/>
    <div class='body'>
        {props.children}
    </div>
</div>
```

is styled with

```fsharp
let styles = compile [
    "view" => [
        borderRadius 4
        border       1 (Color.Grey "cc")
        padding      8
    ]

    "line" => [
        border 1 (Color.Grey "99")
    ]

    "body" => [
        padding  12
        fontSize 8
    ]
]
```

The `div` elements above are actually `RX.View` elements, which is a standard alias we use across all our
libraries and apps. You can see component aliases in the `eggshell.json` file, in each library/app, look
at the `componentAliases` key.

In this example, everything is pretty straight-forward. We associated some simple style rules with each class,
and at runtime, the system pulls those style values out of the sheet, and passes them into the `style` prop of
the corresponding `RX.View` components. The only interesting bit happening here is the distinction between
simple rules like `padding 12`, which map one-to-one to a ReactXP style rule, and the `border 1 (Color.Grey "99")`
rule, which maps to two ReactXP rules, `borderWidth 1` and `borderColor "#999999"`. The underlying style building
blocks system is what allows for this syntactic convenience. See `RulesAdditional.fs` for other functions that
generate multiple rules.

### Multiple rule blocks for the same class

This is allowed (and in some context is necessary) and will work:

```fsharp
let styles = compile [
    "view" => [
        borderRadius 4
    ]

    "view" => [
        padding 8
    ]
]
```

### Multiple classes

In the following scenario

```xml
<div rt-map='name := prop.Names' class='name' rt-class='selected := name = prop.SelectedName'/>
```

You can ensure that desired styles are only applied to an element with both `name` and `selected` like this:

```fsharp
let styles = compile [
    "name" => [
        border 1 (Color.Grey "99")
    ] && [
        "selected" => [
            borderColor (Color.Hex "ff0000")
        ]
    ]
]
```

alternatively, this also works (and can be desirable in certain situations):

```fsharp
let styles = compile [
    "name" => [
        border 1 (Color.Grey "99")
    ]

    "name && selected" => [
        borderColor (Color.Hex "ff0000")
    ]
]
```

### Dynamic classes

Elements can be given dynamic class names, in one of the following ways:

```xml
<div
 class='regular dynamic-{someExpression}'
 rt-class='regular := someBooleanExpression; `dynamic-{someExpression}` := someBooleanExpression'/>
```

In the style file, you can then consruct the key using whatever string-returning expression you want, e.g.
```fsharp
let styles = compile [
    // regular string concatenation
    ("dynamic-" + (someValue.ToString())) => [
        color Color.DevRed
    ]

    // formatted string
    (sprintf "dynamic-%O" someValue) => [
        color Color.DevRed
    ]

    // string-returning standalone function
    (makeClassName someValue) => [
        color Color.DevRed
    ]

    // member function on a type (perhaps a type extension on a union type)
    (someValue.ClassName) => [
        color Color.DevRed
    ]
]
```

When making class names out of union type cases, the `unionCaseName` function is helpful.

## Styling components externally

The components' internals should be viewed as being black-boxed, private parts. No more setting arbitrary
style rules using crafty CSS selectors to target a certain DOM element within the component. To be fair,
with this styling system, such hacking is pretty limited anyway, but to an extent it is possible, and
should be avoided.

There are three legitimate ways of styling a component exeteranally, i.e. from the outside:

* theming all instances of a component
* theming a single instance of a component
* setting margin rules (and possibly the AlignSelf rule) on the top-level block of a component


### Theming all instances of a component

Just like the component provides an explicit public contract for its use — the `Props` type, and
sometimes the `ComponentRef` type if it expects being interfaced with through React refs, it can
also provide an explicit way for users to change its styles. This is called theming. A typical example
would be a `Button` component providing a way to set its colour via a `setColor` function.

Typically a given application will want to style all instances of certain UI components, like buttons,
checkboxes, headings, etc, to match the colour scheme and other design preferences for the given app.
The `eggshell create-app` scaffolding provides a `ComponentsTheme.fs` file, which allows for theming blocks
like this:

```
    TagStyles.Theme.All(
        theBackgroundColor         = Color.Grey "ee",
        theTextColor               = Color.Grey "99",
        theSelectedBackgroundColor = Color.Grey "cc",
        theSelectedTextColor       = Color.Grey "99",
        fontSize                   = 13
    )
```

This call styles all `Tag` components to have these particular colour and font size settings.

For this to work, each components that wants to support such theming needs to provide a unit-returning
`All` function that creates the style rules, and registers them with the system. It looks like this:

```fsharp
type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Tag", baseStyles)

    // This is the function called from ComponentsTheme.fs of individual apps
    static member All (theBackgroundColor: Color, theTextColor: Color, theSelectedBackgroundColor: Color, theSelectedTextColor: Color, fontSize: int) : unit =
        customize [
            Theme.Rules(theBackgroundColor, theTextColor, theSelectedBackgroundColor, theSelectedTextColor)
            Theme.FontSizeRules fontSize
        ]

    // This is the function called from individual `.styles.fs` files where a single instance of this component needs to be themed
    static member One (theBackgroundColor: Color, theTextColor: Color, theSelectedBackgroundColor: Color, theSelectedTextColor: Color) : Styles =
        Theme.Rules(theBackgroundColor, theTextColor, theSelectedBackgroundColor, theSelectedTextColor) |> makeSheet

    // This is an example of a function that styles only one aspect of the component's visuals,
    // as opposed to fully theming the instance.
    static member FontSize (size: int) : Styles =
        Theme.FontSizeRules size |> makeSheet

    // This is an internal building block. It's public because it needs to be accessed in the same file as
    // it's declared in, but outside of the Theme type
    static member FontSizeRules (size: int) : List<ISheetBuildingBlock> = [
        "view" => [
            fontSize size
        ]
    ]

    // This is also an internal building block.
    static member Rules (theBackgroundColor: Color, theTextColor: Color, theSelectedBackgroundColor: Color, theSelectedTextColor: Color) : List<ISheetBuildingBlock> = [
        "view" => [
            backgroundColor theBackgroundColor
            color           theTextColor
        ] && [
            "is-selected" => [
                backgroundColor theSelectedBackgroundColor
                color           theSelectedTextColor
            ]
        ]
    ]
```

Components that provide a theme usually apply a default theme to themselves using the same building blocks.
For example the Tag component applies the default theme like this:

```fsharp
let private baseStyles = asBlocks [
    "view" => [
        ... various base rules ...
    ]
]

type (* class to enable named parameters *) Theme() =
    ... same as above ...

let styles = compile (List.concat [
    baseStyles
    Theme.Rules (theBackgroundColor = Color.Grey "c6", theTextColor = Color.White, theSelectedBackgroundColor = Color.Grey "a6", theSelectedTextColor = Color.White)
    Theme.FontSizeRules 13
])
```

### Theming one instance of a component

We've seen how to apply a theme to all instances of a given component. Now let's see how to use the `One` and `FontSize` functions
from the example above to style a single instance of a Tag in your application. Say you have this in your `.render` file:

```xml
<LC.Tag Text='user.Name' rt-class='important := user.IsImportant' rt-map='user := users'/>
```

Then you can do this in your `.styles.fs` file:

```fsharp
let styles = compile [
    "important" ==> LibClient.Components.TagStyles.Theme.One(theBackgroundColor = Color.Red, theTextColor = Color.White, theSelectedBackgroundColor = Color.Orange, theSelectedTextColor = Color.White)
    "important" ==> LibClient.Components.TagStyles.Theme.FontSize 20
]
```

### Providing explicit styles for margin from the outside

Margin is a special style rule, in that, unlike most other style rules, it dictates the value for a property that
is external to the element, not internal to it. And external space should be goverened by the owner of the space,
not the components that are placed within that space. How can a Button know how many pixels of margin should around it?
It depends on the context — what else is around it? And the Button clearly doesn't have a way of knowing this.

So margin should be set by the parent component. That is, if we have

```xml
<div>
    <SomeComponent/>
    <LC.Button class='button'/>
</div>
```

we should be able to say in the `.styles.fs` file

```fsharp
let styles = compile [
    "button" => {
        marginTop 40
    }
]
```

Our styling system allows for this, by employing a special `TopLevelBlockClass` constant. If the implementation of
a component, say our `Button`, adds this class like this:

```xml
<div class='view {TopLevelBlockClass}'>
    ... button internals ...
</div>
```

then users of this component can pass it style rules (any style rules can be passed, but the caller has no business
specifying anything other than margin) as shown in the above example. The author of a component may choose explicitly
to not allow externally passed styles, or may simply forget to add the `TopLevelBlockClass` — in that case, externally
passed styles will not be applied. In principle the author can add the `TopLevelBlockClass` on any element inside the
`.render` file, but that would throw off expectations and would be a mean thing to do. Don't be mean.

## Styling DOM elements with plain old CSS

For the majority of our UI work, we'll be using ReactXP, as it targets both web and mobile from a single code base.
ReactXP's layout is flexbox based, and there are things that are difficult to do with flexbox. Table-like layouts are
one key example of this. So instead of jumping through flaming hoops in order to make table layouts with flexbox,
we may choose to fall back on standard DOM elements, instead of ReactXP components, for building UIs that we know will
only be used in a web browser.

Such UIs need to be styled, and our default styling system will not work for them. Instead, we need a way to style them
through regular CSS. So we augmented our styling system to allow for adding CSS, by doing this in the `.styles.fs` file:

```fsharp
addCss ("""

table.la-table {
    border-spacing: 0px;
    min-width:      100%;
}

"""
)
```

We can inject colour values using `sprintf` like this:

```fsharp
addCss (sprintf """

table.lp-table td {
    min-width: 100%%;
    color:     %s;
}

"""
    Neutral.medium.ToCssString
)
```

Note that these css rules get added globally to the HTML document. So it is up to the caller to ensure
the uniqueness of the class names. See style guide for conventions.

## Reuse of Style Rules

Supported, but in a pretty crappy manner.

TODO


## Animations

Supported, though syntax not sugary.

TODO


## Dynamically providing style values

Style rules can be provided directly from the `.render` file, though the syntax is a bit rough.

First, add `rt-open='ReactXP.Styles'` to get access to the style functions.

Now you can simply add an `rt-style` attribute to the desired component, e.g.

```
rt-style='[minHeight 200]'
```

See `LC.Route`'s `.render` file for a working example.


## Nth child pseudo classes

We partially support nth child flavour pseudo classes that may be familiar from CSS land.

There are two distinct cases of styling with nth child pseudo classes. First, consider this example:

```xml
<div>
    <div class='card'>...</div>
    <div class='card'>...</div>
    <div class='card'>...</div>
    <div class='card'>...</div>
</div>
```

We want to have 20 pixels of margin between each card. Simply saying

```fsharp
"card" => [
    marginBottom 20
]
```

would generate unwanted margin after the last item. Saying `marginVertical 10` will similarly generate
unwanted margin before the first and after the last card. In order to have no unwanted margin, we can
instead say

```fsharp
"card" => [
    marginBottom 20
] && [
    ":last-child" => [
        marginBottom 0
    ]
]
```

Along with `:last-child`, we also generate `:fist-child`, `:odd-child`, and `even-child` pseudo classes.

Incidentally, as the above example illustrates, we are more often concerned with the case of "when is
it _not_ the last child" rather than the positive case. For that reason, we've also added the
`:not-last-child` pseudo class. So the above style snippet can be reworded more concisely:

```fsharp
"card && :not-last-child" => [
    marginBottom 20
]
```


Next, consider the following example:

```xml
<ToggleButtons>
    <ToggleButton Label='"Banana"'/>
    <ToggleButton Label='"Peach"'/>
    <ToggleButton Label='"Mango"'/>
<ToggleButtons>
```

Since we want toggle buttons to appear as a group, we want the external external corners to be rounded,
while the internal corners remain sharp. But clearly it would be ridiculous for the _use site_ to provide
such style overrides based on the autogenerated `:first-child` and `:last-child` pseudo classes. How
to style itself when it happens to be a first child or a last child is an internal concer of the
`ToggleButton`, not a use-site concern. The `.render` file of `ToggleButton` may look like this:

```xml
<div>
    <uitext>{props.Label}</uitext>
</div>
```

We clearly need some way of taking in external information — which child am I? — and make decisions based
on that. This information is passed behind the scenes, and needs to be extracted like this:

```xml
<div class='view {externalPseudoClasses props}'>
    <uitext>{props.Label}</uitext>
</div>
```

This will pull in the external pseudo classes, prefixed with `external`. We can then do our styling:

```fsharp
"view" => [
    border  1 (Color.Grey "cc")
    padding 10
] && [
    ":external-first-child" => [
        borderTopLeftRadius    5
        borderBottomLeftRadius 5
    ]
    ":external-last-child" => [
        borderTopRightRadius    5
        borderBottomRightRadius 5
    ]
]
```

These classes are generated at runtime, so work cleanly with `rt-map`, `rt-if`, `rt-block` etc.
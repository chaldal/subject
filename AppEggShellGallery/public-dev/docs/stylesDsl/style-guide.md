# StylesDSL Style Guide

## Class name conventions

String literals used in `class` and `rt-class` attributes, and corresponding literals in the `*.styles.fs` files,
should use the `lower-case-hyphen-connected-slug` syntax. Exceptions can be made for auto-generated or parametrized
class names, particularly ones where a union case name is interpolated, e.g. `button-{State.Disabled}` which evaluates
to `button-Disabled`.

NOTE: in the future, we should probably find a way to make these type-safe.

## Ordering of rules

Given that style rules are a mix of value-less rules and name-value rules,
applying our regular alignment guidelines we get the following:

```fsharp
// bad
FlexDirection.Row
AlignItems.Center
marginBottom      10

// good
FlexDirection.Row
AlignItems.Center
marginBottom 10
width        20
```

Most of the times it will be sensible to keep flex-related rules, which are mostly value-less,
grouped at the top, followed by name-value ones below that.

But if in a sensible ordering a value-less rule comes in between two name-value ones,
standard alignment rules apply, i.e. the value-less line "resets" the alignment flow.
Contrived example (i.e. I'm not saying this is necessarily a "sensible" ordering, just
illustrating the "reset alignment" idea):

```
// bad
flex     1
FlexDirection.Row
AlignItems.Center
fontSize 12

// good
flex 1
FlexDirection.Row
AlignItems.Center
fontSize 12
```


## Raw CSS class naming conventions

Our styling system allows for styling regular DOM nodes using regular CSS, by using the `addCss` function
in the `.styles.fs` file. The class names that you use there are global to the document, so we need to make
sure styles added by one component don't conflict with those added by another one.

The convention is `moduleAbbreviation-ComponentName-local-class-name`, where

`moduleAbbreviation` is the lowercased abbreviation of the EggShell lib or app that the component belongs to.
E.g. for `LibClient` it would be `lc`, for `AppProtocol` it would be `ap` and so on. If there are conflicts,
add additional letters to disambiguate.

`ComponentName` is the PascalCased name of the component in whose `.styles.fs` file we're adding the CSS.

`local-class-name` is the lowercase-hyphenated-slug that's left up to the user. Be sensible.


There are also use cases for non-component-specific CSS rules, like `lp-table`. Not quite sure where we'll go
with this stuff yet, so not going to establish a convention just yet.
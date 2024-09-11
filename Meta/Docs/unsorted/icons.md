# Icons Infrastructure

## Basics

We've achieved a pretty nice setup for dealing with icons.

Our icons are SVG based.

In practice, if you want to drop in an icon inside your component, you use the `Icon` component from `LibClient`, like this:

```xml
<div>
    <LC.Icon Icon='Icon.ChevronLeft'/>
</div>
```

This gets you an icon of default size and colour. The set your own values, you use the styling system, like this:

```xml
<LC.Icon class='my-icon' Icon='Icon.ChevronLeft'/>
```

and

```fsharp
let styles = compile [
    "my-icon" => {
        color    Color.DevBlue
        fontSize 42
    }
]
```

## Passing icons around

As with any F# values, you can pass icons around through props. For reasons described in the "Under the hood" section,
the type of the value you pass around is `IconConstructor` rather than just `Icon`. So, if you are building some special
button that needs to display a user-specified icon, you declare the props to be

```fsharp
type Props = (* GenerateMakeFunction *) {
    Icon: LibClient.Icons.IconConstructor
}
```

and then from the call site, you can say

```xml
<SpecialButton Icon='Icon.ChevornLeft'/>
```

As long as at the end of the chain you consume the value with an `LC.Icon` component, you don't need to know
anything else about the specific types.

## Icon collections

The design goal for icon collections was "to be able to, in a single namespace, have both the generic icons applicable
to all apps, and the application specific icons, defined within the app". This was achieved quite nicely by having the
generic collection declared in `LibClient` as

```fsharp
module LibClient.Icons

and Icon =
    // ... internal details ...

    static member ChevronLeft = Icon.MakeSvgPathIcon (512, 512) [
        // ... icon body ...
```

so a type with a bunch of static members on it, one for each generic icon, and in each app,

```fsharp
module AppSample.Icons

type Icon =
    inherit LibClient.Icons.Icon

    static member SomeAppSpecificIcon = Icon.MakeSvgPathIcon (512, 512) [
        // ... icon body ...
```

more static methods for app specific icons added in. The `AppSample.Icons` module is then added to `eggshell.json`'s
`additionalModulesToOpen` list, which makes both generic `Icon` static functions and app-specific `Icon` static functions
all available as `Icon.Whatever` in the `.render` file.

You can see the collection of generic icons defined in `LibClient` over [here](gallery:///%22Desktop%22/Components/%22Icons%22).

## Creating new icons

First, decide if your icon is app-specific or generic, which will tell you where to put it.

Then get a cleaned up SVG from the designer, or make one yourself if you have the skills and software.
Cleaned up in this case means:
* 512 x 512 in size
* all groups ungrouped
* all strokes expanded
* all text converted to oulines
* all overlapping paths unioned
* everything is of one colour
* so all you should have is a flat list of paths and compound paths

In my experience, Adobe Illustrator's "Save" function (as opposed to export) where the file type is selected to "svg"
works quite well.

Save the file into either `LibClient`'s or `AppWhatever`'s `src/IconSources/` directory, naming it `WhateverPascalCased.svg`,
where "WhateverPascalCased" describes what's depicted in the icon.

Next, in a terminal, run

```
svgo --pretty --config='{"full": true}' --enable=convertShapeToPath -o - LibClient/src/IconSources/WhateverPascalCased.svg
```

it will produce something like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--Generator: Adobe Illustrator 24.3.0, SVG Export Plug-In . SVG Version: 6.00 Build 0)-->
<svg version="1.1" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 512 512" style="enable-background:new 0 0 512 512" xml:space="preserve">
    <style type="text/css">
        .st0{fill:#4FB65C;}
    </style>
    <path id="Path_1917" class="st0" d="M197.6,388.82c5.96-0.13,10.77-4.94,10.9-10.9V225.58c0.32-6.01-4.29-11.15-10.3-11.47
        c-6.01-0.32-11.15,4.29-11.47,10.3c-0.02,0.39-0.02,0.78,0,1.17v152.44c-0.07,5.9,4.65,10.74,10.55,10.81
        C197.39,388.82,197.49,388.82,197.6,388.82z"/>
    <path id="Path_1918" class="st0" d="M259.26,388.82c5.96-0.13,10.77-4.94,10.9-10.9V225.58c0.32-6.01-4.29-11.15-10.3-11.47
        c-6.01-0.32-11.15,4.29-11.47,10.3c-0.02,0.39-0.02,0.78,0,1.17v152.44c-0.07,5.9,4.65,10.74,10.55,10.81
        C259.05,388.82,259.15,388.82,259.26,388.82z"/>
    <path id="Path_1919" class="st0" d="M320.95,388.82c5.96-0.13,10.77-4.94,10.9-10.9V225.58c0.32-6.01-4.29-11.15-10.3-11.47
        c-6.01-0.32-11.15,4.29-11.47,10.3c-0.02,0.39-0.02,0.78,0,1.17v152.44c-0.07,5.9,4.65,10.74,10.55,10.81
        C320.74,388.82,320.85,388.82,320.95,388.82z"/>
</svg>
```

Now go into your `Icons.fs` file, add a new function in a good place, like this:

```fsharp
    static member WhateverPascalCased = Icon.MakeSvgPathIcon (512, 512) [
        ""
    ]
```

And then for every `<path>` element you see in the SVG output, copy the `d` attribute value into the quotes your list,
one per each `<path>` element. So for the above data, you'll end up with this:

```fsharp
    static member WhateverPascalCased = Icon.MakeSvgPathIcon (512, 512) [
        "M197.6,388.82c5.96-0.13,10.77-4.94,10.9-10.9V225.58c0.32-6.01-4.29-11.15-10.3-11.47c-6.01-0.32-11.15,4.29-11.47,10.3c-0.02,0.39-0.02,0.78,0,1.17v152.44c-0.07,5.9,4.65,10.74,10.55,10.81C197.39,388.82,197.49,388.82,197.6,388.82z"
        "M259.26,388.82c5.96-0.13,10.77-4.94,10.9-10.9V225.58c0.32-6.01-4.29-11.15-10.3-11.47c-6.01-0.32-11.15,4.29-11.47,10.3c-0.02,0.39-0.02,0.78,0,1.17v152.44c-0.07,5.9,4.65,10.74,10.55,10.81C259.05,388.82,259.15,388.82,259.26,388.82z"
        "M320.95,388.82c5.96-0.13,10.77-4.94,10.9-10.9V225.58c0.32-6.01-4.29-11.15-10.3-11.47c-6.01-0.32-11.15,4.29-11.47,10.3c-0.02,0.39-0.02,0.78,0,1.17v152.44c-0.07,5.9,4.65,10.74,10.55,10.81C320.74,388.82,320.85,388.82,320.95,388.82z"
    ]
```

Your icon is now ready for use. We should be careful to use semantic names at use sites, so you may want to create an alias
for your icon like this:

```fsharp
    static member WhatItRepresents : IconConstructor =
        Icon.WhateverPascalCased
```

E.g. you may have `BackArrow` aliasing `ChevronLeft`, or `Department` aliasing `ThreeHumansInHexagon`. This level of indirection
makes swapping icons much easier. E.g. if you need to change `BackArrow` from `ChevronLeft` to `ArrowLeft`, without the alias
good luck crawling your entire code base and figuring out which instances of `ChevronLeft` are used in back buttons, and which
ones are used for other purposes.

## Under the hood

ReactXP has limited support for SVG, so limited that the only element supported is `<path>`, which is why we use `svgo` to
convert regular `.svg` files into normalized ones first. Color and size have to be specified imperatively, which doesn't sit
well with our desire for declarative styling, so we had to implement some hacks.

The type for passing around icons, `IconConstructor`, is actually a function `Color -> int -> Icon`, and `Icon` itself is
just a react element that represents the ReactXP specific SVG elements. The `Icon` component in `LibClient` takes this
function as a prop, extracts the `color` style prop and the `fontSize` style prop from whatever styles are passed into it,
and passes them as the first and second parameters to the `IconConstructor`, resulting in concrete react elements, that
can then be just rendered.

So, if you wanted to (and that's indeed how we used to do it before `LC.Icon` was introduced), you could render an icon by
manually providing the color and the size in the `.render` file, like this:

```xml
<div>
    {=Icon.ChevronLeft Color.DevBlue 42}
</div>
```

This works, but is now considered an undesired low level way of using icons.
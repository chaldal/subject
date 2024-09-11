# Design Introduction

One of the goals of EggShell is to standardize and unify the design language
between designers and engineers. Ideally, designers will produce mocks that
explicitly specify the configuration parameters of the color scheme and components
theme, and then just gallery components with no customizations (new components
that we don't yet have being an obvious exception).

## Semantic naming

We like to use semantic naming for describing our visuals. To illustrate the idea,
let's start with a simple use case of wanting to show an orange button on the screen.
We may have some HTML that looks like this

```xml
<button class='cta'>Ok</button>
```

and some styles that look like this:

```css
.cta {
    background-color: #ff7800;
}
```

This gets the job done, but is problemmatic for a few reasons. The most obvious one is
that when we later need to reskin the product, and change this shade of orange to some
other one, we need to replace many instances of the literal colour value, which is a
laborious and error-prone process.

To remedy this, a frequently used approach is to name the colour value. So we may have
this kind of style instead:

```css
/* in some shared file */
$orange = $ff7800;
```

```css
/* in your component's style file */
.cta {
    background-color: $orange;
}
```

One problem that this kind of naming leads to is that in the new, reskinned version,
the button may need to be green, and not orange. What to do? Rename the variable to
`$green`, and set its value accordingly? But we also happened to have this snippet:

```xml
<div class='message'>You are about to delete EVERYTHING, are you sure?</div>
```

```css
.message {
    color: $orange;
}
```

Is it okay to now display this message in green?

Probably not. The button and the message were probably orange for different reasons.

And it is this "reason" that we want to base our naming on, as opposed to names derived
from the concrete colour values. In our small example, a more semantic style sheet
could look something like this:

```css
.cta {
    background-color: $primary;
}

.message {
    color: $caution;
}
```

The same kind of reasoning applies to things like sizes. Whether a button is "big" or
"medium" or "small" is a description of its size, but it says nothing about the reasons
behind why it is of that size. The reasons are likely related to the amount of visual
prominence the designer wants to give to any given button — it is either the "primary"
action on the screen, or a "secondary" one, or a "tertiary" one. Note that the actual
visual implementation of this prominence level can be much richer than simply changing
the size — you can make the colours less saturated, you switch to using a text-only
button instead of one with a coloured background, etc.

## Color Schemes

Each application has a color scheme defined, which consists of five sets of colour variants:

* Neutral
* Primary
* Secondary
* Attention
* Caution

Each set of colour variants defines a scale of brightness: 050, 100, 200, 300, 400, 500, 600,
700, 800, 900, and designates one of these (usually somewhere around 500) as the Main color.

We have imported Material Design colour variants so as to easily theme apps, but designers
are welcome to provide custom variant sets, and full schemes, as long as they satsify the
contract described here.

There is capacity for defining custom, out-of-scheme colours for one-off use, but this should
be used sparingly.

## Levels

Buttons currently support Primary, Secondary, and Tertiary levels, though due to attempts to
accommodate some designs, PrimaryB and SecondaryB were also added. Also Cautionary is defined
as a "level", even though visual prominence has nothing to do with whether an action associated
with the button requires caution or not. This stuff needs to be revisited.
# Taps, Clicks, Hovers, etc

Handling pointer events is another one of those aspects of building UI where
having good defaults may take the user experience from passable to great.

Take the simple use case of offering user a link to go to the settings screen.
A naive way to implement it would be

```xml
<span onClick='fun _ -> naiveNavigation.GoToRoute Settings'>
    Settings
</span>
```

There are two problems with this.

First, the route will open in the same tab.
An experienced user of the web may control-click (or command-click on a Mac) the
link, and be frustrated that the link _still_ opens in the same tab, seriously
constraining their freedom to navigate the app on their own terms.

One solution to this problem is to do something like this:

```xml
<span onClick='fun e -> naiveNavigation.GoToRouteMaybeInNewTab Settings e'>
    Settings
</span>
```

or, in its curried form,

```xml
<span onClick='naiveNavigation.GoToRouteMaybeInNewTab Settings'>
    Settings
</span>
```

Here, the `GoToRouteMaybeInNewTab` takes the click event, examines the state of
control/command buttons, and based on that opens the link in either the same or
a new tab. But a better way, which is what our nav system implements, is to make
the default `Go` function always handle the possibilty of a control/command click,
by default.

```xml
<span onClick='nav.Go Settings'>
    Settings
</span>
```

This way, the developer never needs to explicitly think about allowing new tab navigation,
and thus never runs the risk of forgetting to implement it.

Second, the `onClick` handler is attached directly on the `<span>`. This is fine
for high precision pointers like the mouse, but becomes a major source of frustration
for handheld devices with imprecise fingers tapping while simultaneously obsuring the
visiblity of whatever is being tapped.

A naive way to remedy this is do something like

```xml
<span class='link' onClick='nav.Go Settings'>
    Settings
</span>
```

```fsharp
"link" => [
    padding 10
]
```

This works, to an extent. You'll likely need hacks to realign the link's visuals, since now
the link is out of baseline alignment with other elements on the page. What's worse, though,
is if the link appears inside other text, it'll push the line height by 20px, looking really
awkward. To remedy this problem, we might try something like this:

```xml
<span class='link'>
    Settings
    <div class='tap-capture' onClick='nav.Go Settings'/>
</span>
```

```fsharp
"link" => [
    Position.Relative
    Overflow.Visible
]

"tap-capture" => [
    Position.Absolute
    trbl -10 -10 -10 -10
]
```

As you can imagine, even if a diligent developer remembers to code this for every link they
implement, it's laborious, error-prone, and just generally a pain in the neck. And we haven't
even touched on handling hover/depressed states.

The moral of the story is that should never be handling pointer events in their low level form,
i.e. we should never attach `onClick` handlers to ReactXP components directly. Instead, we should
use the `LC.TapCapture` component.

## LC.TapCapture

The `LC.TapCapture` component is a transparent, absolutely positioned, expanded to the full size
of the parent element, that takes `OnPress: PointerEvent -> unit` and `PointerState` props. The
`PointerEvent` type, instead of the raw `Browser.Types.Event`, allows the TapCapture to always
provide an instance of itself in the `Anchor` field, which allows things like context menues to
know which element to show themselves next to.

A benefit of using a single component for all pointer event related needs is that improvements
to this component propagate to the entire suite of applications built on EggShell. For example,
a QA person may want to audit the usability of all tappable elements in the app. They could manually
tap every one at varying distances from the visuals, to see how well they respond to fat-fingering,
or they can simply turn on [TapCapture debug visualization](gallery://act/toggleTapCaptureDebugVisualization)
from the app's debug screen. This allows you to see the extent of each tap capture area, and also
allows you to see which tappable components are implemented in an ad hoc way and are thus not repeating
the benefits of using `LC.TapCapture`.

## Overflow problem

One problem we have with TapCapture is that by default, ReactXP components are set to `Overflow.Hidden`.
This is a default that leads to a tonne of annoying issues when you first learn to use the system,
coming into it from DOM/CSS, which has the opposite default. A number of times I considered flipping the
default, but every time ended up being too scared
(see [Chesterton's Fence](https://en.wiktionary.org/wiki/Chesterton%27s_fence)).

So if you put a TapCapture inside an element in one of your components, you need to make sure to set
`Overflow.Visible`, not only on that element, but all ancestor elements as well. The ReactXP rule is
obviously unaware of component boundaries, but as component developers we have to be careful to not
mess with styling rules for components we don't own. Either way, so far, while it's a pain to ensure
that larger-than-parent-element TapCaptures are always as visible as we desire, it's been fairly manageable.

## Hover and Depressed States

To give the user a sense that the app is interactive and snappy, one cheap trick often imployed is
the implementation of hoverd and depressed states. An element is in a hovered state when a pointer is
above the element. It's in a depressed state between the time a pointer pressed (tapped or clicked) it,
and the time this press is released.

Lest the end user is forced to keep this data in their component's state in an ad hoc fashion, we have
a special helper component to keep track of the pointer state, `LC.Pointer.State`. And `LC.TapCapture`
conveniently takes the value it provides as a prop, and through this updates the state as necessary.
The typical setup looks like this:

```xml
<LC.Pointer.State rt-prop-children='Content(pointerState: ~PointerState)'>
    <CustomComponent
     ...
     rt-class='
        is-hovered   := pointerState.IsHovered;
        is-depressed := pointerState.IsDepressed;
    '/>

    <LC.TapCapture
     ...
     PointerState='pointerState'/>
</LC.Pointer.State>
```

The user then implements whatever visuals they want for the `is-hovered` and `is-depressed` classes.
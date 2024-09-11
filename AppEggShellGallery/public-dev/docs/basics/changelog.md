# Change Log

## 2023-12-31
- Updated minimum NodeJS version to `18.19.0`
- Updated react-native to `73.0.1`
- Using latest ReactXP 2.2.0

## 2023-02-14

- In the last year, EggShell underwent a transition from RenderDSL to F#, and a round of
perf tuning.

## 2021-08-31

* RenderDSL now supports a new attribute, `rt-with`, see [language reference](./renderDsl/index.md) for details.

## 2021-08-11

* New error boundary component: [`LC.ErrorBoundary`](gallery:///%22Desktop%22/Components/%22ErrorBoundary%22)

* `LC.AsyncData`'s default behaviour in the case when `Data` is a `Failed _` is the _throw_ the error,
  and thus unwind the component tree, upwards to the nearest `LC.ErrorBoundary`, or all the way up to
  the root, unmounting the whole application. You can still provide custom visuals for the fialed case
  using the `WhenFailed` prop.

* `LC.AppShell.Content` now takes an `OnError` prop that gives each app an opportunity to provide top level
  handling for all unexpected errors. A helper component, `LC.AppShell.TopLevelErrorMessage` is provided
  to render a default generic error message and a reload button. This component was added to all existing
  apps, in their `App.render` files.

* We were originally considering doing some kind of Alert based handling as an option for AsyncData failures,
  but its unclear whether this is actually a good idea, because so far failures in AsyncData seem to indicate
  critical failures of backend APIs which render the app unusable anyway, so propagating such errors all the
  way to the app top level handler is the sensible thing to do. If we later discover a use case where showing
  alerts makes sense, we may implement this optional functionality.

## 2021-06-22

* Instead of the very incomplete `PointerEvent` wrapper on top of the raw `Browser.Types.Event` hierarchy,
  we now have a full-featured `ReactEvent` type. In most cases, where we previously took a `PointerEvent`
  we should now be taking a `ReactEvent.Action`, which is a type with two cases, `Pointer` and `Keyboard`,
  for all UI situations where an action is called as a result of either a pointer tap/click or a keyboard
  event (enter or space often trigger buttons). This was done both because there was an immediate need for
  it (Dialogs are closable by either pressing the close button, or hitting escape (opt in), so we needed
  a way to represent these kinds of events), and in preparation for the eventuality where we fully support
  keyboard based navigation, where you can tab across components, etc.

* no more numeric keycode comparisons — we now have an active pattern that allows you to have matches like
  `match e.keyCode with | KeyCode.Enter -> doSomething() | _ -> Noop`. Defined in `Input.fs`.

## 2021-03-31

* We now support `:first-child`, `:last-child`, `:odd-child`, `:even-child`, `:not-last-child` pseudo classes

## 2021-03-10

* We now support dynamic styles, e.g. you can set the width of a div to some computed value from
  inside the .render file. Documentation coming at a later time. Example usage in `LC.Route`'s .render file.

## 2021-02-18

* Changed `SelectedItem` prop of [`LC.Input.Picker`](gallery:///%22Desktop%22/Components/%22Input_Picker%22) to `Value`

* Changed `MaybeSelected` prop of [`LC.Input.Date`](gallery:///%22Desktop%22/Components/%22Input_Date%22) to `Value`

* Changed `MaybeValue` prop of [`LC.Input.Quantity`](gallery:///%22Desktop%22/Components/%22Input_Quantity%22) to `Value`

* [RenderDSL language reference](./renderDsl/index.md) rewritten, including a handy summary
  of all language constructs

* [Roadmap](./basics/roadmap.md) added, scattered TODO lists from various other docs moved here

* marked responsive components in gallery with a `Responsive` tag

* Moved react refs documentation to a [stand-alone how-to](./how-to/refs.md)

* Made the `reactref` snippet for working with react refs

* Documented the [directory structure](./unsorted/directory-structure.md)

* Updated version of `showdown-highlight` to get rid of end-of-life warning

## 2021-02-10

* [Snippets](./tools/snippets.md) docs added

* [Responsive](./how-to/responsive.md) docs updated

* `LC.With.ScreenSize` added as a replacement for using the raw `LC.With.Context`

* [Taps, Clicks, Hovers, etc](./how-to/tap-capture.md) docs added

* Nav.Top now allows you to return `{=noElement}` from inside `<rt-prop name='Handheld'>` (or Desktop)
  to render no top nav. (you'll need to do a `./initialize` in `LibClient` for this to work)

## 2021-02-05

* Gallery [landing page](gallery:///)

* [`LC.Nav.Top`](gallery:///%22Desktop%22/Components/%22Nav_Top%22) integrated into the gallery

* TopNav family retired to Legacy.TopNav. Use the new [`LC.Nav.Top`](gallery:///%22Desktop%22/Components/%22Nav_Top%22) family.

* Can now link from markdown files into gallery app routes directly, using this kind of URL `gallery:///%22Desktop%22/Components/%22Icons%22`

* Icons docs updated

## 2021-02-04

* Renamed a bunch of components

* Documented the top level libraries in the repo

* The new Nav.Top component is now available. The TopNav that's been in use until now will be retired into the Legacy namespace shortly.

## 2021-01-26

* We removed the `LC.Form.Input.*` series of wrapper components, and instead makes the `Validity` field on the regular `LC.Input.*` components mandatory. It should be set to `form.FieldValidity Field.YourField`, where previously you were passing `Form='form' Field='Field.YourField'` to the wrapper component. In the rare cases where you're using an input component outside a form, you can set the validity manually, for example to `Validity='Valid'`. Basically at this point, the wrapper components were adding no value, but were a pain to maintain, since F# records don't support inheritance, and the full list of wrappee props needed to be declared in the wrapper manually.

* LC.Input.LocalTime component added

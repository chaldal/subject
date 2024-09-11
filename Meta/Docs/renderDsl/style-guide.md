# RenderDSL Style Guide

* For tags that extend beyond a reasonable line length (100 or so characters), or have more than two attributes, attributes should be broken out
  one per line, indented one space from the tag open angled bracket, like so:
  ```xml
      <SomeComponent
       Thing='theThing'
       AnotherThing='42'
       ThirdThing='true'/>
  ```

* Organize attributes sensibly. All `rt-` attributes should be grouped together, with more specific ones
  appearing below less-specific ones (i.e. `rt-if` should come before all others, `rt-let` after all others,
  and `rt-map` and frieds somewhere in between. Keep `class` and `rt-class` at the top of the attribute list —
  when working with styles, you should at a glance be able to know which class the element has, and not have
  to search all the attributes for it. Finally, all component-specific props should come last.

* In both `.typext.fs` files where you define `type Props`, and in `.render` files where you instantiate the
  component, order props from "most siginificant" to "least significant". For example, if a component
  `TicketRow` has a prop `Ticket` and a prop `CanDelete`, the `Ticket` prop is of primary importance with regards
  to what the component represent, while `CanDelete` is a secondary detail. So the `Ticket` prop should be listed
  first, and `CanDelete` after that.

  Obviously relative importance of many props will not be clear cut — if that's the case, don't waste the time,
  the gain in clarity will be miniscule. This guideline is for cases when the difference is clear.

* It is often convenient to declare component actions in a curried format, taking the event as the last
  parameter, e.g.
  ```fsharp
    member __.OnItemPress (pressedItemLevel: int) (pressedItemIndex: int) (item: ConfigNodeData) (e: Event)
  ```
  this allows to pass a bound version to a downstream handler in a very convenient manner:
  ```
    onPress='actions.OnItemPress props.Level props.Index nodeData'
  ```
  so the downstream handler completes the call by providing the last `e: Event` parameter.

* Do not use hardcoded color values other than for rare ad hoc visual design purposes. Colours should live
  in the `Colors.fs` file, and should be named semantically in all but a small set of ad hoc cases.

* There are two types of actions which have corresponding naming conventions.
  First, there's the "do something" type of action. The expectation is that when you call it,
  "something" will be "done". What will happen upon calling is clear from the name.
  The name should thus be a verb phrase describing what will be done, for example "Preview" or
  "Save" or "OpenDialog" or "DoSomething". The second type is an "on event" type of action.
  The expectation is that when you call it, the event will be somehow handled, but it is not clear how.
  Typically this type of action is used when "there's more to do than just simply 'doing something'".
  For example, something may be done conditionally, or may be throttled, etc.
  The name should thus be of the form "On" + eventName, for example "OnPress" or "OnExpand".
  An important point is that the "eventName" part has already happened, not "will happen as a result of calling this action".
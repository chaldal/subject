# Roadmap

**This is very outdated and needs reivew**

More of a "future work" than a "roadmap" at this point.

## Platform Fundamentals

* packaging for mobile

* switch to F# 5 and the new string interpolation syntax

* upgrade to Fable 3

* switch the `eggshell` toolchain to dotnet

* Anything to do with Subjects needs to be moved out of LibClient, so as not to weigh down the app bundles
  that don't use the Subject stack. We have a LibLifeCycleUi that was meant to be for that, but somewhere
  along the way Anton screwed up and didn't cleanly delineate library boundaries.

* transition away from deployment target based config overrides, to a first class support for env variable
  based configuration

## Higher Level Abstractions

* develop the Executor pattern to its logical conclusion. This is fairly important, it's what allows
  us to fully automate in progress and error states.

* Finish implementing LibAutoUi. Since last we looked at it, we've learned a lot about building
  input forms, have developed a handful of component to support basic types, so now just one
  more major iteration on LibAutoUi should make it usable (estimated 1~2 weeks of work)


## Tooling

* PowerCycle is broken, it doesn't work for library components, only current app components
* RenderDsl VSCode extension is broken, it doesn't work for namespaced components
* error mapping for RenderDsl. As part of this, we'll need to replace the standard .net XML parser
  that we are currently using, because it does not provide us with line number information.
* make snippets for all input types
* `eggshell rename-prop` would be really nice. Unfortunately it's not possible to implement this
  using XSLT, since the .render files are meant for human consumption, and thus cannot be randomly
  reformatted by the XSLT processor, which happens with quotes, whitespace, CDATA sections and more.
  So this will have to wait until we deal with error mapping, at which point we'll have to build an
  XML processor that is aware of the underlying text, and using that to implement prop rename and
  similar features.

## Conveniences

* allow to have < and > and & inside XML attributes — this will go against the XML spec, but will
  make it much easier to work with types, particularly as used in `rt-prop`, and boolean expressions.

* fsproj component registration autogeneration
    * auto-generating a standalone components file, and requiring that from fsproj
      would be the ideal way to do this. Can also automate ordering, but that would
      require parsing of all components at once and running an algo on the
      dependency graph.

* enforce single quotes for RenderDSL attributes (and either exclude `rt-type-parameters`, or
  allow to escape single quotes somehow)

* consider a possible design for type-safe class names



## Gallery Improvements

* Build a "let's build a todo app" tutorial
* third party component wrapping docs
* finish eggshell CLI docs


## Component Library Improvements

* in LC.Input.Picker, selected item should be automatically scrolled into view
* input components need to support a `ShouldFocusOnMount` prop
* navigating back needs to restore route state and scroll state


## Unsorted TODOs

* mutually recursive components. Eventually somebody will want to build
  a TreeView component.

* AppRenderDslCompiler implementation issues
    * clean up the RtCase situation
    * use inheritance for errors
    * there's parsing happening in both Parsing.fs and CodeGeneration.fs, which isn't the cleanest
      way to split code. Should do full all parsing in Parsing.fs, and code generation should be
      mostly free of errors.

* Given that we pass classes around as injected props, we could actually pass them in the
  object format (`Set<string>`), not as serialized string, since that's how they are consumed
  in the case of ReactXP. This would require overwriting the shallow equality based shouldComponentUpdate,
  which isn't something I want to deal with at the moment.


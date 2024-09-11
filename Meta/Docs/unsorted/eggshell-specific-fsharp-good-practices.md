# EggShell-Specific F# Good Coding Practices

EggShell F# code gets compiled with Fable into JavaScript, so even though we are
writing F# code, the runtime is not dotnet, so there are some Fable-specific
coding practices that we need to adapt.

* Do not use `Async.Start`. You'll get a Fable warning instructing you to use `Async.StartImmediate`.
  Do not use `Async.StartImmediate`. It swallows errors silently. Instead, use our wrapper, `startSafely`,
  which starts an async and logs errors on failure. You'll need to `open LibClient` to make `startSafely`
  available.

* When you have some subject/entity, say Dish, It's often natural to have a component that visualizes it
  to be named exactly the same, so that you can do <Dish Dish='dish'/> to render it. In these cases,
	having the component's class be named the same as the type leads to annoying clobbering issues in the
	generated .Render.fs file if you add your `Types` module into `additionalModulesToOpen` in `eggshell.json`,
	since the component module is `open`'ed after all the `additionalModulesToOpen` are. To remedy that,
	let's make a convention of prefixing the offending component class with `Component_`, so `type Component_Dish`
	in this example. This doesn't change any of the use side code in the .render file, but solves the problem.

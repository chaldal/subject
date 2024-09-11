# FAQ, aka "How do I do this in EggShell?"

Here we'll have answers to common questions.

## Why is there so much documentation?

Because currently EggShell is full of leaky abstractions. I.e. to use the system well,
you, regrettably, need to understand a few things about its internals. And the more layers
a system with leaky abstractions has, the more there is to understand.

For example, Raw JavaScript is hard enough to understand — there's the browser runtime,
Node runtime, various gotchas about mutation, weird things you need to remember when
iterating over fields, awkward prototype issues, etc. Add TypeScript to the mix, and now you
need to understand both TypeScript peculiarities, _and_ how TypeScript compiles down to raw JS,
and what can go wrong in the process. Add Webpack to the mix, and now you need to be aware of how
code gets packaged, and what can go wrong there. The more layers there are, the more there is to
know. This is far less true about watertight abstractions. React, for example, is pretty good —
it's fairly easy to just stay in the React world and not need to know much about the underlying,
non-virtual DOM (unless you're mixing JQuery in and doing non-idiomatic things).

So, similarly, with EggShell, you have F#, converted into JS through Fable, and the conversion has
issues. You have ReactXP wrapping over ReactNative and ReactDOM, and any wrapper over such two
vastly different runtimes is bound to have its issues. And then you have an additional
layer that we built on top of the whole thing, with the goal of removing low level UI building,
and moving it to a higher level with all the good defaults and other benefits. But because the
platform is still in development, and we're still figuring many things out, the abstractions are
not watertight, they are leaky, and the early adopter is forced to know a lot more to use the system
well than a later adopter may a year or two down the road, when we've plugged a subset of the leaks.


## How do I make text searchable in the browser.

In ReactXP, searchability is controlled at the same time as selectability, so
`<uitext>blah</uitext>` will be neither selectable nor searchable, while
`<uitext selectable='true'>blah</uitext>` will be both. This is rather unfortunate,
because the whole point of "uitext" was that it's supposed to be "text that's not
selectable, because e.g. why would you want to select a button label? But it's certainly
reasonable to want to search a button label. Oh well.


## How do I make a new component/route/dialog?

Use the [`eggshell` CLI](./tools/cli.md)


## How do I build an admin panel?

Depends on exactly what you want. Other than standard UI components, we have some
admin panel spcecific ones: [LA.Grid](gallery:///%22Desktop%22/Components/%22Grid%22)
and [LA.QueryGrid](gallery:///%22Desktop%22/Components/%22QueryGrid%22). That should get
you started. If you go with `QueryGrid`, you'll need to get an overview of our [forms infra](gallery:///%22Desktop%22/Components/%22Forms%22),
so that you can build the input form for the query.

If it's an admin panel for a Subject based backend, you'll need to simply extend
the `SubjectService` with your concrete subject instances, and use that to get data.
If it's an ad hoc HTTP based service, you'll need to implement an ad hoc service.

# Snippets

Scaffolding helps us solve the problem of having to type out from scratch, or
copy, paste, and clean up example code, whenever we want to use a repeated coding
pattern. As an aside, it's probably worth noting that such repeated patterns are
not necessarily an indication that a better abstraction is needed — often enough
these patterns are of essential use site code, i.e. code that you cannot further
simplify.

The `eggshell` CLI provides a way to deal with common code patterns at the macro level —
scaffolding multiple files required to build a component, or even tens of files required
to get a whole app going.

On the micro level, VSCode's [snippet](https://code.visualstudio.com/docs/editor/userdefinedsnippets)
functionality is very helpful. It essentially allows you to define a parametrized chunk of text,
and bind it to a trigger string, called a "prefix" in VSCode lingo, so that when you enter the
prefix and hit tab, it gets unfolded into the desired chunk of text.

Snippets can specify a "scope" language, and VSCode will make them available only when editing
files in that language.

We define a set of EggShell specific snippets in `Meta/renderdsl.code-snippets`, and symlink them
into all EggShell projects.

There are probably multiple ways to configure triggering of snippets, depending on your preferences
for other IDE features. One way that seems to work well is to set this setting

```
"editor.tabCompletion": "onlySnippets",
```

in your `settings.json` file (which you can open by going to View > Command Palette and typing
"open settings json"). Then typing, for example, `rtm` and hitting tab expands it to the `<rt-match>`
snippet.

Get used to using these snippets, and they will greatly increase your development speed. Below
are the snippets that are currently available.
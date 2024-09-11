# Developer Experience

The less time and brain cycles you spend on structural tasks and waiting,
the easier it is for you to maintain momentum and remain highly productive.
So we aim to have top notch tooling, trying to approach "the computer reads
my mind and helps me in real-time" as close as possible, adjusted for ROI.

Any annoyances in the dev experience shouldn't be "got used to", but instead
should be fixed, ROI permitting. So if you have some pet peeves, or some ideas,
hopefully we can get them implemented.

## NOTE for Mac users

If you are experiencing a stack overflow error when running `eggshell dev-web`, you
may have luck running `export COMPlus_DefaultStackSize=180000` before it.


## Put the Docs at Your Fingertips

Bookmark the gallery so that the docs are always at your fingertips.

## Multi-line Editing

If you're not already, it may be a good idea to get used to multi-line editing,
as it greatly increases coding productivity. This is because when editing code we
frequently think of changing multiple lines in similar/related ways, yet the default
editing experience is single-cursor based, so you are forced to serialize actions that
your brain originally conceived as parallel/related, losing much time to mechanical
editing.


## Stubbing with QQQ

There's a special value implemented in `LibLang`, `QQQ`, which simply throws a
"not implmented" error, but is type-compatible with pretty much anything. This lets
you stub functions with correct return types elegantly:

```fsharp
let doSomething() : SomeType =
    QQQ
```

You can now call `doSomething`, everything type-checks great, and you don't run the risk
of returning fake values like `-1` and forgetting to actually implement later.

## VScode

VSCode has a concept of workspaces, which includes configuration like which directories to make available
for the "open anything" dialog and search/replace, which files to ignore, etc. We keep these .code-workspace
files in the repo. When launching vscode from command line, you can specify the .code-workspace file for it
to open. If you just launch if from a directory that happens to have a workspace file, it'll give you a choice
of whether to simply open the directory, or also load the workspace settings.

You should not generally open VSCode at the root of the repo. Many things about the dev experience will break.
Ionide hates it, for example. So just open the workspace file. All the linked projects should be available
for your consumption anyway. And if you want to open a different app, just open a new VSCode window.

## VSCode Extensions

Please install and use the following extensions:

### Ionide-fsharp
This helps you with the F# intellisense, definition and all the goodies.
NOTE: You will need to install .Net Sdk 5.0 or what ever the latest version of the plugin requires.

**Known Issues**:
- If something is not going well, try going to View > Output, and selecting F# (and possibly msbuild) from the dropdown.

- For the latest (as of 2021-01-27) Ionide, you need to have the dotnet 5 SDK installed.
  It's used by the tool internally, you can still have projects with lower SDK versions built on it.
  Unless the SDK is installed, Ionide just silently chokes. For WSL users, it seems that the SDK needs to be
  installed in WSL (as opposed to the Windows side). Instructions are [here](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu).
  If the apt-get commands on this page are giving you a `http://archive.ubuntu.com/ubuntu focal-backports InRelease Temporary failure resolving 'archive.ubuntu.com'"`,
  then use the following command to fix the DNS: `echo nameserver 8.8.8.8" | sudo tee /etc/resolv.conf > /dev/null`

- Make sure you don't have special characters (e.g. `#`) in the absolute path of your project. [related github issue](https://github.com/ionide/ionide-vscode-fsharp/issues/1151)

- If you cross the limit of [inotify](https://man7.org/linux/man-pages/man7/inotify.7.html) watches,
  try increasing the limit. See [this](https://stackoverflow.com/questions/43469400/asp-net-core-the-configured-user-limit-128-on-the-number-of-inotify-instance)
  for reference. On Ubuntu you can run `echo fs.inotify.max_user_instances=10240 | sudo tee -a /etc/sysctl.conf && sudo sysctl -p`.
  This will increase the limit to 10240 from the default 128.

- Sometimes rebuilding the project helps. [related github isssues](https://github.com/ionide/ionide-vscode-fsharp/search?q=obj&type=issues)
  - Delete `obj` and `bin` directory, close VSCode and then run again. It should work fine now.


Be aware that this extension does a lot of processing, so you might want to
keep your workspace small.
You might want to create and save a VS Code workspace for that matter.
Development environment specific stuff (e.g. .vscode, .sln) are usually tracked in the repository.

### Trailing Spaces

Highlights any trailing whitespace in red, letting you keep the code clean.
You'll probably want to set `"trailing-spaces.highlightCurrentLine": false` in your settings.json.

### Whitespace Xray Vision

This isn't an extension, but a config setting.
Getting whitespace consistent is a must for a neat codebase. Having whitespace characters
always shown in the editor is visually really noisy and distracting — we have better things
to occupy our brains with. There's a way to minimize visual noise yet keep the ability to stay
on top of white space — set it to be always displayed, yet set the colour to match the background
colour of your theme. That way, only when you select the lines will you get to see the characters,
it looks like X-ray vision.

In your VSCode's settings.json file, add

```
    "editor.renderWhitespace": "all",
    "workbench.colorCustomizations": {
        "editorWhitespace.foreground": "#1d1d1e" // <-- theme's background color
    },
```


### awarest-align

For those who don't enjoy the medative experience of aligning code by hand, this
plugin is great. By default it comes with no key bindings. I found that this binding
works great for our style of aligning F# records and style rules in `.style.fs` files.


```
    {
        "key":     "ctrl+space",
        "command": "extension.awarestAlignEach",
        "args":    " "
    }
```
Important note : Do not do this from keyboard shortcuts

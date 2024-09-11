# Directory Structure

## Repo level

Libraries are described [separately](./basics/libraries.md).

Which projects may be good examples to follow is also described [separately](./how-to/projects.md).

Detailed information about the format of the `eggshell.json` config file is [here](./tools/cli.md).

```sh
.
├── AppEggShellGallery         # The EggShell gallery app, where you can see components,
│                              # docs, etc for EggShell
│
├── Directory.Build.props      # dotnet build configuration for all projects in this repo
│
├── Fake                       # the Fake-based build system used in prod builds of EggShell apps
│
├── Lib.*                      # see the libraries link above
│
├── Meta                       # tooling, docs, snippets, etc
│   │
│   ├── AppEggshellCli                    # the eggshell CLI code
│   │
│   ├── AppRenderDslCompiler              # compiler for the .render files
│   │
│   ├── AppRenderDslServer                # LSP server for RenderDsl, i.e. the thing that powers
│   │                                     # the VSCode RenderDsl extension
│   │
│   ├── Docs                              # docs, how-tos, etc
│   │
│   ├── LibEggshell                       # part of eggshell CLI
│   │
│   ├── LibRenderDSL                      # code shared between RenderDsl Server and Compiler
│   │
│   ├── LibRtCompilerFileSystemBindings   # part of eggshell CLI
│   │
│   ├── LibScaffolding                    # part of eggshell CLI
│   │
│   ├── Templates                         # templates for the Subject stack's scaffolding needs
│   │
│   ├── eggshell.js                       # almost the eggshell CLI entry point
│   │
│   ├── fsharplint.json                   # shared fsharplint config (though fsharplint is pretty useless)
│   │
│   └── renderdsl.code-snippets           # shared VSCode snippets, symlinked in individual apps
│
├── ThirdParty              # third party JS code wrapped for consumption within EggShell
│
├── build.fsx               # top level build file used by the Fake-based build system
│
├── eggshell                # the eggshell CLI entry point
│
├── eggshell.json           # top level eggshell.json config file. See link above.
│
├── initialize              # top level initialize script for the repo
│
└── test-build-all-apps.sh  # poor man's script to build all apps to see if refactorings in libraries
                            # broke anything in leaf node apps.
```

## App level

When you scaffold a new app, you get:

```sh
.
├── App.code-workspace        # VSCode workspace file (where you configure what other directories to
│                             # include in search/open/jump-to-definition etc. If you reference a new
│                             # project in a different directory, need to add here.
│
├── AppSample.sln             # dotnet solution file (used by Rider, and by Ionide in VSCode). If you
│                             # reference a new project in a different directory, need to add here.
│
├── README.md                 # a readme file that hopefully somebody keeps up to date with useful info
│                             # about getting up and running with the current app
│
├── clean                     # script to remove all (most?) git-ignored resources and artifacts
│
├── config.base.js            # the base config values (structure defined in src/Config.ts)
│
├── config.overrides.dev.js   # config overrides for dev
│
├── config.overrides.prod.js  # config overrides for prod, except they are not used, since we
│                             # configure using per-process environment variables. There's a TODO
│                             # for redoing this
│
├── config.overrides.test.js  # likewise for test
│
├── dist-template             # the template for the distribution bundle, i.e. the result of running
│   │                         # `eggshell package-app`
│   │
│   ├── package.json.template # package.json for the server.js
│   │
│   ├── public                # this is where actual build artifacts will go
│   │
│   ├── run.sh.template       # shell script to start server.js
│   │
│   └── server.js.template    # template for server.js
│
├── eggshell.json             # EggShell config for this app — see link above for full details
│
├── fsharplint.json           # fslint config (symlinks to Meta)
│
├── images                    # images go here
│   │
│   └── placeholder-lest-empty-directory-does-not-get-created
│
├── initialize                # initialize script for the app (installs npm packages, symlinks a few thigns)
│
├── lint                      # runs fsharplint
│
├── node_modules              # npm modules
│
├── package-lock.json
│
├── package.json
│
├── public-dev                # the public directory used for development. This is where webpack will
│   │                         # server static content from
│   │
│   ├── app.css                        # default style rules
│   │
│   ├── config.base.js                 # symlinked to the root one
│   │
│   ├── config.overrides.target.js     # symlinked to config.overrides.dev.js
│   │
│   ├── fable.ico                      # favico
│   │
│   ├── images                         # symlinked to images
│   │
│   ├── index.html                     # the index.html served by webpack during development
│   │
│   └── launch.js                      # launch script for the app
│
├── src
│   │
│   ├── Actions.fs                 # a place to put global actions (e.g. addItemToShoppingCart)
│   │
│   ├── App.fsproj                 # the F# project file for this app
│   │
│   ├── Bootstrap.fs               # this is where the app gets bootstrapped, launch.js calls into top level
│   │                              # function declared in this file
│   │
│   ├── Colors.fs                  # color scheme definition for your app
│   │
│   ├── ComponentRegistration.fs   # auto-generated file that links components to their render functions and styles
│   │
│   ├── Components                 # this is where individual components scaffolded by `eggshell create-component` go
│   │
│   ├── Components.proj            # subproject where we list component .fs files. Maintained by hand, since
│   │                              # order of declaration is important, and we don't yet scrape the order from
│   │                              # .render files. We could, but the priority of doing this is pretty low.
│   │
│   ├── ComponentsTheme.fs         # app-global components visual theme (e.g. all buttons should be green)
│   │
│   ├── Config.fs                  # app config type definition
│   │
│   ├── DialogsImplementation.fs   # providing dialog implementation (split in two to avoid circular dependencies)
│   │
│   ├── DialogsInterface.fs        # defining what dialogs are available
│   │
│   ├── Icons.fs                   # app-specific icons (as opposed to global ones in LibClient)
│   │
│   ├── Navigation.fs              # here we define routes for the app
│   │
│   ├── Services                   # services, usually one per entity type, are what give us access to
│   │                              # remote data on the backend
│   │
│   ├── Services.fs                # top level directory of services defined here
│   │
│   └── Services.proj              # subproject for defining services
│
├── webpack.config.build.js   # webpack config used during `eggshell package-app`
│
└── webpack.config.js         # webpack config used during development
```
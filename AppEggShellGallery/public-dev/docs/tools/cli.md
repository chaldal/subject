# The `eggshell` Command Line Tool

We use the `eggshell` command to scaffold new apps, components, routes, dialogs,
to rename existing components and thus keep the codebase clean and organized,
to run the watch-for-changes-and-recompile development toolchain, and to package
the app for deployment.

Currently the `eggshell` CLI commands do not take command line arguments, instead
collecting their data in an interactive fashion. We're planning to support both
in the future.

## `eggshell create-app`

Scaffolds a new application at the root of the repo. Some manual steps may be required,
especially if you need to move it in a level deeper into the hierarchy, like into a suite.
Outputs the following message when done:

```plaintext
Created app ${appName}

To get started you can:

cd ${appRootPath} #if you haven't already
eggshell dev-web

And then once it's all running, go to http://localhost:8080 in your browser.

If you need to move the app into a non-root directory, here are the files you'll
need to update with additional dot-dot-slashes

    .npmrc
    eggshell.json
    src/App.fsproj
    webpack.config.js
    webpack.config.build.js
    ${appName}.sln
    App.code-workspace
```

## `eggshell dev-web`

Runs the current app in development mode, via webpack, typically on port 8080 unless you changed the value in `webpack.config.js`.
Watches `.render` and `.typext.fs` files, compiles them with the RenderDSL compiler, in both this project and all projects
that are listed in the current project's `eggshell.json`'s `dependenciesToRtCompile` field.

## `eggshell create-component`

Allows you to create a new component, of the specified type (pure stateless, estateful, pstateful,
or function-based). The component name can optionall be namespaced. So instead of doing something
like `CookDishSchedule` you may want to consider `Cook.Dish.Schedule`. There are no fixed rules
for how to group components, and the `rename-component` command relieves some of the pressure of
naming the component perfectly right the first time around.

## `eggshell rename-component`

Just like easy renaming of values, functions, modules, and types is necessary to maintain
a clean codebase in a general language, easily renaming components is fundamental to maintaining
a clean component library or front end app.

This command allows to rename components, including across namespaces, within the same eggshell
project. If you need to move out a component into a different library, that has to be done manually.
(it's not rocket science or anything, just need to move some files, update the references, rename
modules and namespaces).

This command does not currently update the use sites, as that's the relatively easy task that can be
accomplished by a simple global replace in your IDE.

## `eggshell create-route`

TODO

## `eggshell create-dialog`

TODO

## `eggshell create-third-party-wrapper`

TODO

## `eggshell package-app`

TODO

## `eggshell build-lib`

TODO

## The `eggshell.json` config file

Each eggshell project, including the repo root "metaproject", are configured using
the `eggshell.json` configuration file. It has the following structure:

```typescript
// defined in /Meta/LibEggshell/src/index.ts
// Seq here is an alias for immutable array.

export interface RootConfig {
    type: 'repoRoot';
}

export interface EggShellProjectConfigCommon {
    name: string;
    render: {
        // Which other projects to compile .render and .typext.fs files in, and watch
        // for changes during `eggshell dev-web`. Values should be relative paths to the
        // projects' directories.
        dependenciesToRtCompile:      Seq<string> | undefined;

        // These modules will be `open`'ed in the .Render.fs file
        // that is generated for each .render file. Use this for very
        // common modules only; one-off modules required only in some
        // particular components should be opened using `rt-open` on the
        // root XML node of the .render file.
        additionalModulesToOpen:      Seq<string>;

        // Aliases for component libraries. That's where, for example,
        // "LC" is configured to refer to "LibClient.Components".
        componentLibraryAliases:      Seq<[string | 'default', string]>;

        // Namespace to relative path mapping for libraries.
        componentLibraryPaths:        Seq<[string, string]>;

        // Component aliases. This is where, for example,
        // "div" is mapped to "RX.View".
        componentAliases:             Seq<[string, string]>;

        // In rare cases where a project has multiple Components.proj-like files,
        // you can help the rename-component tool by adding the additional files here.
        additionalComponentProjFiles: Seq<string> | undefined;
    }
}

export interface AppConfig extends EggShellProjectConfigCommon {
    type: 'app';
    build: {
        // Source file or glob to destination file or directory mapping.
        // These files will be copied to the destination bundle.
        copyStaticFiles: Seq<[string, string]>;
    }
}

export interface LibraryConfig extends EggShellProjectConfigCommon {
    type: 'library';
}

export type ProjectConfig = AppConfig | LibraryConfig | RootConfig
```

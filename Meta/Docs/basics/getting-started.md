# Getting Started with EggShell

Our official editor is VSCode. We're building extensions and tooling that make
the dev experience great in VSCode, and don't have the resources to support
other editors at the moment. See DEV_EXPERIENCE.md for more.

The tooling only runs smoothly on *nix, so Windows users need some extra setup.


## Windows Setup

### With GitBash
Due to some some scripting+symlink support - we need to use gitbash on windows.
1. Install git and gitbash ( https://git-scm.com/download/win )

      <summary> Make sure to "Enable Symbolic link" </summary>

      1. Enable "Developer Mode" in Windows 10/11 -- gives mklink permissions
      2. Ensure symlinks are enabled in git with (at least) one of
          1. System setting: check the checkbox when installing msysgit
          2. Global setting: git config --global core.symlinks true
          3. Local setting: git config core.symlinks true

      ![Enable Symbolic links](https://eggtestdata.blob.core.windows.net/catalogimages/git_bash.png "Enable Symbolic links")

2. Install nodejs `18.19.0` ( **Recommended:** use [NVM](https://github.com/coreybutler/nvm-windows#install-nvm-windows))
3. Install dotnet [6.0.300](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.300-windows-x64-installer)
4. (Only for native app development) [Setting up the development environment]()./native/getting-started.md)
5. Clone the EggShell repo
6. Follow setps in `Getting a Sample App Running` below, in the "Gitbash terminal"


## Getting a Sample App Running

After cloning the repo, make sure that

* you have dotnet SDK 6.0.300 installed (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
)
* you have node `18.19.0`

Add the path to the EggShell repo to your `$PATH`, by adding something like this to your
`.bash_profile`, or wherever else you happen to manage your `$PATH`:

```plaintext
export PATH=/Users/anton/Workspace/repos/EggShell:$PATH
```

This gives you access to the `eggshell` executable, which is a helper wrapper to our
scaffolding and dev tools, though you need to build them first. While you're at it,
you may want to grab tab completions for `eggshell` by adding this to your `.bash_profile`
or `.bashrc`:

```plaintext
source /Users/anton/Workspace/EggShell/Meta/AppEggshellCli/bash.completions
```

Now, at the root of the Egg.Shell repo, run

```plaintext
./initialize
```

Now the tooling is ready (it's npm-linked in other projects). Let's make a new app and
check it out in the browser.

```plaintext
eggshell create-app
```

If you enter `Sample` for the name, the directory of the new app will be conveniently
gitignored. You'll then be told to

### Running Web App
```plaintext
cd AppSample
eggshell dev-web
```

Now go to your browser, and open http://localhost:9080/#/home

The app you see is mobile-centric, so until we have a better UI kit, it's best viewed
in device emulation mode in Chrome Dev Tools.

### Running Native App

Go to [Running Native App](./basics/native.md)

## Edit-save-reload

Now, let's edit a file.

In VSCode, open up `RouteHome.render`, and on line 4 change `Welcome to AppSample` to
`Hello World`.

The browser should reload with the new copy visible in the top nav.

**Caveat**: The file watcher in eggshell has some issues. So you might want to kill the
eggshell program then and run it again. One confirmed instance of this problem is when
you checkout some commit in git and there are new files (or removal of files), that would
be a good time to restart the eggshell program.


## Getting an Existing App Running

Please follow the steps in [Getting a Sample App Running](#getting-a-sample-app-running).
However, instead of `eggshell create-app`, in your selected application directory run
`./initialize`. This will set up the required npm link to the `render-dsl-compiler` binary.

<small>NB: `./initialize` is run by `eggshell create-app` but not `eggshell web-dev` in order to
not slow down normal development</small>

## Next steps

Now that you got started successfully, you can build your app. You'll probably want to
add some new components, routes, dialogs, wrappers for third party
npm libraries, etc. See the [eggshell CLI docs](./tools/cli.md) to help get these
scaffolded. You should probably also get acquainted with the
[directory structure](./unsorted/directory-structure.md). And for any other "how do I..?"
questions, head over to the [How To](./how-to/index.md) section.

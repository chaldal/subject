# AppEggShellGallery

Contains documentation and a control gallery for EggShell. Hosted at https://eggshell.dev/.

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 2.1 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)
* An F# editor like Visual Studio, Visual Studio Code with [Ionide](http://ionide.io/) or [JetBrains Rider](https://www.jetbrains.com/rider/).

## Building and running the app

* If you scaffolding the app using `eggshell create-app` then running `eggshell dev-web` should
  get you going, you should be able to see the app running at http://localhost:8080/
* If something fails to compile, the first thing to try is to re-run `./initialize`, and then
  try `eggshell dev-web` again.

Any modification you do to the F# code will be reflected in the web page after saving.
Any modifications to the `.render` files will also be reflected.

## Scraping

Component properties shown at the top of each component's page are extracted from the code and
placed into `AppEggshellGallery/src/ScrapedData.fs`. This scraping process is triggered manually
by developers, and should be performed whenever a component is added or an existing component's
properties are changed. There's no harm in running it "just in case".

Assuming you've already built the gallery itself, you can run the scraper like this:

```
cd $ROOT_OF_REPO
cd AppEggShellGallery/Scraping
dotnet run
```

Be sure to include the updated `ScrapedData.fs` in your pull request.

## Releasing

Releases are triggered whenever the `releases/gallery` branch is updated. To kick off the process,
you can do the following:

```
cd $ROOT_OF_REPO
git checkout master
git pull
git checkout releases/gallery
git merge master
git push
```

In 15 minutes or so, you should see the updates at https://eggshell.dev/. 
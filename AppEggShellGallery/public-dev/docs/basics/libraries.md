# EggShell Libraries

There's a handful of libraries in the repo. Let's go through what they are. Ones not covered here are not directly related to EggShell.

## LibClient

This is the "main" EggShell library that contains:

* Our wrapper over the Fable wrapper over raw React
* Abstract classes for services (services are essentially accessors for data, typically one per high level entity, e.g. DishService, or DateService)
* Basic services, including:
** DateService (you should never use System.DateTimeOffset.Now directly)
** HttpService for making raw HTTP requests, and ThothEncodedHttpService to use with F# typed backends
** SubjectService to interface with the Subject stack
** LocalStorageService to interface with browser/native local storage
** ComponentSettingsService for easily remembering user preferences at a component level
** AudioService for audio playback
** PageTitleService for changing the page title
* Abstract classes for building Apps and Dialogs
* The design-time DSL and runtime code for the styles system
* Typed wrappers over raw ReactXP components
* The vast majority of our standard UI components (buttons, form input fields, cards, etc)
* Logging setup, including ApplicationInsights bindings
* Icons system
* Chars system (replacement for HTML entities, since we are not in HTML world)
* Responsive setup (handheld vs desktop)
* Dialogs infra
* Context menus infra
* Colours infra
* Default component theme

## LibRouter

This is our type-safe wrapper over ReactRouter.

## LibAutoUi

A library to automatically generate an input form for any type 'T. Essentially you should be able to say
something like `<AutoUI.Form Type='typeof MyEntity' OnChange='actions.OnChange'/>` and that would give you
a form with all the fields necessary to input an instance of `MyEntity`. It supports things like pre-filled
fields, custom validation, etc.

The library got to about 80% completion, and then priorities changed and we abandoned development. There is
currently about two weeks of work remaining.

## LibLangFsharp

Here we keep all extensions to the F# standard libraries, like additional function, name aliases, new data structures, etc.

## LibLangTypeScript

Here we keep all extensions to TypeScript. Most of this is inherited from Yumscroll and can thus be somewhat
outdated. It made no sense to invest in learning what commonly accepted replacement for, say, a custom built
Option or Result type is these days, since TypeScript is used only for the `eggshell` CLI, which we are hoping
to eventually migrate to F# and the `dotnet` toolchain anyway.

## LibNode

Shared code useful for building any node-based application.


## LibService

Some sharable code for building backend services. Most of what's in here should probably be considered legacy,
superceded by the Subject stack.


## LibUiSubject

Components for working with Subjects.


## LibUiAdmin

UI components for building admin panels.


## LibUiSubjectAdmin

Subject specific admin panel UI components.


## LibLifeCycle.*

These are all parts of the Subject stack.

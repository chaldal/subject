# Fake Build

- The Egg.Shell side of the stack (frontend) is built using F# interactive with Fake (a build system) library.


- Note that we discontinued using FAKE runner (it's kind of out of support starting from net7) 
and instead consume [Fake lib via F# Interactive](https://fake.build/guide/getting-started.html#Run-FAKE-using-F-interactive-FSI)

  
- We don't use Fake as a project although it's the [increasingly adopted](https://fake.build/guide/getting-started.html#Run-FAKE-using-a-dedicated-build-project) 
way in community. This would require to have dedicated build.fsproj paired to each lib and app. 
Maybe we'll resort to it in the end because F# Interactive can be a pain too, especially with many dispersed scripts like we have


- Why not just use `dotnet build` for frontend like we do for the backend? Because frontend builds require a lot of scripting
which would be painful to do via .fsproj.


- Technically, we can also build the backend with fake, so maybe we should
do that unified build script and plug it into CI, already? Probably backend does not need to use Fake in any form. 
Backend is simpler and usual `dotnet build` is enough. Also since we want to nugetize the Subjcet stack there won't be
a need to build both backend and frontend from sources at the same time.

# Build.fsproj

The Build.fsproj file in this directory pulls together all build scripts across
the entire codebase into one project, so we can do build system refactors easily.

TODO: Ideally each Suite should define a FakeBuild.proj file, that
Build.fsproj simply pulls in; that will avoid the need to link deeply into each
Suite.

# IMPORANT: Two modes of Exection

Having a successful `dotnet build` execute is no guarantee that your F# Fake
run will succeed, as that uses a different execution mechanism.

And vice versa.

So you have to test both ways:
1. dotnet build will ensure that you have no type
   errors anywhere in the project. This is mostly useful when you're doing large
   refactors.

1. dotnet fsi build.fsx of an app or a project will do the actual build execution. However this succeeding doesn't
   guarantee dotnet build will work, as files need to be ordered correctly in the project
   file, and the #load directives need to be hidden from the compiler.

NOTE: while we have only fsx files here, to the compiler they're just as good as any other
.fs file - as long as #load and #r directives are #if'ed away by preprocessor

## This strange setup ..

This is a tiresome exercise in constraint solving, so a lot of things
won't be ideal. In particular, each file that can be executed by Fake
will have some boilerplate on the top.

1. Fake doesn't support multiple linked build files
1. FSI doesn't support large project structures
   - No support for namespaces
   - No way to not load "#r nuget:.." multiple times, if the same file
     is being loaded via multiple paths (e.g. #load Includes.fsx)
1. F# Compiler doesn't like #load and #r directives (so these are hidden from its view)
1. We want all things to be linked via a project, so we can do full-stack refactors
1. Fake/FSI dont like it when file names collide

> Nick: OK since Fake runner is no longer used, we can just use the project right?  

# File ordering in project file

There are two modes of execution:
- When invoked via F# interactive, it will simply function like any other scripting mechanism, i.e. #load 
  files when asked to. So when referencing other files, simply add a #load directive,
  and use them as is.
- When compiling, the order is maintained through the project file, so you need to order
  it correctly.
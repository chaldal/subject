#if !FAKE_BUILD_FSPROJ
#load "../../Fake/Includes.fsx"
#load "../LibEggshell/build-LibEggshell.fsx"
#endif

namespace Egg.Shell.Meta.LibFablePlus

module Build =
    open Fake.IO
    open Egg.Shell.Fake

    type Target =
    | InitializeDependencies
    | Build
    | Clean

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun buildContext normalizePath ->
            function
            | InitializeDependencies ->
                NodeNpm.install buildContext

            | Build ->
                TypeScript.invokeCompiler normalizePath

            | Clean ->
                Shell.rm_rf (normalizePath "./node_modules")
                Shell.rm_rf (normalizePath "./dist")
            )
        |> BuildFile.withDefaultTarget Build

        |> BuildFile.withInternalDependency (InitializeDependencies >=> Build)

        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibEggshell.Build.targetFactory
            (Egg.Shell.Meta.LibEggshell.Build.Target.InitializeDependencies >=> InitializeDependencies)

        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibEggshell.Build.targetFactory
            (Egg.Shell.Meta.LibEggshell.Build.Target.Build >=> Build)

        |> BuildFile.initialize